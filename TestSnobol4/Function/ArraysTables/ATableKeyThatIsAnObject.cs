using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// SPITBOL keys a table by the object itself when the subscript is no string or number: an array, a pattern (and every copy of it),
/// an expression, a name. A name is a key of its own, never a string (T&lt;.X&gt; is not T&lt;'X'&gt;): the names of one variable or of
/// one element are one key, those of two elements are two, and the name of a keyword or of a deferred expression is only itself.
/// CONVERT of the table to ARRAY gives each key back as it was, the very array (IDENT) and the NAME. dotnet keyed a name by its text,
/// so names met strings and every element's name was one key, and keyed an object by its creation number, which CONVERT handed back
/// as an INTEGER. Found by a probe of names as subscripts from the infinite_snobol4 generator. Every expected line is what sbl -b
/// printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ATableKeyThatIsAnObject
{
    [TestMethod]
    public void TEST_ATableKeyThatIsAnObject_a_name_is_a_key_of_its_own()
    {
        var program = @"        T = TABLE()
        A = ARRAY(3)
        T<.X> = 'nameX'
        OUTPUT = '1 [' T<.X> '] [' T<'X'> ']'
        T<'Y'> = 'strY'
        OUTPUT = '2 [' T<.Y> '] [' T<'Y'> ']'
        T<.A<1>> = 'el1'
        OUTPUT = '3 [' T<.A<1>> '] [' T<.A<2>> '] [' T<''> ']'
        T<.&TRACE> = 'kw'
        OUTPUT = '4 [' T<.&TRACE> '] [' T<'&trace'> '] [' T<'&TRACE'> ']'
        N = .*A<1>
        T<N> = 'deferred'
        OUTPUT = '5 [' T<N> '] [' T<.*A<1>> '] [' T<.A<1>> ']'
        C = CONVERT(T, 'ARRAY')
        I = 0
L       I = I + 1
        OUTPUT = '6 ' DATATYPE(C<I, 1>) ' ' C<I, 2>          :S(L)
END";
        Assert.AreEqual(@"1 [nameX] []
2 [] [strY]
3 [el1] [] []
4 [] [] []
5 [deferred] [] [el1]
6 NAME nameX
6 STRING strY
6 NAME el1
6 NAME kw
6 NAME deferred", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_ATableKeyThatIsAnObject_is_the_object_itself()
    {
        var program = @"        T = TABLE()
        A = ARRAY(2)
        P = LEN(1)
        E = *X
        T<A> = 'arr'
        T<P> = 'pat'
        T<E> = 'exp'
        T<3> = 'int'
        T<2.5> = 'real'
        OUTPUT = '1 [' T<A> '] [' T<P> '] [' T<E> '] [' T<COPY(A)> ']'
        C = CONVERT(T, 'ARRAY')
        I = 0
L       I = I + 1
        OUTPUT = '2 ' DATATYPE(C<I, 1>) ' ' C<I, 2>          :S(L)
        OUTPUT = '3 ' (IDENT(C<1, 1>, A) 'ident', 'differ')
        Q = P
        OUTPUT = '4 [' T<Q> '] [' T<LEN(1)> ']'
        N = .&TRACE
        T<N> = 'kwN'
        OUTPUT = '5 [' T<N> ']'
END";
        Assert.AreEqual(@"1 [arr] [pat] [exp] []
2 ARRAY arr
2 PATTERN pat
2 EXPRESSION exp
2 INTEGER int
2 REAL real
3 ident
4 [pat] []
5 [kwN]", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
