using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// An error found by folding a statement's constants is found once the whole statement is read, so its page lists the statement's last
/// card and names that card's line, as sbl -bf does: X = 'a' ! / + 'b' is ERROR 015 at line 2, its page the + card (decus_vax84a atn,
/// palin and synt). snobol4dotnet listed the first card and named its line.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AFoldingErrorStandsOnTheLastCard
{
    [TestMethod]
    public void TEST_AFoldingErrorStandsOnTheLastCard()
    {
        var output = SetupTests.MaskListingHead(SetupTests.RunWithInputDirectives("-bf", @"        X = 'a' !
+           'b'
        OUTPUT = 'ran'
END")).Replace("\r\n", "\n");
        StringAssert.Contains(output, "1       +           'b'\n\nTest.sno(2) : ERROR 015 -- exponentiation right operand is not numeric");
    }
}
