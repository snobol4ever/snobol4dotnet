using Test.TestLexer;

namespace Test.ObjectCreation;

/// <summary>
/// COPY returns a distinct copy of a pattern, an expression or a code block, as SPITBOL's does ("The argument may be an array, table,
/// code block, pattern, or program-defined data type"): IDENT of the argument and its copy fails, and the copy still matches and runs
/// as the original does. A string, a number or a name is its own copy (IDENT succeeds). dotnet's COPY gave the very pattern, expression
/// or code back, so IDENT succeeded. Found by a probe of the infinite_snobol4 generator's .COPY(X). Every expected line is what sbl -b
/// printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CopyIsDistinct
{
    [TestMethod]
    public void TEST_Copy_of_a_pattern_an_expression_or_a_code_is_distinct()
    {
        var program = @"        P = LEN(1)
        X = 'x'
        N = 3
        R = 2.5
        E = *X
        C = CODE(' Z = 7  :(BACK)')
        M = .X
        OUTPUT = 'P ' (IDENT(P, COPY(P)) 'same', 'differ')
        OUTPUT = 'X ' (IDENT(X, COPY(X)) 'same', 'differ')
        OUTPUT = 'N ' (IDENT(N, COPY(N)) 'same', 'differ')
        OUTPUT = 'R ' (IDENT(R, COPY(R)) 'same', 'differ')
        OUTPUT = 'E ' (IDENT(E, COPY(E)) 'same', 'differ')
        OUTPUT = 'C ' (IDENT(C, COPY(C)) 'same', 'differ')
        OUTPUT = 'M ' (IDENT(M, COPY(M)) 'same', 'differ')
        Q = COPY(P)
        'abc' ? Q . V
        OUTPUT = 'Q matched ' V ' ' DATATYPE(Q)
        OUTPUT = 'E copy ' EVAL(COPY(E))
        D = COPY(C)                                 :<D>
BACK    OUTPUT = 'C copy ran ' Z
END";
        Assert.AreEqual(@"P differ
X same
N same
R same
E differ
C differ
M same
Q matched a PATTERN
E copy x
C copy ran 7", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
