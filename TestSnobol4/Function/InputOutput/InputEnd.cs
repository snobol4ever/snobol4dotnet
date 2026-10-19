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
}
