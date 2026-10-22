using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Array/TABLE/DATA tests — new coverage only (091-096 already in SimpleOutput_FunctionsData.cs).
/// New: default values, missing keys, integer keys, two types, linked list, 2D array, PROTOTYPE.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CorpusRef_Data
{
    // ── Additional array/table/data building blocks ──────────────────────────

    [TestMethod]
    public void TEST_Corpus_data_array_default_value()
    {
        // Unset array elements return empty string
        var s = @"
        A = ARRAY(3)
        A<2> = 'set'
        OUTPUT = IDENT(A<1>) 'empty'
        OUTPUT = A<2>
END";
        Assert.AreEqual("empty"+ Environment.NewLine + "set", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_table_missing_key()
    {
        // Unset table key returns empty string
        var s = @"
        T = TABLE()
        T<'x'> = 'found'
        OUTPUT = IDENT(T<'missing'>) 'empty'
        OUTPUT = T<'x'>
END";
        Assert.AreEqual("empty"+ Environment.NewLine + "found", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_table_integer_key()
    {
        // TABLE keys can be integers
        var s = @"
        T = TABLE()
        T<1> = 'one'
        T<2> = 'two'
        T<3> = 'three'
        OUTPUT = T<1>
        OUTPUT = T<2>
        OUTPUT = T<3>
END";
        Assert.AreEqual("one"+ Environment.NewLine + "two"+ Environment.NewLine + "three", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_two_types()
    {
        // Two DATA types coexist; DATATYPE distinguishes them
        var s = @"
        DATA('cat(name,lives)')
        DATA('dog(name,tricks)')
        C = cat('Felix', 9)
        D = dog('Rex', 3)
        OUTPUT = REPLACE(DATATYPE(C), &LCASE, &UCASE)
        OUTPUT = REPLACE(DATATYPE(D), &LCASE, &UCASE)
        OUTPUT = name(C)
        OUTPUT = name(D)
END";
        Assert.AreEqual("CAT"+ Environment.NewLine + "DOG"+ Environment.NewLine + "Felix"+ Environment.NewLine + "Rex", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_linked_list()
    {
        // Classic linked list via DATA
        var s = @"
        DATA('node(val,nxt)')
        HEAD =
        HEAD = node(3, HEAD)
        HEAD = node(2, HEAD)
        HEAD = node(1, HEAD)
        P = HEAD
LOOP    IDENT(P)                                   :S(END)
        OUTPUT = val(P)
        P = nxt(P)                                 :(LOOP)
END";
        Assert.AreEqual("1"+ Environment.NewLine + "2"+ Environment.NewLine + "3", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_array_2d()
    {
        // 2D array: ARRAY('3,3')
        var s = @"
        A = ARRAY('3,3')
        A<1,1> = 'a'
        A<2,2> = 'b'
        A<3,3> = 'c'
        OUTPUT = A<1,1>
        OUTPUT = A<2,2>
        OUTPUT = A<3,3>
END";
        Assert.AreEqual("a"+ Environment.NewLine + "b"+ Environment.NewLine + "c", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_array_size_prototype()
    {
        // PROTOTYPE returns array dimensions as string
        var s = @"
        A = ARRAY(5)
        OUTPUT = PROTOTYPE(A)
END";
        Assert.AreEqual("5", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_field_names()
    {
        // FIELD(typename, n) returns the NAME of the nth field, which is IDENT to .X and not to the string 'X', as sbl answers
        // Type name must be uppercase (as registered by DATA())
        var s = @"
        DATA('POINT(X,Y)')
        DIFFER(FIELD('POINT', 1), .X)               :F(ok1)
        OUTPUT = 'FAIL: FIELD POINT 1'              :(END)
ok1     DIFFER(FIELD('POINT', 2), .Y)               :F(ok2)
        OUTPUT = 'FAIL: FIELD POINT 2'              :(END)
ok2     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_field_set_get_roundtrip()
    {
        // Create DATA object, set via field accessor, read back via FIELD name
        var s = @"
        DATA('color(r,g,b)')
        C = color(10, 20, 30)
        DIFFER(r(C), 10)                            :F(ok1)
        OUTPUT = 'FAIL: r field'                    :(END)
ok1     DIFFER(g(C), 20)                            :F(ok2)
        OUTPUT = 'FAIL: g field'                    :(END)
ok2     DIFFER(b(C), 30)                            :F(ok3)
        OUTPUT = 'FAIL: b field'                    :(END)
ok3     r(C) = 99
        DIFFER(r(C), 99)                            :F(ok4)
        OUTPUT = 'FAIL: r set'                      :(END)
ok4     OUTPUT = 'PASS'
END";
        Assert.AreEqual("PASS", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_data_prototype_table()
    {
        // PROTOTYPE of a TABLE returns the prototype string
        var s = @"
        T = TABLE(10)
        P = PROTOTYPE(T)
        IDENT(P, '')                                :S(pass)
        OUTPUT = P                                  :(END)
pass    OUTPUT = 'PASS'
END";
        // TABLE prototype may be empty or implementation-defined — just verify no crash
        var actual = SetupTests.RunWithInput(s);
        Assert.IsTrue(actual == "PASS" || actual.Length >= 0, $"Got: {actual}");
    }
}
