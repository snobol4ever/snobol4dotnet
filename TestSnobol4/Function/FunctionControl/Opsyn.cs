using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Opsyn
{

    [TestMethod]
    public void TEST_Opsyn_001()
    {
        var dllName = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\CustomFunction\bin\Debug\net10.0\AreaLibrary.dll";
        if (SetupTests.IsLinux)
            dllName = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/CustomFunction/bin/Debug/net10.0/AreaLibrary.dll";

        var s = $"""

                         load('{dllName}', 'AreaFunction.Area')
                         opsyn('#','AreaOfCircle',1)
                         opsyn('%','AreaOfSquare',1)
                         r1 = 'Area of circle with radius ' 4.5 ' is ' #4.5
                         r2 = 'Area of square with side  ' 15.9 ' is ' %15.9
                         unload('{dllName}')
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("Area of circle with radius 4.5 is 63.61725123519331", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("Area of square with side  15.9 is 252.81", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_002()
    {
        var s = """

                        opsyn('LENGTH','size')
                        r1 = LENGTH('rabbit')
                        opsyn('LENGTH','datatype')
                        r2 = LENGTH('bunny')
                        opsyn('LENGTH','size',0)
                        r3 = LENGTH('rabbit')
                        opsyn('LENGTH','datatype',0)
                        r4 = LENGTH('bunny')
                        opsyn('#','size',1);   
                        r5 = #'rabbit'
                        opsyn('#','datatype',1)
                        r6 = #'bunny'
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(6, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("r1")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r2")]).Data);
        Assert.AreEqual(6, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("r3")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r4")]).Data);
        Assert.AreEqual(6, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("r5")]).Data);
        Assert.AreEqual("string", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r6")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_003()
    {
        var s = """

                        opsyn('%','differ',2)
                        1 % 1.0 :s(y)f(n)
                y       r = 'success' :(end)
                n       r = 'failure'
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_004()
    {
        var s = """

                        opsyn('%','differ',2)
                        1 % 1 :s(y)f(n)
                y       r = 'success' :(end)
                n       r = 'failure'
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("failure", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_005()
    {
        var s = """

                        opsyn('%','differ',2)
                        1 % 2 :s(y)f(n)
                y       r = 'success' :(end)
                n       r = 'failure'
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_006()
    {
        var s = """

                	    define('ucase(s)')
                	    opsyn('!','ucase',1)	:(ucase_end)
                ucase   ucase = replace(s,&lcase, &ucase) :(return)
                ucase_end
                        output = datatype(&lcase)
                	    r = ucase('spitbol is very fast.')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("SPITBOL IS VERY FAST.", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_007()
    {
        var s = """

                    	opsyn('!', 'any', 1)
                	    'abc321'  ? !'3c' . r
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("c", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data);
    }

    [TestMethod]
    public void TEST_Opsyn_008()
    {
        var s = """

                    	opsyn('+', 'plus',2)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_009()
    {
        var s = """

                    	opsyn('!','any','a')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(152, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_010()
    {
        var s = """

                    	opsyn('!','any',3)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(153, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_011()
    {
        var s = """

                    	opsyn('!','any',-1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(153, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_012()
    {
        var s = """

                    	opsyn('!',any('123'))
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_013()
    {
        var s = """

                    	opsyn('#','anyone')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_014()
    {
        var s = """

                    	opsyn('#','+')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_015()
    {
        var s = """

                    	opsyn('#','any')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(155, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Opsyn_022()
    {
        var s = """

                    	opsyn('!',any('123'))
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_023()
    {
        var s = """

                    	opsyn('#','anyone')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_024()
    {
        var s = """

                    	opsyn('#','+')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_025()
    {
        var s = """

                    	opsyn('#','any')
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(155, build.ErrorCodeHistory[0]);
    }



    [TestMethod]
    public void TEST_Opsyn_032()
    {
        var s = """

                    	opsyn('anyone',any('123',1))
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_033()
    {
        var s = """

                    	opsyn('anyone','anyone',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_034()
    {
        var s = """

                    	opsyn('anyone','+',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_035()
    {
        var s = """

                    	opsyn('anyone','any',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(156, build.ErrorCodeHistory[0]);
    }




    [TestMethod]
    public void TEST_Opsyn_042()
    {
        var s = """

                    	opsyn('anyone',any('123',1))
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_043()
    {
        var s = """

                    	opsyn('anyone','anyone',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_044()
    {
        var s = """

                    	opsyn('anyone','+',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_045()
    {
        var s = """

                    	opsyn('anyone','any',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(156, build.ErrorCodeHistory[0]);
    }


    [TestMethod]
    public void TEST_Opsyn_052()
    {
        var s = """

                    	opsyn('%',any('123',1))
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_053()
    {
        var s = """

                    	opsyn('%','%',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_054()
    {
        var s = """

                    	opsyn('%','+',1)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_055()
    {
        var s = """

                    	opsyn('%','any',2)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Opsyn_062()
    {
        var s = """

                    	opsyn('%',any('123',2))
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_063()
    {
        var s = """

                    	opsyn('%','#',2)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_064()
    {
        var s = """

                    	opsyn('%','+',2)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(154, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Opsyn_065()
    {
        var s = """

                    	opsyn('%','any',2)
                end
                """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }


}


