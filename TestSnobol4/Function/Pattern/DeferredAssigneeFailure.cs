using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A conditional assignment whose deferred target ends in an error (22, a function not defined) or in a failure (its function returns by
/// FRETURN) makes the whole statement fail when &ERRLIMIT lets the run go on, so its :F is taken, and the assignments after it are not made.
/// The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DeferredAssigneeFailure
{
    [TestMethod]
    public void TEST_DeferredAssigneeFailure_an_error_in_the_target_fails_the_statement()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('b' . *FOO())  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_an_error_in_the_target_of_an_immediate_assignment_fails_the_statement()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('b' $ *FOO())  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_an_error_in_the_second_target_fails_the_statement()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('b' . X . *FOO(*1))  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_a_replacement_statement_fails_too()
    {
        var program = @"        &ERRLIMIT = 5
        S = 'abc'
        S ('b' . *FOO()) = 'z'  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_the_first_assignment_stops_the_commits()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('a' . *FOO() 'b' . X)  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE ' X=[' X ']'  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE ' X=[' X ']'
END";
        Assert.AreEqual(@"F err 22 X=[]", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_a_target_function_that_fails_fails_the_statement()
    {
        var program = @"        DEFINE('FOO()')   :(GO)
FOO     :(FRETURN)
GO      &ERRLIMIT = 5
        'abc' ('b' . *FOO())  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    // A deferred target is evaluated for a NAME: a call that returns a value (by RETURN, or a builtin's), a literal, an assignment, a
    // concatenation and the name operator each fail the match, after the call's side effects and only once (the console took . *G() and
    // $ *G() as matched, assigning G, and ended the run in error 212 at . *'ZZ'); $'ZZ' and an element are names
    // (rungs user_function_pos_defer_branch_2, user_function_defer_branch_3, user_function_len_defer_branch_6,
    // test_regress_deferred_nonname_capture_target_fails)
    [TestMethod]
    public void TEST_DeferredAssigneeFailure_a_target_that_gives_a_value_not_a_name_fails_the_match()
    {
        var program = @"        DEFINE('G()')
        DEFINE('MARK()')                    :(GE)
G       G = 'ZZ'                            :(RETURN)
MARK    FIRES = FIRES + 1
        MARK = ''                           :(RETURN)
GE      S = 'abcdefghi'
        A = ARRAY(1)
        N = 0
        FIRES = 0
        S 'a' . *G()                        :S(Y1)
        R = R 'n'                           :(E1)
Y1      R = R 'y'
E1      S 'b' $ *G()                        :S(Y2)
        R = R 'n'                           :(E2)
Y2      R = R 'y'
E2      S 'c' . *'ZZ'                       :S(Y3)
        R = R 'n'                           :(E3)
Y3      R = R 'y'
E3      S 'd' . *(N = N + 1)                :S(Y4)
        R = R 'n'                           :(E4)
Y4      R = R 'y'
E4      S 'e' . *(.ZZ)                      :S(Y5)
        R = R 'n'                           :(E5)
Y5      R = R 'y'
E5      S 'f' . *('Z' 'Z')                  :S(Y6)
        R = R 'n'                           :(E6)
Y6      R = R 'y'
E6      S 'g' . *DUPL('ZZ', 1)              :S(Y7)
        R = R 'n'                           :(E7)
Y7      R = R 'y'
E7      'X' ? (EPS . *MARK()) (EPS . *MARK()) :S(Y8)
        R = R 'n'                           :(E8)
Y8      R = R 'y'
E8      S 'h' . *$'ZZ'                      :S(Y9)
        R = R 'n'                           :(E9)
Y9      R = R 'y'
E9      S 'i' . *A<1>                       :S(Y10)
        R = R 'n'                           :(E10)
Y10     R = R 'y'
E10     OUTPUT = R ' N=' N ' FIRES=' FIRES ' ZZ=' ZZ ' A1=' A<1> ' G=' G
end";
        Assert.AreEqual(@"nnnnnnnnyy N=1 FIRES=1 ZZ=h A1=i G=", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
