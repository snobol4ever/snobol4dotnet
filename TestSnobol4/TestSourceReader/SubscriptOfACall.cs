using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A subscript of what a call returns, F(X)[1] or F(X)&lt;1&gt;, is an element like A[1], and the statement around it is checked as any
/// other is when it is compiled: a value where a name is required is error 212 and a constant division by zero 14, as in SPITBOL. The
/// check gave up on the whole statement at such a subscript, which has no array name before its bracket, so the statement compiled and
/// ran: (Q = 'x') ARRAY(1)[1] ? 'x' = 'gt' through EVAL replaced into Q (found by the infinite_snobol4 generator at scope 9,
/// ~((Q = SIZE(RSORT(u)[j, 1])) Q ? ... = 'gt')). Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SubscriptOfACall
{
    [TestMethod]
    public void TEST_SubscriptOfACall_the_statement_is_still_checked()
    {
        var program = @"        X = ARRAY(1)[1] . ~B
        X = (Q SORT(T)[1, 2] ? 'x' = 'gt')
        X = 1 / 0 ARRAY(1)<1>
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = ARRAY(1)[1] . ~B

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required

2               X = (Q SORT(T)[1, 2] ? 'x' = 'gt')

Test.sno(2) : ERROR 212 -- syntax error: value used where name is required

3               X = 1 / 0 ARRAY(1)<1>

Test.sno(3) : ERROR 014 -- division caused integer overflow", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SubscriptOfACall_through_EVAL_a_replacement_into_a_concatenation_is_212()
    {
        var program = @"        &ERRLIMIT = 10
        SETEXIT('ERR')
        R = EVAL(""(Q = 'x') ARRAY(1)[1] ? 'x' = 'gt'"")  :F(NO)
        OUTPUT = 'value ' R                          :(CAPS)
NO      OUTPUT = 'FAIL'                              :(CAPS)
ERR     OUTPUT = 'ERROR ' &ERRTYPE
CAPS    OUTPUT = 'Q=' Q
        R = EVAL(""ARRAY(1)[1] ? '' = 'gt'"")
        OUTPUT = 'element ' R
END";
        Assert.AreEqual(@"ERROR 212
Q=
element gt", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
