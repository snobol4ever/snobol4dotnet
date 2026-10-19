using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// At end of input an assignment from INPUT fails and assigns nothing, as in SPITBOL: OUTPUT = INPUT
/// writes no empty line and X = INPUT leaves X as it was.
/// </summary>
[DoNotParallelize]
[TestClass]
public class InputEnd
{
    [TestMethod]
    public void TEST_InputEnd_the_failed_read_assigns_nothing()
    {
        // sbl -b with input x answers x then X=keep
        Assert.AreEqual("x\nX=keep", SetupTests.RunWithInput(@"        X = 'keep'
R       OUTPUT = INPUT   :F(D)
        :(R)
D       X = INPUT        :S(END)
        OUTPUT = 'X=' X
END", "x").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_InputEnd_the_name_of_input_reads_no_line()
    {
        // sbl -b with input one/two answers NAME INPUT, L=one, via=two: .INPUT is INPUT's name and reads
        // nothing (Gimpel's INPUT(.INPUT, 8, , file)); snobol4dotnet read a line and refused with error 212
        Assert.AreEqual("NAME INPUT\nL=one\nvia=two", SetupTests.RunWithInput(@"        N = .INPUT
        OUTPUT = DATATYPE(N) ' ' N
        L = INPUT
        OUTPUT = 'L=' L
        OUTPUT = 'via=' $N
END", "one\ntwo").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_InputEnd_input_of_a_name_alone_reads_standard_input()
    {
        // sbl -b with input one/two answers one|two: INPUT(.SRC) with no channel or file reads standard input
        // (aisnobol's ATN does INPUT(.ATNSOURCE)); snobol4dotnet crashed with KeyNotFoundException
        Assert.AreEqual("one|two", SetupTests.RunWithInput(@"        INPUT(.SRC)
        A = SRC
        B = SRC
        OUTPUT = A '|' B
END", "one\ntwo").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_InputEnd_predicates_read_their_input_operand()
    {
        // sbl -b with input a/b/c answers 3 then 2: ?INPUT and ~INPUT read a line each, and end of input fails
        // ?INPUT and lets ~INPUT succeed (aisnobol's HSORT counts lines with N = ?INPUT N + 1); snobol4dotnet
        // read nothing and looped forever
        Assert.AreEqual("3\n2", SetupTests.RunWithInput(@"        N = 0
C       N = ?INPUT N + 1               :S(C)
        OUTPUT = N
END", "a\nb\nc") + "\n" + SetupTests.RunWithInput(@"        N = 0
C       ~INPUT                         :S(E)
        N = N + 1                      :(C)
E       OUTPUT = N
END", "a\nb"));
    }

    [TestMethod]
    public void TEST_InputEnd_the_line_read_is_the_variables_value()
    {
        // sbl -bf: each line read becomes the input variable's value, so after X = INPUT and Y = INPUT the
        // detached variable INPUT holds 'two' and DUMP lists it (testpgms' test6 dumps INPUT at the end);
        // snobol4dotnet left the variable null
        Assert.AreEqual("one two two", SetupTests.RunWithInput(@"        X = INPUT
        Y = INPUT
        DETACH(.INPUT)
        OUTPUT = X ' ' Y ' ' INPUT
END", "one\ntwo\nthree"));
    }

    [TestMethod]
    public void TEST_InputEnd_subscripts_and_arguments_fail_at_end_of_input()
    {
        // sbl -bf: an input-associated subscript or function argument reads a line, and at end of input the
        // statement fails, so T[W] = 0 :S(L) and OUTPUT = SIZE(W) :S(L) stop when the input does (the dotnet
        // package's asgn1 builds its dictionary so); snobol4dotnet never read the subscript and looped forever,
        // and ran the function on the failed read
        Assert.AreEqual("0 3,2", SetupTests.RunWithInput(@"        T = TABLE()
L       T[INPUT] = 0                   :S(L)
        OUTPUT = T['banana'] ' ' PROTOTYPE(CONVERT(T,'ARRAY'))
END", "apple\nbanana\ncherry"));
        Assert.AreEqual("5\n6", SetupTests.RunWithInput(@"L       OUTPUT = SIZE(INPUT)           :S(L)
END", "apple\nbanana").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_InputEnd_a_files_byte_order_mark_is_read()
    {
        // sbl -bf reads a file's bytes as they are: a UTF-8 byte-order mark stays part of the first line, so
        // the first line's key differs from the same name later (the dotnet package's 1brc counts station001
        // twice); snobol4dotnet's reader dropped the mark
        var file = Path.Combine(Path.GetTempPath(), "snobom_" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllBytes(file, [0xEF, 0xBB, 0xBF, (byte)'a', (byte)'b', (byte)'\n', (byte)'a', (byte)'b', (byte)'\n']);
        try
        {
            var build = SetupTests.SetupScript("-bf", $"""
                         INPUT(.F, 3, '{file}')
                         X = F
                         Y = F
                         R = DIFFER(X, Y) 'differ'
                 END
                 """);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
            Assert.AreEqual("\uFEFFab", ((StringVar)build.Execute!.IdentifierTable["X"]).Data);
        }
        finally { File.Delete(file); }
    }

    [TestMethod]
    public void TEST_InputEnd_f0_option_reads_standard_input()
    {
        // sbl -bf: INPUT(.X,5,' -f0'), no file name and the F 0 option, associates X with standard input
        // (Gimpel's poker reads its answers so); snobol4dotnet refused the specification (error 116)
        Assert.AreEqual("got one\ngot two", SetupTests.RunWithInputDirectives("-bf", @"        INPUT(.X,5,' -f0')   :F(BAD)
        OUTPUT = 'got ' X
        OUTPUT = 'got ' X   :(END)
BAD     OUTPUT = 'failed'
END", "one\ntwo\nthree").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_InputEnd_missing_file_fails_and_is_not_created()
    {
        // sbl -bf: INPUT of a file that does not exist fails and creates nothing; snobol4dotnet opened it with
        // OpenOrCreate, made an empty file and succeeded
        var file = Path.Combine(Path.GetTempPath(), "snomissing_" + Guid.NewGuid().ToString("N") + ".txt");
        var build = SetupTests.SetupScript("-bf", $"""
                         INPUT(.X, 7, '{file}')   :F(F)
                         R = 'opened'             :(END)
                 F       R = 'failed'
                 END
                 """);
        Assert.AreEqual("failed", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
        Assert.IsFalse(File.Exists(file));
    }
}
