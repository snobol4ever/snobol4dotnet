using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// CODE's statements are numbered on from the program's last statement, each block ending in an implicit
/// statement that takes a number of its own, and EVAL takes none, as SPITBOL numbers them.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeStatementNumbers
{
    [TestMethod]
    public void TEST_CodeStatementNumbers_follow_the_program()
    {
        // sbl -b answers 13 14 3 / 16 6 16 / E=7 / 18 for this twelve-statement program
        Assert.AreEqual("13 14 3\n16 6 16\nE=7\n18", SetupTests.RunWithInput(@"        C1 = CODE(' A = &STNO ; B = &STNO :(R1)')
        :<C1>
R1      OUTPUT = A ' ' B ' ' &STNO
        C2 = CODE(' D = &STNO :(R2)')
        :<C2>
R2      OUTPUT = D ' ' &STNO ' ' &LASTNO
        E = EVAL('&STNO')
        OUTPUT = 'E=' E
        C3 = CODE(' F = &STNO :(R3)')
        :<C3>
R3      OUTPUT = F
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_CodeStatementNumbers_failed_compile_is_quiet()
    {
        // sbl -b answers "13 226 syntax error: missing right paren 233 226 16": CODE and CONVERT to CODE or
        // EXPRESSION of text that does not compile fail, printing nothing, with &ERRTYPE and &ERRTEXT set, and
        // a failed block numbers only the statements before the one in error (B = 1 takes 15, so the next
        // block starts at 16). snobol4dotnet printed each diagnostic, left &ERRTYPE 0 and numbered every
        // statement of the failed block and its implicit end (aisnobol's TEST)
        var build = SetupTests.SetupScript("-b", """
                 C1 = CODE(' A = &STNO :(R1)')  :<C1>
         R1      R = A
                 X = CODE(' B = 1 ; Y = (')     :S(END)
                 R = R ' ' &ERRTYPE ' ' &ERRTEXT
                 X = CONVERT('a=b+c?','EXPRESSION')  :S(END)
                 R = R ' ' &ERRTYPE
                 X = CONVERT(' Y = (','CODE')   :S(END)
                 R = R ' ' &ERRTYPE
                 C2 = CODE(' A = &STNO :(R2)')  :<C2>
         R2      R = R ' ' A
                 TERMINAL = R
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("13 226 syntax error: missing right paren 233 226 16", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }

    [TestMethod]
    public void TEST_CodeStatementNumbers_blank_statements()
    {
        // sbl -b answers 2 6 8 10 12 / 20 25 27: a blank statement between semicolons, a blank line and an
        // empty line take numbers; nothing after a final semicolon does, and a source line's trailing blanks
        // are trimmed, while CODE text keeps them, so CODE(' X ; ') ends in a blank statement (aisnobol's DEXP
        // builds every function with such text). snobol4dotnet numbered a final empty segment in the program
        // and no blank statement in CODE
        Assert.AreEqual("2 6 8 10 12 20 25 27", SetupTests.RunWithInput(" X = 1 ; \n OUTPUT = &STNO\n" +
            " X = 1 ; ; Y = 2\n OUTPUT = &STNO\n \n OUTPUT = &STNO\n\n OUTPUT = &STNO\n X = 1 ;\n OUTPUT = &STNO\n" +
            " C1 = CODE(' A = &STNO :(R1) ; ; ')  :<C1>\nR1 OUTPUT = A\n" +
            " C2 = CODE(' ; A = &STNO :(R2)')  :<C2>\nR2 OUTPUT = A\n" +
            " C3 = CODE(' A = &STNO :(R3) ;')  :<C3>\nR3 OUTPUT = A\nEND").Replace("\r\n", "\n").Replace("\n", " "));
    }
}
