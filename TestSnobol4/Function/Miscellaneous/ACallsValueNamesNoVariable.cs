using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The value a builtin hands back is a value, never a name, even when it is the very argument it was given (CONVERT to the type the
/// argument has, COPY of a string): under the name operator it is error 21, "function called by name returned a value", as in SPITBOL,
/// and it keeps the argument's identity (IDENT(A, CONVERT(A, 'ARRAY')), an element set through either is set in both). dotnet handed back
/// the argument's own object, which still carried the name of the variable it came from, so .CONVERT(X, 'STRING') and .COPY(X) named X.
/// Found by the infinite_snobol4 generator (.CONVERT(h(b), 'STRING')). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ACallsValueNamesNoVariable
{
    [TestMethod]
    public void TEST_ACallsValueNamesNoVariable_convert_and_copy_by_name_are_error_21()
    {
        var program = @"        &ERRLIMIT = 20
        X = 'x'
        N = 5
        P = LEN(1)
        A = ARRAY(2)
        T = TABLE()
        V = .CONVERT(X, 'STRING')                   :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                    :(E1)
Y1      OUTPUT = '1 S ' DATATYPE(V)
E1      V = .CONVERT(N, 'INTEGER')                  :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                    :(E2)
Y2      OUTPUT = '2 S ' DATATYPE(V)
E2      V = .CONVERT(P, 'PATTERN')                  :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                    :(E3)
Y3      OUTPUT = '3 S ' DATATYPE(V)
E3      V = .CONVERT(A, 'ARRAY')                    :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                    :(E4)
Y4      OUTPUT = '4 S ' DATATYPE(V)
E4      V = .CONVERT(T, 'TABLE')                    :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                    :(E5)
Y5      OUTPUT = '5 S ' DATATYPE(V)
E5      V = .COPY(X)                                :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE                    :(E6)
Y6      OUTPUT = '6 S ' DATATYPE(V)
E6      OUTPUT = '7 ' (IDENT(A, CONVERT(A, 'ARRAY')) 'same', 'differ') ' '
+              (IDENT(T, CONVERT(T, 'TABLE')) 'same', 'differ') ' ' (IDENT(P, CONVERT(P, 'PATTERN')) 'same', 'differ')
        B = CONVERT(A, 'ARRAY')
        B<1> = 'set'
        U = CONVERT(T, 'TABLE')
        U<'k'> = 'kv'
        OUTPUT = '8 ' A<1> ' ' T<'k'>
        'abc' ? CONVERT(P, 'PATTERN') . W
        OUTPUT = '9 ' W ' ' CONVERT(X, 'STRING') ' ' CONVERT(N, 'INTEGER') + 1
END";
        Assert.AreEqual(@"1 F 21
2 F 21
3 F 21
4 F 21
5 F 21
6 F 21
7 same same same
8 set kv
9 a x 6", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
