using Snobol4.Common;

namespace Test.ArraysTables;

[TestClass]
public class Compare
{
    [TestMethod] 
    public void TEST_Compare_Array_Array()
    {
        var v1 = new ArrayVar();
        var v2 = new ArrayVar();
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_Integer_Integer1()
    {
        var v1 = new IntegerVar(4);
        var v2 = new IntegerVar(6);
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_Integer_Integer2()
    {
        var v1 = new IntegerVar(4);
        var v2 = new IntegerVar(4);
        var c = v1.Compare(v2);
        Assert.AreEqual(0, c);
    }

    [TestMethod]
    public void TEST_Compare_Integer_Integer3()
    {
        var v1 = new IntegerVar(4);
        var v2 = new IntegerVar(2);
        var c = v1.Compare(v2);
        Assert.AreEqual(1, c);
    }

    [TestMethod]
    public void TEST_Compare_Integer_Real1()
    {
        var v1 = new IntegerVar(4);
        var v2 = new RealVar(6.5);
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_Integer_Real2()
    {
        var v1 = new IntegerVar(4);
        var v2 = new RealVar(4.0);
        var c = v1.Compare(v2);
        Assert.AreEqual(0, c);
    }

    [TestMethod]
    public void TEST_Compare_Integer_Real3()
    {
        var v1 = new IntegerVar(4);
        var v2 = new RealVar(2.5);
        var c = v1.Compare(v2);
        Assert.AreEqual(1, c);
    }

    [TestMethod]
    public void TEST_Compare_Real_Integer1()
    {
        var v1 = new RealVar(4);
        var v2 = new IntegerVar(6);
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_Real_Integer2()
    {
        var v1 = new RealVar(4);
        var v2 = new IntegerVar(4);
        var c = v1.Compare(v2);
        Assert.AreEqual(0, c);
    }

    [TestMethod]
    public void TEST_Compare_Real_Integer3()
    {
        var v1 = new RealVar(4);
        var v2 = new IntegerVar(2);
        var c = v1.Compare(v2);
        Assert.AreEqual(1, c);
    }

    [TestMethod]
    public void TEST_Compare_Real_Real1()
    {
        var v1 = new RealVar(4);
        var v2 = new RealVar(6.5);
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_Real_Real2()
    {
        var v1 = new IntegerVar(4);
        var v2 = new RealVar(4.0);
        var c = v1.Compare(v2);
        Assert.AreEqual(0, c);
    }

    [TestMethod]
    public void TEST_Compare_Real_Real3()
    {
        var v1 = new RealVar(4);
        var v2 = new RealVar(2.5);
        var c = v1.Compare(v2);
        Assert.AreEqual(1, c);
    }

    [TestMethod]
    public void TEST_Compare_String1()
    {
        var v1 = new StringVar("abcdef");
        var v2 = new StringVar("def");
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_String2()
    {
        var v1 = new StringVar("abc");
        var v2 = new StringVar("abc");
        var c = v1.Compare(v2);
        Assert.AreEqual(0, c);
    }

    [TestMethod]
    public void TEST_Compare_String3()
    {
        var v1 = new StringVar("defdef");
        var v2 = new StringVar("abc");
        var c = v1.Compare(v2);
        Assert.AreEqual(1, c);
    }

    [TestMethod]
    public void TEST_Compare_Array1()
    {
        var v1 = new ArrayVar();
        var v2 = new ArrayVar();
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

    [TestMethod]
    public void TEST_Compare_Array2()
    {
        var v1 = new ArrayVar();
        var v2 = v1;
        var c = v1.Compare(v2);
        Assert.AreEqual(0, c);
    }

    [TestMethod]
    public void TEST_Compare_Array3()
    {
        var v1 = new ArrayVar();
        var v2 = new ArrayVar();
        var c = v2.Compare(v1);
        Assert.AreEqual(1, c);
    }

    [TestMethod]
    public void TEST_Compare_Array_Code()
    {
        var v1 = new ArrayVar();
        var v2 = new CodeVar();
        var c = v1.Compare(v2);
        Assert.AreEqual(-1, c);
    }

}