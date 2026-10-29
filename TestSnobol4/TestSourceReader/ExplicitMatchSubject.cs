using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A statement whose body holds a binary ? outside every bracket is that one match: everything before the ? is the subject, as in
/// SPITBOL, where ? is the operator of least precedence: A B ? 'z' matches A B, and A B ? @X sets X to 0. The console took the first
/// element for the subject, as a statement with no ? takes it, and matched A against B ? 'z' (found by measuring the CODE road of
/// DESIGN-PLAN random-codegen-scope step 11, a[8] a[4] ? @X). A replacement there wants a name, so A B ? 'b' = 'Q' is 212 as it is
/// compiled. Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ExplicitMatchSubject
{
    [TestMethod]
    public void TEST_ExplicitMatchSubject_everything_before_the_question_mark_is_the_subject()
    {
        Assert.AreEqual("1 succ X=0\n2 succ\n3 fail\n4 succ b\n5 fail", SetupTests.RunWithInput(@"        A = 'xyz'
        B = 'abc'
        A B ? @X                       :S(S1)F(F1)
S1      OUTPUT = '1 succ X=' X         :(N2)
F1      OUTPUT = '1 fail'
N2      A B ? 'z'                      :S(S2)F(F2)
S2      OUTPUT = '2 succ'              :(N3)
F2      OUTPUT = '2 fail'
N3      A 'y' ? 'q'                    :S(S3)F(F3)
S3      OUTPUT = '3 succ'              :(N4)
F3      OUTPUT = '3 fail'
N4      Z = A B ? 'b'                  :S(S4)F(F4)
S4      OUTPUT = '4 succ ' Z           :(N5)
F4      OUTPUT = '4 fail'
N5      A (B ? 'b')                    :S(S5)F(F5)
S5      OUTPUT = '5 succ'              :(END)
F5      OUTPUT = '5 fail'
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_ExplicitMatchSubject_a_replacement_of_a_concatenation_is_212()
    {
        var program = @"        A B ? 'b' = 'Q'
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               A B ? 'b' = 'Q'

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
