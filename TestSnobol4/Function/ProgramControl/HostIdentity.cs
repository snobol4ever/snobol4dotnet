using Snobol4.Common;
using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// HOST() with no argument answers the implementation's identity in SPITBOL's three colon-separated fields, the computer type, the
/// operating system and the implementation (sbl -bf: "x86-64:unix :Macro SPITBOL 15.01 #"); each engine names itself, and the corpus
/// masks the line by that shape, ^host\(\): [^:]+:[^:]+:.+$ (spitbol_x64_tests, spitbol_x32_tests). snobol4dotnet read the computer
/// type from PROCESSOR_ARCHITECTURE, which only Windows sets, so on Linux the first field was empty and the answer had four fields.
/// </summary>
[TestClass]
public class HostIdentity
{
    [TestMethod]
    public void TEST_HostIdentity_has_three_fields_none_empty()
    {
        var build = SetupTests.SetupScript("-b", @"        H = HOST()
END");
        var h = ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("H")]).Data;
        var fields = h.Split(':');
        Assert.AreEqual(3, fields.Length, h);
        foreach (var f in fields)
            Assert.IsFalse(string.IsNullOrWhiteSpace(f), h);
    }

    [TestMethod]
    public void TEST_HostIdentity_names_the_computer_type_first()
    {
        var build = SetupTests.SetupScript("-b", @"        H = HOST()
END");
        var h = ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("H")]).Data;
        var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
        var want = arch switch
        {
            System.Runtime.InteropServices.Architecture.X64 => "x86-64",
            System.Runtime.InteropServices.Architecture.X86 => "x86",
            _ => arch.ToString().ToLowerInvariant()
        };
        Assert.AreEqual(want, h.Split(':')[0], h);
    }
}
