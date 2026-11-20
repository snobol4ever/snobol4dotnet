using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Define
{

    [TestMethod]
    public void TEST_Define_locals_and_parameters_keep_their_associations()
    {
        // sbl -b: a local F associated with a file still reads it ([a,1] inside G, [b,2] after), and assigning a
        // parameter O associated with an output file writes there (aisnobol's LOADEX reads its library through
        // a local LIB.FILE); snobol4dotnet nulled the local's association and looped
        var dir = Path.Combine(Path.GetTempPath(), "snolocal_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var input = Path.Combine(dir, "idx.txt");
            var output = Path.Combine(dir, "o.txt");
            File.WriteAllText(input, "a,1\nb,2\n");
            var build = SetupTests.SetupScript("-b", $@"
        INPUT(.F, 15, '{input}')
        DEFINE('G()F')                 :(GE)
G       G = F                          :(RETURN)
GE      R = '[' G() ']'
        S = '[' F ']'
        OUTPUT('O', 16, '{output}')
        DEFINE('H(O)')                 :(HE)
H       O = 'via param'                :(RETURN)
HE      H('x')
        ENDFILE(16)
end");
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("[a,1] [b,2]", $"{build.Execute!.IdentifierTable["R"]} {build.Execute!.IdentifierTable["S"]}");
            Assert.AreEqual("via param\n", File.ReadAllText(output).Replace("\r\n", "\n"));
        }
        finally { Directory.Delete(dir, true); }
    }
}
