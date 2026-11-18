using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// A NAME stored in a table or array element keeps what it names, as in SPITBOL: T&lt;'n'&gt; = .hold, then IDENT(T&lt;'n'&gt;, .hold) succeeds
/// and $T&lt;'n'&gt; reads hold; T&lt;'n'&gt; = .A&lt;1&gt;, then $T&lt;'n'&gt; reads A&lt;1&gt; and assigns it. snobol4dotnet kept a value's place (Key,
/// Collection) and a name's target in the same two fields, so a store into a collection overwrote what the name named: the IDENT failed,
/// and an element's name read back as the null string. Every expected line is what sbl -bf printed for the same program
/// (gate_cases_snobol4 test_gate_sno_an_array_element_read_or_store_allocates_nothing__tbl and __arr read red on it).
/// </summary>
[DoNotParallelize]
[TestClass]
public class ANameStoredInAnElement
{
    [TestMethod]
    public void TEST_ANameStoredInAnElement_a_variables_name_is_still_that_name()
    {
        var program = @"        hold = 'held'
        R2 = .hold
        T = TABLE()
        T['n'] = .hold
        R = T['n']
        OUTPUT = IDENT(R, R2) 'd'
        OUTPUT = DIFFER(R, R2) 'e'
        OUTPUT = IDENT(T['n'], .hold) 'ident'
        OUTPUT = $R
        A = ARRAY(2)
        A[1] = .hold
        OUTPUT = IDENT(A[1], .hold) 'ident4'
END";
        Assert.AreEqual("d\nident\nheld\nident4", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_ANameStoredInAnElement_an_elements_name_reads_and_assigns_the_element()
    {
        var program = @"        A = ARRAY(2)
        A[1] = 'x'
        T = TABLE()
        T['n'] = .A[1]
        R = T['n']
        OUTPUT = $R
        $R = 'y'
        OUTPUT = A[1]
        OUTPUT = T['n']
        OUTPUT = IDENT(R, .A[1]) 'same'
        B = ARRAY(2)
        B[2] = .A[1]
        OUTPUT = $B[2]
END";
        Assert.AreEqual("x\ny\nNAME\nsame\ny", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_ANameStoredInAnElement_the_element_holding_a_name_is_assigned_as_an_element()
    {
        var program = @"        hold = 'held'
        T = TABLE()
        T['n'] = .hold
        T['n'] = 'z'
        OUTPUT = T['n'] ' ' hold
END";
        Assert.AreEqual("z held", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd());
    }
}
