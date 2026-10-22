using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SORT and RSORT of a table order rows with equal keys by the table's buckets, last bucket first, each bucket's entries in the order
/// they were added (SPITBOL builds the array it sorts from the table that way, though CONVERT(T, 'ARRAY') lists the entries in the
/// order they were added): Budne's cfreq sorted its character counts by frequency and printed its ties in another order. The expected
/// text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SortTableTies
{
    [TestMethod]
    public void TEST_SortTableTies_one_bucket_keeps_the_order_added()
    {
        var program = @"        T = TABLE(1)
        K = 'dbhafcge'
LOOP    K LEN(1) . C =  :F(DONE)
        T<C> = 1  :(LOOP)
DONE    B = SORT(T, 2)
        I = 0
L2      I = I + 1
        R = R B<I,1>  :F(S2)
        :(L2)
S2      OUTPUT = 'sort  ' R
        B = RSORT(T, 2)
        I = 0
        R =
L3      I = I + 1
        R = R B<I,1>  :F(S3)
        :(L3)
S3      OUTPUT = 'rsort ' R
END";
        Assert.AreEqual(@"sort  dbhafcge
rsort dbhafcge", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortTableTies_five_buckets_last_bucket_first()
    {
        var program = @"        T = TABLE(5)
        K = 'dbhafcge'
LOOP    K LEN(1) . C =  :F(DONE)
        T<C> = 1  :(LOOP)
DONE    B = SORT(T, 2)
        I = 0
L2      I = I + 1
        R = R B<I,1>  :F(S2)
        :(L2)
S2      OUTPUT = 'sort  ' R
        B = RSORT(T, 2)
        I = 0
        R =
L3      I = I + 1
        R = R B<I,1>  :F(S3)
        :(L3)
S3      OUTPUT = 'rsort ' R
END";
        Assert.AreEqual(@"sort  bfcgdahe
rsort bfcgdahe", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortTableTies_sixteen_buckets()
    {
        var program = @"        T = TABLE(16)
        K = 'abcdefgh'
LOOP    K LEN(1) . C =  :F(DONE)
        T<C> = 1  :(LOOP)
DONE    B = SORT(T, 2)
        I = 0
L2      I = I + 1
        R = R B<I,1>  :F(S2)
        :(L2)
S2      OUTPUT = 'sort  ' R
        B = RSORT(T, 2)
        I = 0
        R =
L3      I = I + 1
        R = R B<I,1>  :F(S3)
        :(L3)
S3      OUTPUT = 'rsort ' R
END";
        Assert.AreEqual(@"sort  hfgdebca
rsort hfgdebca", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortTableTies_a_hundred_buckets()
    {
        var program = @"        T = TABLE(100)
        K = 'dbhafcge'
LOOP    K LEN(1) . C =  :F(DONE)
        T<C> = 1  :(LOOP)
DONE    B = SORT(T, 2)
        I = 0
L2      I = I + 1
        R = R B<I,1>  :F(S2)
        :(L2)
S2      OUTPUT = 'sort  ' R
        B = RSORT(T, 2)
        I = 0
        R =
L3      I = I + 1
        R = R B<I,1>  :F(S3)
        :(L3)
S3      OUTPUT = 'rsort ' R
END";
        Assert.AreEqual(@"sort  bcahfgde
rsort bcahfgde", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SortTableTies_character_counts_by_frequency()
    {
        var program = @"        T = TABLE(1000)
        S = 'hello world abracadabra'
LOOP    S LEN(1) . C =  :F(DONE)
        T<C> = T<C> + 1  :(LOOP)
DONE    B = RSORT(T, 2)
        I = 0
L1      I = I + 1
        R = R B<I,1>  :F(S1)
        :(L1)
S1      OUTPUT = R
END";
        Assert.AreEqual(@"arlodb whec", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
