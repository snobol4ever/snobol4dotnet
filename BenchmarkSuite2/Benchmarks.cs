namespace Snobol4.Benchmarks2;

/// <summary>
/// SNOBOL4 benchmark scripts. Each constant is a complete SNOBOL4 program
/// used by Program.cs to measure performance via BenchmarkHelper.Measure().
///
/// Five workload profiles:
///   Roman          — recursive functions, heavy identifier/function dispatch
///   ArithLoop      — pure interpreter dispatch, tight counter loop (5000 iters)
///   StringPattern  — realistic CSV parsing via BREAK pattern (1000 iters)
///   Fibonacci      — deep recursion, FIB(20) ~21k recursive calls
///   StringManip    — REPLACE/SIZE/SUBSTR string operations (2000 iters)
/// </summary>
public static class Scripts
{
    // -------------------------------------------------------------------------
    // Roman — recursive function, heavy identifier lookup and function dispatch.
    // ROMAN('1776') recurses 4 levels, exercising DEFINE, RPOS, LEN, BREAK,
    // REPLACE, label-table gotos. Most sensitive to Roslyn compile overhead
    // and function dispatch cost.
    // -------------------------------------------------------------------------
    public const string Roman = @"
    DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
    '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+   T   BREAK(',') . T                  :F(FRETURN)
    ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                       :S(RETURN)F(FRETURN)
ROMAN_END
    R1 = ROMAN('1776')
    R2 = ROMAN('9')
    R3 = ROMAN('400')
    R4 = ROMAN('2026')
end";

    // -------------------------------------------------------------------------
    // ArithLoop — tight counter loop, pure interpreter dispatch overhead.
    // Increments N from 0 to 5000 with no I/O or pattern matching.
    // Most sensitive to per-statement overhead, arithmetic operators, goto.
    // -------------------------------------------------------------------------
    public const string ArithLoop = @"
    &TRIM = 1
    N = 0
LOOP    N = N + 1
    N = LT(N, 1000) N          :S(LOOP)
    RESULT = N
end";

    public const string StringPattern = @"
    &TRIM = 1
    PAT = BREAK(',') . WORD ','
    ITER = 0
OUTER   ITER = LT(ITER, 200) ITER + 1      :F(DONE)
    S = 'alpha,beta,gamma,delta,epsilon,zeta,eta,theta,iota,kappa,'
    RESULT = ''
INNER   S PAT = ''                          :F(OUTER)
    RESULT = RESULT WORD               :(INNER)
DONE    FINAL = RESULT
end";

    // -------------------------------------------------------------------------
    // Fibonacci — deep recursion stress test.
    // FIB(20) = 6765, requires ~21,891 recursive SNOBOL4 function calls.
    // Most sensitive to function call/return overhead and stack management.
    // -------------------------------------------------------------------------
    public const string Fibonacci = @"
    DEFINE('FIB(N)')                        :(FIB_END)
FIB     FIB = LT(N,2) N                    :S(RETURN)
    FIB = FIB(N - 1) + FIB(N - 2)     :(RETURN)
FIB_END
    RESULT = FIB(18)
end";

    // -------------------------------------------------------------------------
    // StringManip — string operation throughput.
    // Runs REPLACE, SIZE, and SUBSTR on a sentence string 2000 times.
    // Most sensitive to string function dispatch and string allocation.
    // -------------------------------------------------------------------------
    public const string StringManip = @"
    &TRIM = 1
    ITER = 0
LOOP    ITER = LT(ITER, 500) ITER + 1      :F(DONE)
    S = 'The quick brown fox jumps over the lazy dog'
    S = REPLACE(S, 'aeiou', '*****')
    N = SIZE(S)
    S2 = SUBSTR(S, 1, 10) SUBSTR(S, 11, 10)    :(LOOP)
DONE    RESULT = N
end";

    // -------------------------------------------------------------------------
    // VarAccess — identifier lookup bottleneck isolation.
    // Reads and writes 5 distinct variables in a tight loop.
    // Bottleneck: every PushVar does IdentifierTable[symbol] string lookup.
    // csnobol4 ref (1M iters): ~0ms.  Expected RESULT: 12006
    // -------------------------------------------------------------------------
    public const string VarAccess = @"
    A = 1
    B = 2
    C = 0
    D = 0
    E = 0
    N = 0
LOOP    A = A + 1
    B = B + 2
    C = A + B
    D = C + A
    E = D + B
    N = LT(N, 2000) N + 1               :S(LOOP)
    RESULT = E
end";

    public const string OperatorDispatch = @"
    N = 1
    OUTER = 0
OUTER   N = N + 3
    N = N - 1
    N = N * 2
    N = GE(N, 10000000) N / 10000      :S(OUTER)F(NEXT)
NEXT    OUTER = LT(OUTER, 100) OUTER + 1     :S(OUTER)
    RESULT = N
end";

    public const string PatternBacktrack = @"
    &ANCHOR = 0
    PAT = ('aaa' | 'bbb' | 'ccc' | 'ddd') SPAN('abcd') . W
    S = 'xxxxxxxxxxbbbccccddddaaaaxxxxxxxxxxbbbccccddddaaaa'
    N = 0
LOOP    S PAT                           :F(DONE)
    N = LT(N, 500) N + 1                :S(LOOP)
DONE    RESULT = N
end";

    public const string TableAccess = @"
    T = TABLE(512)
    I = 0
FILL    I = LT(I, 500) I + 1           :F(READ)
    T[I] = I * 2                       :(FILL)
READ    SUM = 0
    I = 0
LOOP    I = LT(I, 500) I + 1           :F(DONE)
    SUM = SUM + T[I]                   :(LOOP)
DONE    RESULT = SUM
end";

    public const string FunctionCallOverhead = @"
    DEFINE('INC(N)')                    :(INC_END)
INC INC = N + 1                        :(RETURN)
INC_END
    R = 0
    N = 0
LOOP    N = LT(N, 300) N + 1           :F(DONE)
    R = INC(R)                         :(LOOP)
DONE    RESULT = R
end";

    public const string StringConcat = @"
    S = ''
    N = 0
LOOP    N = LT(N, 100) N + 1           :F(DONE)
    S = S 'x'                          :(LOOP)
DONE    RESULT = SIZE(S)
end";

    public const string MixedWorkload = @"
    DEFINE('RSUM(N)')                   :(RSUM_END)
RSUM    RSUM = EQ(N,0) 0               :S(RETURN)
    RSUM = N + RSUM(N - 1)             :(RETURN)
RSUM_END
    PAT = BREAK(',') . WORD ','
    OUTER = 0
OUTER   T = TABLE(16)
    DATA = '10,20,30,40,50,60,70,80,90,100,'
    IDX = 0
PARSE   DATA PAT =                     :F(COMPUTE)
    IDX = IDX + 1
    T[IDX] = WORD + 0                  :(PARSE)
COMPUTE TOTAL = 0
    I = 0
ADD     I = LT(I, IDX) I + 1          :F(CHECK)
    TOTAL = TOTAL + T[I]               :(ADD)
CHECK   CHECK = RSUM(10)
    OUTER = LT(OUTER, 200) OUTER + 1    :S(OUTER)
    RESULT = TOTAL
end";
}
