using Test.TestLexer;

namespace Test.ObjectCreation;

/// <summary>
/// COPY makes a new array, table or DATA object one level deep, as SPITBOL does: its own elements or
/// fields, while nested arrays, tables and DATA objects stay shared with the original.
/// </summary>
[TestClass]
public class CopyDepth
{
    [TestMethod]
    public void TEST_Copy_is_one_level_deep()
    {
        // sbl -b answers a changed deep / differ / inner s t / ti []
        var build = SetupTests.SetupScript("-b", @"
        DATA('NODE(VAL,NXT)')
        N = NODE('a', NODE('b'))
        C = COPY(N)
        VAL(C) = 'changed'
        VAL(NXT(C)) = 'deep'
        R1 = VAL(N) ' ' VAL(C) ' ' VAL(NXT(N))
        R2 = DIFFER(N,C) 'differ'
        A = ARRAY(2)
        A<1> = ARRAY(2)
        A<2> = 's'
        CA = COPY(A)
        CA<1><1> = 'inner'
        CA<2> = 't'
        R3 = A<1><1> ' ' A<2> ' ' CA<2>
        T = TABLE()
        T<'k'> = ARRAY(1)
        CT = COPY(T)
        CT<'k'><1> = 'ti'
        CT<'j'> = 'new'
        R4 = T<'k'><1> ' [' T<'j'> ']'
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("a changed deep|differ|inner s t|ti []",
            $"{build.Execute!.IdentifierTable["R1"]}|{build.Execute!.IdentifierTable["R2"]}|{build.Execute!.IdentifierTable["R3"]}|{build.Execute!.IdentifierTable["R4"]}");
    }
}
