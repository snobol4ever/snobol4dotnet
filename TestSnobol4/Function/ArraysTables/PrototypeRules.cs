using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// ARRAY prototypes and TABLE sizes as sbl takes them. Every expected line is what sbl -b printed for the same program. ARRAY takes its
/// first argument as an integer before it takes it as a prototype; a prototype is bounds separated by commas, each "high" or "low:high",
/// where a bound is read as an integer (a null bound is 0, a real is truncated). The error is 64 when the argument is no string, 65 for
/// a low bound and 66 for a high bound that is not an integer, 67 for a dimension that is zero, negative or out of range (so there is no
/// zero-length array: 1:0 is 67), and 68 when the array is larger than the biggest object. A prototype is returned as it was written.
/// </summary>
[DoNotParallelize]
[TestClass]
public class PrototypeRules
{
    private const string Header = "        &ERRLIMIT = 1000\n        &TRIM = 0\n        S = 'abc'\n        P = LEN(2)\n        TT = TABLE()\n";

    private static readonly (string Argument, string Expected)[] Cases =
    [
        ("''", "fail 67"),
        ("0", "fail 67"),
        ("-1", "fail 67"),
        ("3", "ok ARRAY [3] 0"),
        ("3.7", "ok ARRAY [3] 0"),
        ("'3'", "ok ARRAY [3] 0"),
        ("'2,3'", "ok ARRAY [2,3] 0"),
        ("'1:3'", "ok ARRAY [1:3] 0"),
        ("'0:2'", "ok ARRAY [0:2] 0"),
        ("'-1:1'", "ok ARRAY [-1:1] 0"),
        ("'1:0'", "fail 67"),
        ("'2:1'", "fail 67"),
        ("'3:'", "fail 67"),
        ("':3'", "ok ARRAY [:3] 0"),
        ("' 3'", "ok ARRAY [3] 0"),
        ("'3 '", "ok ARRAY [3] 0"),
        ("'1 : 3'", "ok ARRAY [1 : 3] 0"),
        ("'a'", "fail 66"),
        ("'1:a'", "fail 66"),
        ("'a:3'", "fail 65"),
        ("'1,,2'", "fail 67"),
        ("'1,'", "fail 67"),
        ("',1'", "fail 67"),
        ("'+3'", "ok ARRAY [3] 0"),
        ("'-3'", "fail 67"),
        ("'1e1'", "ok ARRAY [10] 0"),
        ("'3.0'", "ok ARRAY [3] 0"),
        ("'3.5'", "ok ARRAY [3] 0"),
        ("'1:3,2:4'", "ok ARRAY [1:3,2:4] 0"),
        ("'  '", "fail 67"),
        ("P", "fail 64"),
        ("*S", "fail 64"),
        ("TT", "fail 64"),
        ("'1:2,3:4,5:6'", "ok ARRAY [1:2,3:4,5:6] 0"),
        ("'100,100'", "ok ARRAY [100,100] 0"),
        ("'4000000'", "fail 68"),
        ("'20000000'", "fail 68"),
        ("'1,1,1,1'", "ok ARRAY [1,1,1,1] 0"),
        ("'0:0'", "ok ARRAY [0:0] 0"),
        ("'5:5'", "ok ARRAY [5:5] 0"),
        ("'-2:-1'", "ok ARRAY [-2:-1] 0")
    ];

    private static string Run(string body) => SetupTests.RunWithInput(Header + body + "END");

    [TestMethod]
    public void TEST_Prototype_each_argument_gives_the_result_and_error_sbl_gives()
    {
        var program = new System.Text.StringBuilder();
        for (var i = 0; i < Cases.Length; ++i)
        {
            program.Append("        &ERRTYPE = 0\n");
            program.Append($"        A = ARRAY({Cases[i].Argument}, 'f')   :F(F{i})\n");
            program.Append($"        OUTPUT = 'ok ' DATATYPE(A) ' [' PROTOTYPE(A) '] ' &ERRTYPE   :(N{i})\n");
            program.Append($"F{i}     OUTPUT = 'fail ' &ERRTYPE\n");
            program.Append($"N{i}\n");
        }

        var expected = string.Join("\n", Cases.Select(c => c.Expected));
        Assert.AreEqual(expected, Run(program.ToString()));
    }

