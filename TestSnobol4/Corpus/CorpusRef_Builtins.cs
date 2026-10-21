using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Coverage for builtins with zero or near-zero prior test coverage:
///   $name        — indirect variable lookup by string name (VALUE(name) is not SPITBOL's: error 22)
///   CLEAR(skip)  — reinitialize all variables except skip list
///   COLLECT()    — GC hint (no crash, returns null)
///   CHAR(n)      — integer code point → one-character string
/// </summary>
[DoNotParallelize]
[TestClass]
public class CorpusRef_Builtins
{
    // ── VALUE and CONCAT are not SPITBOL's (CEO-1858: sbl -bf answers error 22; no extension) ─────

    [TestMethod]
    public void TEST_Builtin_value_is_undefined_22()
    {
        var s = @"
        X = 'hello'
        OUTPUT = VALUE('X')
        OUTPUT = 'after'
END";
        var build = SetupTests.SetupScript("-bf", s);
        Assert.AreEqual(22, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Builtin_concat_is_undefined_22()
    {
        var s = @"
        P = CONCAT('a', 'b')
        OUTPUT = 'after'
END";
        var build = SetupTests.SetupScript("-bf", s);
        Assert.AreEqual(22, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Builtin_value_as_data_field_still_defines()
    {
        // a program's own DATA field named VALUE is an ordinary field function (sbl -bf prints v)
        var s = @"
        DATA('CLUNK(VALUE)')
        C = CLUNK('v')
        OUTPUT = VALUE(C)
END";
        Assert.AreEqual("v", SetupTests.RunWithInputDirectives("-bf", s));
    }

    // ── indirection by name: SPITBOL spells it $ (these were VALUE(...) before CEO-1858) ─────────

    [TestMethod]
    public void TEST_Builtin_indirect_basic()
    {
        // $'X' looks up variable X by name
        var s = @"
        X = 'hello'
        DIFFER($'X', 'hello')                       :F(ok)
        OUTPUT = 'FAIL'                             :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Builtin_indirect_via_namevar()
    {
        // $NM where NM = .X is a name descriptor — deref to value
        var s = @"
        X = 'world'
        NM = .X
        DIFFER($NM, 'world')                        :F(ok)
        OUTPUT = 'FAIL'                             :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Builtin_indirect_string_name_of_integer_value()
    {
        // $N where N = 'A' names the variable A, whose value is the integer 42
        var s = @"
        A = 42
        N = 'A'
        DIFFER($N, 42)                              :F(ok)
        OUTPUT = 'FAIL'                             :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    // ── CLEAR ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Builtin_clear_resets_variable()
    {
        // CLEAR('OUTPUT') resets all user vars except OUTPUT
        var s = @"
        A = 'hello'
        B = 42
        CLEAR('OUTPUT')
        IDENT(A, '')                                :S(ok1)
        OUTPUT = 'FAIL: A not cleared'              :(END)
ok1     IDENT(B, '')                                :S(ok2)
        OUTPUT = 'FAIL: B not cleared'              :(END)
ok2     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Builtin_clear_skip_list()
    {
        // CLEAR('A,OUTPUT') resets all except A and OUTPUT
        var s = @"
        A = 'keep'
        B = 'lose'
        CLEAR('A,OUTPUT')
        DIFFER(A, 'keep')                           :F(ok1)
        OUTPUT = 'FAIL: A should be kept'           :(END)
ok1     IDENT(B, '')                                :S(ok2)
        OUTPUT = 'FAIL: B should be cleared'        :(END)
ok2     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    // ── COLLECT ────────────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Builtin_collect_no_crash()
    {
        // COLLECT() is a GC hint — must not crash and returns null
        var s = @"
        COLLECT()
        OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    // ── CHAR ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Builtin_char_basic()
    {
        // CHAR(65) = 'A', CHAR(97) = 'a'
        var s = @"
        DIFFER(CHAR(65), 'A')                       :F(ok1)
        OUTPUT = 'FAIL: CHAR(65)'                   :(END)
ok1     DIFFER(CHAR(97), 'a')                       :F(ok2)
        OUTPUT = 'FAIL: CHAR(97)'                   :(END)
ok2     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Builtin_char_digits()
    {
        // CHAR(48)='0', CHAR(57)='9'
        var s = @"
        DIFFER(CHAR(48), '0')                       :F(ok1)
        OUTPUT = 'FAIL: CHAR(48)'                   :(END)
ok1     DIFFER(CHAR(57), '9')                       :F(ok2)
        OUTPUT = 'FAIL: CHAR(57)'                   :(END)
ok2     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Builtin_char_size_one()
    {
        // CHAR always returns exactly 1 character
        var s = @"
        DIFFER(SIZE(CHAR(72)), 1)                   :F(ok)
        OUTPUT = 'FAIL: SIZE(CHAR(72)) != 1'        :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Builtin_char_roundtrip()
    {
        // Build a string using CHAR and verify with pattern
        var s = @"
        S = CHAR(72) CHAR(105)
        DIFFER(S, 'Hi')                             :F(ok)
        OUTPUT = 'FAIL: CHAR roundtrip'             :(END)
ok      OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }
}
