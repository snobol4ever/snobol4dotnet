namespace Test.TestLexer;

// A numeric literal SPITBOL cannot hold (an integer past the 64-bit range, a real past the doubles) is error 231 reported where the literal
// starts, not after it; the columns are the ones sbl -b prints for the same lines (13, 14 and 13), and ColumnHistory counts from 0.
public partial class TestLexer
{
    [TestMethod]
    public void TEST_231_integer_past_the_range_is_reported_at_its_start()
    {
        var s = "        A = 9223372036854775808 + 1\nend";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_the_digits_of_a_negative_literal_start_after_the_sign()
    {
        var s = "        A = -9223372036854775808\nend";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(13, build.ColumnHistory[0]);
    }

    [TestMethod]
    public void TEST_231_real_past_the_doubles_is_reported_at_its_start()
    {
        var s = "        A = 1e400 + 1\nend";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(231, build.ErrorCodeHistory[0]);
        Assert.AreEqual(12, build.ColumnHistory[0]);
    }
}
