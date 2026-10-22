using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The dump of level 3 lists the null variables too, and SPITBOL has a variable block for more names than the program assigns: the END
/// statement's label and its first letter E, each defined label and its first letter, and the name of every function the program calls. The
/// expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DumpNullVariables
{
    [TestMethod]
    public void TEST_DumpNullVariables_the_end_label_and_e_are_always_there()
    {
        var program = @"        X = 1
        DUMP(3)
END";
        Assert.AreEqual(@"


dump of natural variables

DUMP = ''
E = ''
END = ''
INPUT = ''
OUTPUT = ''
TERMINAL = ''
X = 1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 1
&LASTNO = 1
&LINE = 2
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 2
&STLIMIT = 2147483647
&STNO = 2
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DumpNullVariables_a_defined_label_and_its_first_letter()
    {
        var program = @"LONGNAME  X = 1
        DUMP(3)
END";
        Assert.AreEqual(@"


dump of natural variables

DUMP = ''
E = ''
END = ''
INPUT = ''
L = ''
LONGNAME = ''
OUTPUT = ''
TERMINAL = ''
X = 1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 1
&LASTNO = 1
&LINE = 2
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 2
&STLIMIT = 2147483647
&STNO = 2
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DumpNullVariables_labels_with_the_same_first_letter_give_one_block()
    {
        var program = @"AB      X = 1
AC      X = 2
BD      X = 3
        DUMP(3)
END";
        Assert.AreEqual(@"


dump of natural variables

A = ''
AB = ''
AC = ''
B = ''
BD = ''
DUMP = ''
E = ''
END = ''
INPUT = ''
OUTPUT = ''
TERMINAL = ''
X = 3


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 3
&LASTNO = 3
&LINE = 4
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 4
&STLIMIT = 2147483647
&STNO = 4
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DumpNullVariables_a_label_referenced_but_not_defined_has_no_first_letter()
    {
        var program = @"        &ERRLIMIT = 5
        X = 1  :F(NOLAB)
        DUMP(3)
END";
        Assert.AreEqual(@"


dump of natural variables

DUMP = ''
E = ''
END = ''
ERRLIMIT = ''
INPUT = ''
NOLAB = ''
OUTPUT = ''
TERMINAL = ''
X = 1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 5
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 3
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DumpNullVariables_the_names_of_called_functions_are_listed()
    {
        var program = @"        X = LEN(1) SIZE('ab')
        DUMP(3)
END";
        Assert.AreEqual(@"


dump of natural variables

DUMP = ''
E = ''
END = ''
INPUT = ''
LEN = ''
OUTPUT = ''
SIZE = ''
TERMINAL = ''
X = PATTERN


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 1
&LASTNO = 1
&LINE = 2
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = ''
&STCOUNT = 2
&STLIMIT = 2147483647
&STNO = 2
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DumpNullVariables_a_defined_function_and_its_label()
    {
        var program = @"        DEFINE('FN()')
        X = FN()
        DUMP(3)
        :(END)
FN      FN = 1  :(RETURN)
END";
        Assert.AreEqual(@"


dump of natural variables

DEFINE = ''
DUMP = ''
E = ''
END = ''
F = ''
FN = ''
INPUT = ''
OUTPUT = ''
RETURN = ''
TERMINAL = ''
X = 1


dump of keyword values

&ANCHOR = 0
&CASE = 1
&CODE = 0
&DUMP = 0
&ERRLIMIT = 0
&ERRTEXT = ''
&ERRTYPE = 0
&FILE = 'Test.sno'
&FNCLEVEL = 0
&FTRACE = 0
&FULLSCAN = 1
&INPUT = 1
&LASTFILE = 'Test.sno'
&LASTLINE = 2
&LASTNO = 2
&LINE = 3
&MAXLNGTH = 16777216
&OUTPUT = 1
&PROFILE = 0
&RTNTYPE = 'RETURN'
&STCOUNT = 4
&STLIMIT = 2147483647
&STNO = 3
&TRACE = 0
&TRIM = 1", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
