using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// An = inside the replacement, outside every bracket, whose target is a concatenation is a replacement of its own, as in SPITBOL: the
/// concatenation's first element is the subject and the rest the pattern, and its value is the subject's new value, so X = A B = 1
/// replaces B in A and X is A then. A literal subject is no name, which is error 212 as the statement is compiled. The console took
/// the concatenation as an assignment's target and stopped the run with error 212 (found probing the CONTINUE road). Every
/// expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ReplacementInAReplacement
{
    [TestMethod]
    public void TEST_ReplacementInAReplacement_replaces_in_its_subject()
    {
        Assert.AreEqual("X=[x1z] A=[x1z]\nX=[x2] A=[x2]\nX=[xa3cz] A=[xa3cz] C=[a3c]\nA=[xa5cz] C=[a5c]\nX=[pQr] T1=[pQr]\nX=[xxyz] A=[xxyz]\n" +
                        "fail X=[xxyz] A=[xyz]", SetupTests.RunWithInput(@"        A = 'xyz'
        X = A 'y' = 1
        OUTPUT = 'X=[' X '] A=[' A ']'
        A = 'xyz'
        X = A 'y' 'z' = 2
        OUTPUT = 'X=[' X '] A=[' A ']'
        A = 'xyz'
        C = 'abc'
        X = A 'y' = C 'b' = 3
        OUTPUT = 'X=[' X '] A=[' A '] C=[' C ']'
        A = 'xyz'
        C = 'abc'
        A 'y' = C 'b' = 5
        OUTPUT = 'A=[' A '] C=[' C ']'
        T = TABLE()
        T[1] = 'pqr'
        X = T[1] 'q' = 'Q'
        OUTPUT = 'X=[' X '] T1=[' T[1] ']'
        A = 'xyz'
        X = A LEN(1) . V = V V
        OUTPUT = 'X=[' X '] A=[' A ']'
        A = 'xyz'
        X = A 'q' = 1                       :S(S1)F(F1)
S1      OUTPUT = 'succ'                     :(END)
F1      OUTPUT = 'fail X=[' X '] A=[' A ']'
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_ReplacementInAReplacement_a_literal_subject_is_212_as_it_is_compiled()
    {
        // OUTPUT = 'A' C = 1 is 212 in the listing and the program does not run; in parentheses the concatenation is no match, and its
        // = an assignment to a value, 212 as before
        var build = SetupTests.SetupScript("-b", @"        X = 'ran'
        OUTPUT = 'A' C = 1
        X = Y = ('A' C = 1)
        X = A B = C D = 1
END");
        CollectionAssert.AreEqual(new[] { 212, 212 }, build.ErrorCodeHistory.ToArray());
        Assert.AreEqual(231, build.ExitStatus);
    }
}
