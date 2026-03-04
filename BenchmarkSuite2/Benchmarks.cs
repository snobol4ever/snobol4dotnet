using BenchmarkDotNet.Attributes;

namespace Snobol4.Benchmarks2;

// ---------------------------------------------------------------------------
// Benchmark 1: ROMAN — recursive function, heavy identifier lookup
//
// Each call to ROMAN(N) executes ~4 SNOBOL4 statements and makes recursive
// calls for each digit. ROMAN('1776') recurses 4 levels deep, exercising
// Identifier(), FunctionName(), Function(), and label-table gotos repeatedly.
// This is the benchmark most sensitive to dictionary lookup overhead.
// ---------------------------------------------------------------------------
[MemoryDiagnoser]
public class RomanBenchmark
{
    private const string Script = @"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
        R2 = ROMAN('9')
        R3 = ROMAN('45')
        R4 = ROMAN('2026')
end
";

    [Benchmark]
    public string Roman_1776() => BenchmarkHelper.RunScript(Script)
        .Execute!.IdentifierTable["R1"].ToString()!;

    [Benchmark]
    public string Roman_AllFour() => BenchmarkHelper.RunScript(Script)
        .Execute!.IdentifierTable["R4"].ToString()!;
}

// ---------------------------------------------------------------------------
// Benchmark 2: ArithmeticLoop — tight counter loop, pure dispatch overhead
//
// A loop that increments a counter 1000 times with no I/O or pattern matching.
// This isolates the per-statement dispatch cost: InitializeStatement,
// Identifier lookups, Operator("__+"), FinalizeStatement, goto resolution.
// The result should be insensitive to what the statements do and almost purely
// reflect interpreter loop overhead.
// ---------------------------------------------------------------------------
[MemoryDiagnoser]
public class ArithmeticLoopBenchmark
{
    private const string Script = @"
        &TRIM = 1
        N = 0
LOOP    N = N + 1
        N = LT(N, 1000) N          :S(LOOP)
        RESULT = N
end
";

    [Benchmark]
    public string ArithLoop_1000() => BenchmarkHelper.RunScript(Script)
        .Execute!.IdentifierTable["RESULT"].ToString()!;
}

// ---------------------------------------------------------------------------
// Benchmark 3: StringPatternLoop — realistic string parsing workload
//
// Parses a CSV-style string 500 times using BREAK pattern matching, appending
// tokens to an accumulator. Exercises: pattern creation, Scanner.PatternMatch,
// string concatenation, Identifier lookups, conditional gotos.
// This represents typical SNOBOL4 "real work" — the kind of program users
// actually write. Most sensitive to pattern matching and string allocation.
// ---------------------------------------------------------------------------
[MemoryDiagnoser]
public class StringPatternLoopBenchmark
{
    private const string Script = @"
        &TRIM = 1
        PAT = BREAK(',') . WORD ','
        ITER = 0
OUTER   ITER = LT(ITER, 500) ITER + 1      :F(DONE)
        S = 'alpha,beta,gamma,delta,epsilon,zeta,eta,theta,iota,kappa,'
        RESULT = ''
INNER   S PAT = ''                          :F(OUTER)
        RESULT = RESULT WORD               :(INNER)
DONE    FINAL = RESULT
end
";

    [Benchmark]
    public string StringPattern_500iters() => BenchmarkHelper.RunScript(Script)
        .Execute!.IdentifierTable["FINAL"].ToString()!;
}
