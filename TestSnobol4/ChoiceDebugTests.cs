using Snobol4.Common;
using Test.TestLexer;

namespace Test.Debug;

[TestClass]
public class ChoiceDebugTests
{
    [TestMethod]
    public void Debug_RomanLabelTable()
    {
        // Temporarily disable threaded execution to let the normal path run
        // and print what LabelTable contains after Run() populates it
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
end");
        Console.WriteLine($"Errors: {build.ErrorCodeHistory.Count}");
        Console.WriteLine($"StatementCount: {build.StatementCount}");
        Console.WriteLine($"SourceLines.Count: {build.Code.SourceLines.Count}");
        Console.WriteLine($"StatementInstructionStarts.Length: {build.StatementInstructionStarts?.Length ?? -1}");
        Console.WriteLine("LabelTable:");
        foreach (var kv in build.Execute!.LabelTable)
            Console.WriteLine($"  '{kv.Key}' => {kv.Value}");
    }
}
