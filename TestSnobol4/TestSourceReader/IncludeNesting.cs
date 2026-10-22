using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// Includes nest nine deep and an include that cannot be opened is left open in sbl's source stack, counted all the same: the tenth
/// -INCLUDE of the kind is error 284, "excessively nested include files", and so is every one after it. Every expectation is what
/// sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class IncludeNesting
{
    [TestMethod]
    public void TEST_IncludeNesting_the_ninth_failing_include_is_285_and_the_tenth_284()
    {
        var program = @"-INCLUDE 'nope1.sno'
-INCLUDE 'nope2.sno'
-INCLUDE 'nope3.sno'
-INCLUDE 'nope4.sno'
-INCLUDE 'nope5.sno'
-INCLUDE 'nope6.sno'
-INCLUDE 'nope7.sno'
-INCLUDE 'nope8.sno'
-INCLUDE 'nope9.sno'
-INCLUDE 'nope10.sno'
-INCLUDE 'nope11.sno'
        A = 1
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -INCLUDE 'nope1.sno'
                 !
Test.sno(1,10) : ERROR 285 -- include file cannot be opened

     1  -INCLUDE 'nope2.sno'
                 !
Test.sno(2,10) : ERROR 285 -- include file cannot be opened

     2  -INCLUDE 'nope3.sno'
                 !
Test.sno(3,10) : ERROR 285 -- include file cannot be opened

     3  -INCLUDE 'nope4.sno'
                 !
Test.sno(4,10) : ERROR 285 -- include file cannot be opened

     4  -INCLUDE 'nope5.sno'
                 !
Test.sno(5,10) : ERROR 285 -- include file cannot be opened

     5  -INCLUDE 'nope6.sno'
                 !
Test.sno(6,10) : ERROR 285 -- include file cannot be opened

     6  -INCLUDE 'nope7.sno'
                 !
Test.sno(7,10) : ERROR 285 -- include file cannot be opened

     7  -INCLUDE 'nope8.sno'
                 !
Test.sno(8,10) : ERROR 285 -- include file cannot be opened

     8  -INCLUDE 'nope9.sno'
                 !
Test.sno(9,10) : ERROR 285 -- include file cannot be opened

     9  -INCLUDE 'nope10.sno'
                 !
Test.sno(10,10) : ERROR 284 -- excessively nested include files

     10 -INCLUDE 'nope11.sno'
                 !
Test.sno(11,10) : ERROR 284 -- excessively nested include files", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_IncludeNesting_nine_failing_includes_are_all_285()
    {
        var program = @"-INCLUDE 'nope1.sno'
-INCLUDE 'nope2.sno'
-INCLUDE 'nope3.sno'
-INCLUDE 'nope4.sno'
-INCLUDE 'nope5.sno'
-INCLUDE 'nope6.sno'
-INCLUDE 'nope7.sno'
-INCLUDE 'nope8.sno'
-INCLUDE 'nope9.sno'
        A = 1
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

        -INCLUDE 'nope1.sno'
                 !
Test.sno(1,10) : ERROR 285 -- include file cannot be opened

     1  -INCLUDE 'nope2.sno'
                 !
Test.sno(2,10) : ERROR 285 -- include file cannot be opened

     2  -INCLUDE 'nope3.sno'
                 !
Test.sno(3,10) : ERROR 285 -- include file cannot be opened

     3  -INCLUDE 'nope4.sno'
                 !
Test.sno(4,10) : ERROR 285 -- include file cannot be opened

     4  -INCLUDE 'nope5.sno'
                 !
Test.sno(5,10) : ERROR 285 -- include file cannot be opened

     5  -INCLUDE 'nope6.sno'
                 !
Test.sno(6,10) : ERROR 285 -- include file cannot be opened

     6  -INCLUDE 'nope7.sno'
                 !
Test.sno(7,10) : ERROR 285 -- include file cannot be opened

     7  -INCLUDE 'nope8.sno'
                 !
Test.sno(8,10) : ERROR 285 -- include file cannot be opened

     8  -INCLUDE 'nope9.sno'
                 !
Test.sno(9,10) : ERROR 285 -- include file cannot be opened", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