    [TestMethod]
    public void TEST_Prototype_the_biggest_array_is_the_biggest_object()
    {
        // a vector is its elements and 3 words of 8 bytes, an array 6 and two for each dimension, in 16777216 bytes
        var program =
            "        A = ARRAY(2097149)   :F(F1)\n        OUTPUT = 'v1 ok ' &ERRTYPE   :(N1)\nF1      OUTPUT = 'v1 fail ' &ERRTYPE\nN1      &ERRTYPE = 0\n" +
            "        A = ARRAY(2097150)   :F(F2)\n        OUTPUT = 'v2 ok ' &ERRTYPE   :(N2)\nF2      OUTPUT = 'v2 fail ' &ERRTYPE\nN2      &ERRTYPE = 0\n" +
            "        A = ARRAY('1:2097144')   :F(F3)\n        OUTPUT = 'p1 ok ' &ERRTYPE   :(N3)\nF3      OUTPUT = 'p1 fail ' &ERRTYPE\nN3      &ERRTYPE = 0\n" +
            "        A = ARRAY('1:2097145')   :F(F4)\n        OUTPUT = 'p2 ok ' &ERRTYPE   :(N4)\nF4      OUTPUT = 'p2 fail ' &ERRTYPE\nN4      &ERRTYPE = 0\n" +
            "        A = ARRAY('1048571,2')   :F(F5)\n        OUTPUT = 'm1 ok ' &ERRTYPE   :(N5)\nF5      OUTPUT = 'm1 fail ' &ERRTYPE\nN5      &ERRTYPE = 0\n" +
            "        A = ARRAY('1048572,2')   :F(F6)\n        OUTPUT = 'm2 ok ' &ERRTYPE   :(N6)\nF6      OUTPUT = 'm2 fail ' &ERRTYPE\nN6      &ERRTYPE = 0\n" +
            "        A = ARRAY('4611686018427387904,4')   :F(F7)\n        OUTPUT = 'o1 ok ' &ERRTYPE   :(N7)\nF7      OUTPUT = 'o1 fail ' &ERRTYPE\nN7      &ERRTYPE = 0\n" +
            "        A = ARRAY('-9223372036854775807:9223372036854775807')   :F(F8)\n        OUTPUT = 'o2 ok ' &ERRTYPE   :(N8)\nF8      OUTPUT = 'o2 fail ' &ERRTYPE\nN8      &ERRTYPE = 0\n";
        Assert.AreEqual("v1 ok 0\nv2 fail 68\np1 ok 0\np2 fail 68\nm1 ok 0\nm2 fail 68\no1 fail 68\no2 fail 67", Run(program));
    }

    [TestMethod]
    public void TEST_Prototype_a_null_low_bound_is_zero()
    {
        Assert.AreEqual("zero  0", Run("        A = ARRAY(':3')\n        A<0> = 'zero'\n        OUTPUT = A<0> ' ' A<3> ' ' &ERRTYPE\n"));
    }

    [TestMethod]
    public void TEST_Table_a_size_that_is_no_integer_is_195_and_one_out_of_range_is_196()
    {
        var program =
            "        BIG = 16777217\n        X = TABLE('x')\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n" +
            "        X = TABLE(-1)\n        OUTPUT = &ERRTYPE\n        &ERRTYPE = 0\n" +
            "        X = TABLE(BIG)\n        OUTPUT = &ERRTYPE\n";
        Assert.AreEqual("195\n196\n196", Run(program));
    }

    [TestMethod]
    public void TEST_Table_its_prototype_is_its_size_as_an_integer_and_11_when_none_is_given()
    {
        var program =
            "        T = TABLE(5)\n        X = PROTOTYPE(T)\n        OUTPUT = DATATYPE(X) ' ' X\n" +
            "        T = TABLE()\n        X = PROTOTYPE(T)\n        OUTPUT = DATATYPE(X) ' ' X\n" +
            "        T = TABLE(1.5)\n        X = PROTOTYPE(T)\n        OUTPUT = DATATYPE(X) ' ' X\n" +
            "        T = TABLE('7')\n        X = PROTOTYPE(T)\n        OUTPUT = DATATYPE(X) ' ' X\n" +
            "        T = TABLE(0)\n        X = PROTOTYPE(T)\n        OUTPUT = DATATYPE(X) ' ' X\n";
        Assert.AreEqual("INTEGER 5\nINTEGER 11\nINTEGER 1\nINTEGER 7\nINTEGER 11", Run(program));
    }
}
