using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// ARG, LOCAL, FIELD, ITEM and APPLY as sbl takes their arguments: the second argument of ARG, LOCAL and FIELD is looked at first (a
/// number that is no integer is the error, one out of range fails) and only then the function or datatype named by the first (a name
/// that is not a program function or datatype is 63, 135 or 108); ARG, LOCAL and FIELD give NAMEs; a first argument of ITEM that is no
/// array or table is 235; APPLY of a null string, a pattern or an element name is 60. Every expected line is what sbl -b printed for
/// the same expression.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NamedFunctionArguments
{
    private const string Header =
        "        &ERRLIMIT = 1000\n        &TRIM = 0\n        NEG = -1\n        BAD = 'x'\n        ONE = 1\n        E = ''\n        S = 'str'\n" +
        "        HUGE = 9223372036854775807\n        P = LEN(2)\n" +
        "        DEFINE('F(A,B)L1,L2')   :(SKIP)\nF       F = 'called'   :(RETURN)\nSKIP    DATA('D(X,Y,Z)')\n" +
        "        A = ARRAY(3)\n        A<2> = 'two'\n        T = TABLE()\n        T<'k'> = 'kay'\n        M = ARRAY('2,2')\n";

    private static readonly (string Expression, string Expected)[] Cases =
    [
        ("ARG('F',1)", "ok NAME [A] 0"),
        ("ARG('F',2)", "ok NAME [B] 0"),
        ("ARG('F',0)", "fail 0"),
        ("ARG('F',3)", "fail 0"),
        ("ARG('F',NEG)", "fail 0"),
        ("ARG('F',BAD)", "fail 62"),
        ("ARG('G',1)", "fail 63"),
        ("ARG('D',1)", "fail 63"),
        ("ARG(.F,1)", "ok NAME [A] 0"),
        ("ARG(ONE,1)", "fail 63"),
        ("ARG(E,1)", "fail 63"),
        ("ARG('f',1)", "ok NAME [A] 0"),
        ("ARG('F',HUGE)", "fail 0"),
        ("ARG(F,1)", "fail 63"),
        ("LOCAL('F',1)", "ok NAME [L1] 0"),
        ("LOCAL('F',2)", "ok NAME [L2] 0"),
        ("LOCAL('F',3)", "fail 0"),
        ("LOCAL('F',0)", "fail 0"),
        ("LOCAL('G',1)", "fail 135"),
        ("LOCAL('F',BAD)", "fail 134"),
        ("LOCAL(BAD,BAD)", "fail 134"),
        ("LOCAL(.F,2)", "ok NAME [L2] 0"),
        ("FIELD('D',1)", "ok NAME [X] 0"),
        ("FIELD('D',3)", "ok NAME [Z] 0"),
        ("FIELD('D',4)", "fail 0"),
        ("FIELD('D',0)", "fail 0"),
        ("FIELD('F',1)", "fail 108"),
        ("FIELD('D',BAD)", "fail 107"),
        ("FIELD(BAD,BAD)", "fail 107"),
        ("FIELD(E,1)", "fail 108"),
        ("FIELD(.D,2)", "ok NAME [Y] 0"),
        ("FIELD('d',2)", "ok NAME [Y] 0"),
        ("ITEM(T,'k')", "ok STRING [kay] 0"),
        ("ITEM(A,2)", "ok STRING [two] 0"),
        ("ITEM(A)", "fail 236"),
        ("ITEM(S,1)", "fail 235"),
        ("ITEM(T,1,2)", "fail 237"),
        ("ITEM(A,BAD)", "fail 238"),
        ("ITEM(A,9)", "fail 0"),
        ("ITEM(M,1,2)", "ok STRING [] 0"),
        ("ITEM(M,1)", "fail 236"),
        ("ITEM(T)", "fail 237"),
        ("ITEM(E,1)", "fail 235"),
        ("ITEM(ONE,1)", "fail 235"),
        ("ITEM(P,1)", "fail 235"),
        ("ITEM()", "fail 235"),
        ("APPLY('SIZE','abc')", "ok INTEGER [3] 0"),
        ("APPLY(.SIZE,'abc')", "ok INTEGER [3] 0"),
        ("APPLY(ONE)", "fail 22"),
        ("APPLY(E)", "fail 60"),
        ("APPLY('F',1,2)", "ok STRING [called] 0"),
        ("APPLY('G')", "fail 22"),
        ("APPLY(BAD)", "fail 41"),
        ("APPLY('SIZE')", "ok INTEGER [0] 0"),
        ("APPLY('SIZE',1,2)", "ok INTEGER [1] 0"),
        ("APPLY(P)", "fail 60"),
        ("APPLY()", "fail 60"),
        ("APPLY(.A<1>)", "fail 60"),
        ("APPLY('lt',1,2)", "ok STRING [] 0"),
        ("APPLY('LT',2,1)", "fail 0")
    ];

    [TestMethod]
    public void TEST_NamedFunctionArguments_each_expression_gives_the_result_and_error_sbl_gives()
    {
        var program = new System.Text.StringBuilder(Header);
        for (var i = 0; i < Cases.Length; ++i)
        {
            program.Append("        &ERRTYPE = 0\n");
            program.Append($"        X = {Cases[i].Expression}   :F(F{i})\n");
            program.Append($"        OUTPUT = 'ok ' DATATYPE(X) ' [' X '] ' &ERRTYPE   :(N{i})\n");
            program.Append($"F{i}     OUTPUT = 'fail ' &ERRTYPE\n");
            program.Append($"N{i}\n");
        }

        program.Append("END");
        var expected = string.Join("\n", Cases.Select(c => c.Expected));
        Assert.AreEqual(expected, SetupTests.RunWithInput(program.ToString()));
    }

    [TestMethod]
    public void TEST_NamedFunctionArguments_convert_to_a_null_type_is_74()
    {
        Assert.AreEqual("74", SetupTests.RunWithInput("        &ERRLIMIT = 10\n        E = ''\n        X = CONVERT('abc', E)\n        OUTPUT = &ERRTYPE\nEND"));
    }
}
