using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// SPITBOL's compiler takes the name of a variable, of a keyword and of a deferred expression (.X, .&amp;ERRTEXT, .*X) for a constant
/// NAME, and the cursor assignment into a variable (@X) for a constant pattern, so an operator or a pre-evaluated function given one is
/// evaluated as the statement is compiled and its error is the statement's: +.X is 4, -.X 10, .X + 1 1, .X * 2 26, 2 ** .X 15,
/// LEN(.X) 120, TAB(.X) 183, .*X + 1 and @X + 1 1; a keyword's name has no text, so .&amp;ERRTEXT 'z' is 8, 'z' .&amp;ERRTEXT 9,
/// SIZE(.&amp;ERRTEXT) 189, ANY(.&amp;ERRTEXT) 59 and .&amp;ERRTEXT | 'a' 6. The name of an element, of an indirection or of a call is found
/// as the program runs (.A&lt;1&gt; + 1 compiles). dotnet found none of them as it compiled, and the first ended the run as it ran. Found by
/// the infinite_snobol4 generator (+.X, .X + 1). Every expected number is what sbl -b listed for the same statement.
/// </summary>
[TestClass]
public class NameConstants
{
    [TestMethod]
    public void TEST_NameConstants_the_name_of_a_fixed_name_is_folded_as_it_is_compiled()
    {
        var build = SetupTests.SetupScript("-b -n", @"        Y = +.X
        Y = -.X
        Y = .X + 1
        Y = .X * 2
        Y = .&ERRTEXT 'z'
        Y = 'z' .&ERRTEXT
        Y = .X 'l'
        Y = LEN(.X)
        Y = SIZE(.X)
        Y = SIZE(.&ERRTEXT)
        Y = ANY(.X)
        Y = ANY(.&ERRTEXT)
        Y = .X | 'a'
        Y = .&ERRTEXT | 'a'
        Y = DUPL(.X, 2)
        Y = REPLACE(.X, 'X', 'Y')
        Y = .A<1> + 1
        Y = .$X + 1
        Y = .F() + 1
        Y = .X ? 'X'
        Y = -.&ERRTEXT
        Y = 2 ** .X
        Y = .X . Z
        Y = (.X , 1) + 1
        Y = TAB(.X)
        Y = .*X + 1
        Y = @X + 1
        Y = @X 'z'
        Y = &.X
END");
        CollectionAssert.AreEqual(new[] { 4, 10, 1, 26, 8, 9, 120, 189, 59, 6, 10, 15, 183, 1, 1, 212 },
            build.ErrorCodeHistory.ToArray());
    }
}
