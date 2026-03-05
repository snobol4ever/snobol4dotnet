using Test.TestLexer;

namespace Test.InputOutput;

[TestClass]
public class InputOutput
{
    [TestMethod]
    public void TEST_113_001()
    {
        var s = @"
        input('a','1','a' | 'b')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(113, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_114_001()
    {
        var s = @"    input('a','a' | 'b','file78.txt')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(114, build.ErrorCodeHistory[0]);
    }



    [TestMethod]
    public void TEST_115_001()
    {
        var s = @"
        input('a' | 'b','1','file25.txt')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(115, build.ErrorCodeHistory[0]);
    }

    

    [TestMethod]
    public void TEST_116_001()
    {
        var s = @"
        input('a','1','test1.txt', -1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_116_002()
    {
        var s = @"
        input('a','1','test1.txt', 7)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_116_003()
    {
        var s = @"
        input('a','1','test1.txt',,7)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }
    
    [TestMethod]
    public void TEST_116_004()
    {
        var s = @"
        input('a','1','test1.txt','a')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_116_005()
    {
        var s = @"
        input('a','1','test1.txt',,6)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_116_006()
    {
        var s = @"
        input('a','1','test1.txt',,6)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_116_007()
    {
        var s = @"
        input('a','1','test1.txt',,'a')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }
    
    [TestMethod]
    public void TEST_116_008()
    {
        var s = @"
        input('a','1','test1.txt','a')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

                [TestMethod]
    public void TEST_157_001()
    {
        var s = @"
        output('a','1','a' | 'b')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(157, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_158_001()
    {
        var s = @"    output('a','a' | 'b','file78.txt')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(158, build.ErrorCodeHistory[0]);
    }



    [TestMethod]
    public void TEST_159_001()
    {
        var s = @"
        output('a' | 'b','1','file25.txt')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(159, build.ErrorCodeHistory[0]);
    }



    [TestMethod]
    public void TEST_160_001()
    {
        var s = @"
        output('a','1','test1.txt', -1)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_002()
    {
        var s = @"
        output('a','1','test1.txt', 7)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_003()
    {
        var s = @"
        output('a','1','test1.txt',,7)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_004()
    {
        var s = @"
        output('a','1','test1.txt','a')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_005()
    {
        var s = @"
        output('a','1','test1.txt',,6)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_006()
    {
        var s = @"
        output('a','1','test1.txt',,6)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_007()
    {
        var s = @"
        output('a','1','test1.txt',,'a')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_160_008()
    {
        var s = @"
        output('a','1','test1.txt','a')
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(160, build.ErrorCodeHistory[0]);
    }

    // Hardcoded Windows file path - hangs on Linux waiting for non-existent input file.
    [TestMethod, Ignore]
    public void TEST_Input_001()
    {
        var testFile = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\TestInput.txt";
        if (SetupTests.IsLinux)
            testFile = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Output/TestInput.txt";

        var s = $@"
        input('input','2','{testFile}')
	    a = -input
	    b = input - input
	    c = atan(input)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("-123", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
        Assert.AreEqual("-93", build.Execute!.IdentifierTable[build.FoldCase("b")].ToString());
        Assert.AreEqual("1.1902899496825317", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void TEST_Output_String_001()
    {
        var s = @"
        output = 'test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("test", build.Execute!.IdentifierTable[build.FoldCase("output")].ToString());
    }

    [TestMethod]
    public void TEST_Output_String_002()
    {
        var s = @"
        a = output = 'test'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("test", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_Output_Integer_001()
    {
        var s = @"
        output = 123
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("123", build.Execute!.IdentifierTable[build.FoldCase("output")].ToString());
    }

    [TestMethod]
    public void TEST_Output_Integer_002()
    {
        var s = @"
        a = output = 123
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("123", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void TEST_Output_Real_001()
    {
        var s = @"
        output = 123.456
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("123.456", build.Execute!.IdentifierTable[build.FoldCase("output")].ToString());
    }

    [TestMethod]
    public void TEST_Output_Real_002()
    {
        var s = @"
        a = output = 123.456
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("123.456", build.Execute!.IdentifierTable[build.FoldCase("a")].ToString());
    }

    [TestMethod]
    public void Test00000()
    {
        // error 289 -- input channel currently in use
        var s = @"
        output('a','1','file168.txt')
        input('a','1','file168.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        Console.Error.WriteLine(ToString());
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test00001()
    {
        // error 289 -- input channel currently in use
        var s = @"
        output('a','1','file186.txt')
        input('b','1','file186.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test00100()
    {
        // error 290 -- output channel currently in use
        var s = @"
        output('a','1','file240.txt')
        output('a','1','file240.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test00101()
    {
        // error 290 -- output channel currently in use
        var s = @"
        output('a','1','file258.txt')
        output('b','1','file258.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test01000()
    {
        // error 289 -- input channel currently in use
        var s = @"
        output('a','1','file312.txt')
        input('a','1','file312.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test01001()
    {
        // error 289 -- input channel currently in use
        var s = @"
        output('a','1','file330.txt')
        input('b','1','file331.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test01100()
    {
        // error 290 -- output channel currently in use
        var s = @"
        output('a','1','file384.txt')
        output('a','1','file385.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test01101()
    {
        // error 290 -- output channel currently in use
        var s = @"
        output('a','1','file402.txt')
        output('b','1','file403.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test01110()
    {
        // success
        var s = @"
        output('a','1','file420.txt')
        output('a','2','file421.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test01111()
    {
        // success
        var s = @"
        output('a','1','file438.txt')
        output('b','2','file438.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test02000()
    {
        // error 117 -- input file cannot be read
        var s = @"
        output('a','1','fil456.txt')
        input('a','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void Test02001()
    {
        // error 117 -- input file cannot be read
        var s = @"
        output('a','1','file474.txt')
        input('b','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void Test02010()
    {
        // error 116 -- inappropriate file specification for input
        var s = @"
        output('a','1','file491.txt')
        input('a','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test02011()
    {
        // error 116 -- inappropriate file specification for input
        var s = @"
        output('a','1','file508.txt')
        input('b','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test02100()
    {
        // success
        var s = @"
        output('a','1','file537.txt')
        output('a','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test02101()
    {
        // success
        var s = @"
        output('a','1','file544.txt')
        output('b','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test02110()
    {
        var s = @"
        output('a','1','file562.txt')
        output('a','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("+console-output", build.Execute!.IdentifierTable[build.FoldCase("a")].OutputChannel);
    }

    [TestMethod]
    public void Test02111()
    {
        var s = @"
        output('a','1','file580.txt')
        output('b','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("+console-output", build.Execute!.IdentifierTable[build.FoldCase("b")].OutputChannel);
    }

    [TestMethod]
    public void Test10000()
    {
        // error 289 -- input channel currently in use
        var s = @"
        input('a','1','file598.txt')
        input('a','1','file598.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test10001()
    {
        // error 289 -- input channel currently in use
        var s = @"
        input('a','1','file616.txt')
        input('b','1','file616.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test10100()
    {
        // error 290 -- output channel currently in use
        var s = @"
        input('a','1','file670.txt')
        output('a','1','file670.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test10101()
    {
        // error 290 -- output channel currently in use
        var s = @"
        input('a','1','file688.txt')
        output('b','1','file688.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test11000()
    {
        // error 289 -- input channel currently in use
        var s = @"
        input('a','1','file742.txt')
        input('a','1','file742.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test11001()
    {
        // error 289 -- input channel currently in use
        var s = @"
        input('a','1','file760.txt')
        input('b','1','file761.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(289, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test11100()
    {
        // error 290 -- output channel currently in use
        var s = @"
        input('a','1','file814.txt')
        output('a','1','file815.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test11101()
    {
        // error 290 -- output channel currently in use
        var s = @"
        input('a','1','file832.txt')
        output('b','1','file833.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(290, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test11110()
    {
        // success
        var s = @"
        input('a','1','file850.txt')
        output('a','2','file851.txt')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test11111()
    {
        // success
        var s = @"
        input('a','1','file868.txt')
        output('b','2','file869.txt')      :s(y)f(n)
y       c = 'success'  :(end)
n       c = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test12000()
    {
        // success
        var s = @"
        input('a','1','file886.txt')
        input('a','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test12001()
    {
        // success
        var s = @"
        input('a','1','file904.txt')
        input('b','1')      :s(y)f(n)
y       c = 'success'  :(end)
n       c = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", build.Execute!.IdentifierTable[build.FoldCase("c")].ToString());
    }

    [TestMethod]
    public void Test12010()
    {
        // error 116 -- inappropriate file specification for input
        var s = @"
        input('a','1','file922.txt')
        input('a','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test12011()
    {
        // error 116 -- inappropriate file specification for input
        var s = @"
        input('a','1','file941.txt')
        input('b','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(116, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void Test12100()
    {
        // error 161 -- output file cannot be written to
        var s = @"
        input('a','1','file959.txt')
        output('a','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void Test12101()
    {
        // error 161 -- output file cannot be written to
        var s = @"
        input('a','1','file976.txt')
        output('b','1')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void Test12110()
    {
        var s = @"
        input('a','1','file992.txt')
        output('a','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("+console-output", build.Execute!.IdentifierTable[build.FoldCase("a")].OutputChannel);
    }

    [TestMethod]
    public void Test12111()
    {
        var s = @"
        input('a','1','file1010.txt')
        output('b','2')      :s(y)f(n)
y       c  = 'success'  :(end)
n       c  = 'failure'
end
";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("+console-output", build.Execute!.IdentifierTable[build.FoldCase("b")].OutputChannel);
    }



}