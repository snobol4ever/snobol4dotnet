using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.ArraysTables;

[TestClass]
public class ReverseSort
{

    [TestMethod]
    public void TEST_ReverseSort001()
    {
        var s = @"
		a = array('15,4')

		a<1,1> = 'LE1;K3'
		a<2,1> = ')1B!M/'
		a<3,1> = '^60MN '
		a<4,1> = '^&!I(G'
		a<5,1> = 'G#Q*OR'
		a<6,1> = '/UK:@U'
		a<7,1> = '1PB*Q6'
		a<8,1> = 'J6JD<:'
		a<9,1> = ')@990%'
		a<10,1> = ':F.@HY'
		a<11,1> = 'E%$_H;'
		a<12,1> = 'W,DSJV'
		a<13,1> = 'XH$Q$C'
		a<14,1> = 'YB3IRM'
		a<15,1> = 'TGL, ='

		a<1,2> = -322.949
		a<2,2> = -218.23
		a<3,2> = -496.834
		a<4,2> = 323.164
		a<5,2> = -74.752
		a<6,2> = 258.34
		a<7,2> = 178.413
		a<8,2> = 93.172
		a<9,2> = 117.829
		a<10,2> = 279.952
		a<11,2> = -409.301
		a<12,2> = -63.012
		a<13,2> = -318.769
		a<14,2> = -458.497
		a<15,2> = 339.188

		a<1,3> = -22748
		a<2,3> = 4490
		a<3,3> = -12061
		a<4,3> = 39784
		a<5,3> = 39306
		a<6,3> = -46890
		a<7,3> = -37241
		a<8,3> = -8910
		a<9,3> = 48338
		a<10,3> = 2877
		a<11,3> = 25808
		a<12,3> = -4626
		a<13,3> = -1974
		a<14,3> = -47059
		a<15,3> = 45064

        a<1,4> = 1
		a<2,4> = 2
		a<3,4> = 3
		a<4,4> = 4
		a<5,4> = 5
		a<6,4> = 6
		a<7,4> = 7
		a<8,4> = 8
		a<9,4> = 9
		a<10,4> = 10
		a<11,4> = 11
		a<12,4> = 12
		a<13,4> = 13
		a<14,4> = 14
		a<15,4> = 15

		b = rsort(a,1)

        i = 1;
        r = ''
        d = ''
loop    output = b<i,1> '  ' b<i,4> 
        r = r ' ' b<i,4>
        d = d ' ' b<i,1>
        i = i + 1
        le(i,15) :s(loop)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(" 14 13 12 15 1 8 5 11 7 3 4 6 2 9 10", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_ReverseSort002()
    {
        var s = @"
		a = array('15,4')

		a<1,1> = 'LE1;K3'
		a<2,1> = ')1B!M/'
		a<3,1> = '^60MN '
		a<4,1> = '^&!I(G'
		a<5,1> = 'G#Q*OR'
		a<6,1> = '/UK:@U'
		a<7,1> = '1PB*Q6'
		a<8,1> = 'J6JD<:'
		a<9,1> = ')@990%'
		a<10,1> = ':F.@HY'
		a<11,1> = 'E%$_H;'
		a<12,1> = 'W,DSJV'
		a<13,1> = 'XH$Q$C'
		a<14,1> = 'YB3IRM'
		a<15,1> = 'TGL, ='

		a<1,2> = -322.949
		a<2,2> = -218.23
		a<3,2> = -496.834
		a<4,2> = 323.164
		a<5,2> = -74.752
		a<6,2> = 258.34
		a<7,2> = 178.413
		a<8,2> = 93.172
		a<9,2> = 117.829
		a<10,2> = 279.952
		a<11,2> = -409.301
		a<12,2> = -63.012
		a<13,2> = -318.769
		a<14,2> = -458.497
		a<15,2> = 339.188

		a<1,3> = -22748
		a<2,3> = 4490
		a<3,3> = -12061
		a<4,3> = 39784
		a<5,3> = 39306
		a<6,3> = -46890
		a<7,3> = -37241
		a<8,3> = -8910
		a<9,3> = 48338
		a<10,3> = 2877
		a<11,3> = 25808
		a<12,3> = -4626
		a<13,3> = -1974
		a<14,3> = -47059
		a<15,3> = 45064

        a<1,4> = 1
		a<2,4> = 2
		a<3,4> = 3
		a<4,4> = 4
		a<5,4> = 5
		a<6,4> = 6
		a<7,4> = 7
		a<8,4> = 8
		a<9,4> = 9
		a<10,4> = 10
		a<11,4> = 11
		a<12,4> = 12
		a<13,4> = 13
		a<14,4> = 14
		a<15,4> = 15

		b = rsort(a,2)

        i = 1;
        r = ''
        d = ''
loop    output = b<i,2> '  ' b<i,4> 
        r = r ' ' b<i,4>
        d = d ' ' b<i,1>
        i = i + 1
        le(i,15) :s(loop)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(" 15 4 10 6 7 9 8 12 5 2 13 1 11 14 3", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

    [TestMethod]
    public void TEST_ReverseSort003()
    {
        var s = @"
		a = array('15,4')

		a<1,1> = 'LE1;K3'
		a<2,1> = ')1B!M/'
		a<3,1> = '^60MN '
		a<4,1> = '^&!I(G'
		a<5,1> = 'G#Q*OR'
		a<6,1> = '/UK:@U'
		a<7,1> = '1PB*Q6'
		a<8,1> = 'J6JD<:'
		a<9,1> = ')@990%'
		a<10,1> = ':F.@HY'
		a<11,1> = 'E%$_H;'
		a<12,1> = 'W,DSJV'
		a<13,1> = 'XH$Q$C'
		a<14,1> = 'YB3IRM'
		a<15,1> = 'TGL, ='

		a<1,2> = -322.949
		a<2,2> = -218.23
		a<3,2> = -496.834
		a<4,2> = 323.164
		a<5,2> = -74.752
		a<6,2> = 258.34
		a<7,2> = 178.413
		a<8,2> = 93.172
		a<9,2> = 117.829
		a<10,2> = 279.952
		a<11,2> = -409.301
		a<12,2> = -63.012
		a<13,2> = -318.769
		a<14,2> = -458.497
		a<15,2> = 339.188

		a<1,3> = -22748
		a<2,3> = 4490
		a<3,3> = -12061
		a<4,3> = 39784
		a<5,3> = 39306
		a<6,3> = -46890
		a<7,3> = -37241
		a<8,3> = -8910
		a<9,3> = 48338
		a<10,3> = 2877
		a<11,3> = 25808
		a<12,3> = -4626
		a<13,3> = -1974
		a<14,3> = -47059
		a<15,3> = 45064

        a<1,4> = 1
		a<2,4> = 2
		a<3,4> = 3
		a<4,4> = 4
		a<5,4> = 5
		a<6,4> = 6
		a<7,4> = 7
		a<8,4> = 8
		a<9,4> = 9
		a<10,4> = 10
		a<11,4> = 11
		a<12,4> = 12
		a<13,4> = 13
		a<14,4> = 14
		a<15,4> = 15

		b = rsort(a,3)

        i = 1;
        r = ''
        d = ''
loop    output = b<i,3> '  ' b<i,4> 
        r = r ' ' b<i,4>
        d = d ' ' b<i,1>
        i = i + 1
        le(i,15) :s(loop)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(" 9 15 4 5 11 2 10 13 12 8 3 1 7 6 14", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","r")]).Data);
    }

}