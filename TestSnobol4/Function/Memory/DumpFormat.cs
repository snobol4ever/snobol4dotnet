using Test.TestLexer;

namespace Test.Memory;

/// <summary>
/// DUMP writes SPITBOL's dump to standard output: variables in lexical order, the keyword values, then
/// for DUMP(2) each array, table and DATA object by creation number, its elements named through a
/// variable that holds it (found in SPITBOL's hash order) or else by its own value line. The expectation
/// is sbl -b's output with the keyword lines left out.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DumpFormat
{
    [TestMethod]
    public void TEST_DumpFormat_variables_blocks_and_names()
    {
        var output = SetupTests.RunWithInput(@"        T = TABLE(5)
        T<'a'> = ARRAY(2)
        T<'a'><1> = 'q'
        U = T
        AA = ARRAY(3)
        AA<2> = AA
        DATA('NODE(VAL,NXT)')
        N = NODE(1, NODE(2))
        DUMP(2)
        OUTPUT = 'after'
END").Replace("\r\n", "\n");
        var withoutKeywords = string.Join("\n", output.Split('\n').Where(line => !line.StartsWith('&')));
        Assert.AreEqual("\n\n\ndump of natural variables\n\nAA = ARRAY(3) #3\nN = NODE #5\nT = TABLE(5) #1\nU = TABLE(5) #1\n\n\ndump of keyword values\n\n\nTABLE(5) #1\nU<'a'> = ARRAY(2) #2\n\nARRAY(2) #2\nARRAY(2) #2<1> = 'q'\n\nARRAY(3) #3\nAA<2> = ARRAY(3) #3\n\nNODE #4\nVAL(NODE #4) = 2\n\nNODE #5\nVAL(N) = 1\nNXT(N) = NODE #4\n\n\n\nafter",
            withoutKeywords);
    }

    [TestMethod]
    public void TEST_DumpFormat_table_buckets_and_array_order()
    {
        // sbl -b: TABLE(0) has 11 buckets; entries list by bucket (|-7| mod 100, the bits of 2.5 mod 100,
        // the string's hash mod 100); a two-dimensional array lists row by row
        var output = SetupTests.RunWithInput(@"        T0 = TABLE(0)
        T9 = TABLE(100)
        T9<2.5> = 'real'
        T9<-7> = 'neg'
        T9<'abcdefghijklmnopqrstuvwxyz0123'> = 'long'
        A = ARRAY('2,2')
        A<2,1> = 21
        A<1,2> = 12
        DUMP(2)
END").Replace("\r\n", "\n");
        var lines = output.Split('\n').Where(line => !line.StartsWith('&') && line != "").ToArray();
        CollectionAssert.AreEqual(new[]
        {
            "dump of natural variables", "A = ARRAY(2,2) #3", "T0 = TABLE(11) #1", "T9 = TABLE(100) #2",
            "dump of keyword values", "TABLE(11) #1", "TABLE(100) #2", "T9<-7> = 'neg'", "T9<2.5> = 'real'",
            "T9<'abcdefghijklmnopqrstuvwxyz0123'> = 'long'", "ARRAY(2,2) #3", "A<1,2> = 12", "A<2,1> = 21"
        }, lines);
    }
}
