using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SETEXIT with an argument that is no label of the program (a blank, one with a blank in it, an undefined name) is error 187, and the handler
/// that was set stays set and takes the error: it ran, and the handler is spent as after any error it takes. The console forgot the old handler
/// when the label was not there. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SetexitBadLabel
{
    [TestMethod]
    public void TEST_SetexitBadLabel_the_old_handler_takes_the_error()
    {
        var program = @"        &ERRLIMIT = 100
        SETEXIT(.H)
        X = SETEXIT('BAD LABEL')
        OUTPUT = 'after bad setexit ' X
        Y = SETEXIT()
        OUTPUT = 'handler now [' Y ']'
        :(END)
H       OUTPUT = 'handler ' &ERRTYPE ' ' &ERRTEXT
        :(CONTINUE)
END";
        Assert.AreEqual(@"handler 187 setexit argument is not label name or null
after bad setexit 
handler now []", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SetexitBadLabel_an_undefined_name_is_the_same()
    {
        var program = @"        &ERRLIMIT = 100
        SETEXIT(.H)
        X = SETEXIT(.NOLAB)
        Y = SETEXIT()
        OUTPUT = 'handler now [' Y ']'
        :(END)
H       OUTPUT = 'handler ' &ERRTYPE
        :(CONTINUE)
END";
        Assert.AreEqual(@"handler 187
handler now []", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SetexitBadLabel_without_a_handler_it_fails()
    {
        var program = @"        &ERRLIMIT = 100
        OUTPUT = 'r=[' SETEXIT(' X') ']'  :S(OK)F(NO)
OK      OUTPUT = 'ok ' &ERRTYPE  :(END)
NO      OUTPUT = 'fail ' &ERRTYPE
END";
        Assert.AreEqual(@"fail 187", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
