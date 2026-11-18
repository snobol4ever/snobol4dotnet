using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// TRACE(.A[2]) and TRACE(.T['k'], 'VALUE') trace the element itself, as in SPITBOL: an assignment to that slot of that array or table, by
/// whatever variable holds it, prints the trace line, named as the block is named (A&lt;2&gt;, A&lt;1,2&gt;, T&lt;'k'&gt;), and a new array bound to
/// the same variable is not traced. snobol4dotnet answered ERROR 198, trace first argument is not appropriate name, since its trace tables
/// are keyed by a variable's name alone. Every expected output is sbl -bf's (gate_cases_snobol4 trace_of_an_array_or_table_element and
/// the_value_trace_of_a_multi_dimensional_array_element, the master's ladder__rung23_trace_element).
/// </summary>
[DoNotParallelize]
[TestClass]
public class TraceOfAnElement
{
    private static string Run(string program) => SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd();

    [TestMethod]
    public void TEST_TraceOfAnElement_an_array_and_a_table_element()
    {
        Assert.AreEqual("****6*******  A<2> = 5\n****8*******  T<'k'> = 7\ndone 5", Run(@"	&TRACE = 100
	A = ARRAY(3)
	T = TABLE()
	TRACE(.A[2])
	TRACE(.T['k'], 'VALUE')
	A[2] = 5
	A[1] = 6
	T['k'] = 7
	T['j'] = 8
	OUTPUT = 'done ' A[2]
END"));
    }

    [TestMethod]
    public void TEST_TraceOfAnElement_through_an_alias()
    {
        Assert.AreEqual("****5*******  A<2> = 5\nend", Run(@"	&TRACE = 100
	A = ARRAY(3)
	B = A
	TRACE(.B[2])
	A[2] = 5
	OUTPUT = 'end'
END"));
    }

    [TestMethod]
    public void TEST_TraceOfAnElement_a_new_array_is_not_traced()
    {
        Assert.AreEqual("****5*******  A<2> = 5\nend", Run(@"	&TRACE = 100
	A = ARRAY(3)
	B = A
	TRACE(.A[2])
	B[2] = 5
	A = ARRAY(3)
	A[2] = 9
	OUTPUT = 'end'
END"));
    }

    [TestMethod]
    public void TEST_TraceOfAnElement_a_cell_of_two_dimensions()
    {
        Assert.AreEqual("****4*******  A<1,2> = 5\n5", Run(@"        &TRACE = 100
        A = ARRAY('2,3')
        TRACE(.A[1,2], 'VALUE')
        A[1,2] = 5
        OUTPUT = A[1,2]
END"));
    }
}
