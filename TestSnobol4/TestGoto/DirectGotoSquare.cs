using Test.TestLexer;

namespace Test.TestGoto;

/// <summary>
/// sbl -bf takes a direct goto in square brackets as it takes one in angle brackets: :[C], :S[D], :S[C3]F[D3], :[A[1]] with a subscript
/// inside, and :F[E]S(L5) beside a normal goto. snobol4dotnet refused the statement, ERROR 234 -- syntax error: goto field incorrect, since
/// its goto patterns read ( and &lt; alone (corpus SNOBOL4 master, sweep__a_direct_goto_in_square_brackets_is_a_direct_goto; the expected
/// output is sbl's).
/// </summary>
[DoNotParallelize]
[TestClass]
public class DirectGotoSquare
{
    [TestMethod]
    public void TEST_DirectGotoSquare_every_form_goes_to_the_code()
    {
        var program = @"         C = CODE(' OUTPUT = 1 :(L1)')
         N = N + 1                              :[C]
L1       D = CODE(' OUTPUT = 2 :(L2)')
         EQ(1,1)                                :S[D]
L2       C3 = CODE(' OUTPUT = 3 :(L3)')
         D3 = CODE(' OUTPUT = 4 :(L3)')
         EQ(1,2)                                :S[C3]F[D3]
L3       A = ARRAY(2)
         A[1] = CODE(' OUTPUT = 5 :(L4)')
                                                :[A[1]]
L4       E = CODE(' OUTPUT = 6 :(L5)')
         EQ(1,2)                                :F[E]S(L5)
L5       OUTPUT = 'end'
END";
        Assert.AreEqual("1\n2\n4\n5\n6\nend", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_DirectGotoSquare_an_unconditional_one()
    {
        var program = @"         C = CODE(' OUTPUT = ""in code"" :(L1)')
                                                :[C]
         OUTPUT = 'not here'
L1       OUTPUT = 'after'
END";
        Assert.AreEqual("in code\nafter", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }
}
