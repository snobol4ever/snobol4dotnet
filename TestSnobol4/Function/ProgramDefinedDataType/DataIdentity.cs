using Snobol4.Common;
using Test.TestLexer;

namespace Test.ProgramDefinedDataType;

/// <summary>
/// A DATA object keeps its identity through assignment, argument passing, field storage and use as
/// a table key: IDENT sees one object, as in SPITBOL (sbl -b; Budne's atn tests IDENT(X, nil)).
/// </summary>
[TestClass]
public class DataIdentity
{
    private static string Result(string body)
    {
        var build = SetupTests.SetupScript("-b", body + "\nend");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable["R"]).Data;
    }

    [TestMethod]
    public void TEST_DataIdentity_survives_assignment_and_field_storage()
    {
        Assert.AreEqual("ident ident", Result(@"
        DATA('STACK(TOP,REST)')
        NIL = STACK()
        T = REST(STACK('a', NIL))
        T2 = T
        R = (IDENT(T, NIL) 'ident', 'differ') ' ' (IDENT(T2, NIL) 'ident', 'differ')"));
    }

    [TestMethod]
    public void TEST_DataIdentity_survives_argument_passing()
    {
        Assert.AreEqual("null", Result(@"
        DATA('STACK(TOP,REST)')
        NIL = STACK()
        DEFINE('NULL(X)')                 :(NULL_END)
NULL    IDENT(X, NIL)                     :S(RETURN)F(FRETURN)
NULL_END
        R = (NULL(NIL) 'null', 'notnull')"));
    }

    [TestMethod]
    public void TEST_DataIdentity_one_object_is_one_table_key()
    {
        Assert.AreEqual("found", Result(@"
        DATA('STACK(TOP,REST)')
        T = TABLE()
        N = STACK()
        M = N
        T<N> = 'found'
        R = T<M>"));
    }
}
