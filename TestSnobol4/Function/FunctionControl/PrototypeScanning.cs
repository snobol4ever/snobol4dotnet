using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// DEFINE and DATA scan their prototype as SPITBOL does: the name up to the left paren (83 and 77 when there is none; 84 and 78 when it is
/// null), the arguments or fields up to the right paren, each ended by a comma or the paren (85 and 79 at the end of the string; a null
/// field is 80, a null argument is skipped), and DEFINE's locals ended by commas. A prototype of blanks has no left paren, a null one is
/// 82 and 76. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class PrototypeScanning
{
    [TestMethod]
    public void TEST_PrototypeScanning_each_prototype_is_taken_or_refused_as_sbl_does()
    {
        var s = @"
        &ERRLIMIT = 1000
        &TRIM = 0
        &ERRTYPE = 0
        DEFINE('F1(A,B)L1,L2')   :S(DS0)F(DF0)
DF0     OUTPUT = 'define 0 fail ' &ERRTYPE   :(DD0)
DS0     OUTPUT = 'define 0 ok'   :(DD0)
DD0
        &ERRTYPE = 0
        DATA('F1(A,B)L1,L2')   :S(TS0)F(TF0)
TF0     OUTPUT = 'data 0 fail ' &ERRTYPE   :(TD0)
TS0     OUTPUT = 'data 0 ok'   :(TD0)
TD0
        &ERRTYPE = 0
        DEFINE('F2(A)')   :S(DS1)F(DF1)
DF1     OUTPUT = 'define 1 fail ' &ERRTYPE   :(DD1)
DS1     OUTPUT = 'define 1 ok'   :(DD1)
DD1
        &ERRTYPE = 0
        DATA('F2(A)')   :S(TS1)F(TF1)
TF1     OUTPUT = 'data 1 fail ' &ERRTYPE   :(TD1)
TS1     OUTPUT = 'data 1 ok'   :(TD1)
TD1
        &ERRTYPE = 0
        DEFINE('F3()')   :S(DS2)F(DF2)
DF2     OUTPUT = 'define 2 fail ' &ERRTYPE   :(DD2)
DS2     OUTPUT = 'define 2 ok'   :(DD2)
DD2
        &ERRTYPE = 0
        DATA('F3()')   :S(TS2)F(TF2)
TF2     OUTPUT = 'data 2 fail ' &ERRTYPE   :(TD2)
TS2     OUTPUT = 'data 2 ok'   :(TD2)
TD2
        &ERRTYPE = 0
        DEFINE('F4()L1')   :S(DS3)F(DF3)
DF3     OUTPUT = 'define 3 fail ' &ERRTYPE   :(DD3)
DS3     OUTPUT = 'define 3 ok'   :(DD3)
DD3
        &ERRTYPE = 0
        DATA('F4()L1')   :S(TS3)F(TF3)
TF3     OUTPUT = 'data 3 fail ' &ERRTYPE   :(TD3)
TS3     OUTPUT = 'data 3 ok'   :(TD3)
TD3
        &ERRTYPE = 0
        DEFINE('F5( A , B ) L1 , L2 ')   :S(DS4)F(DF4)
DF4     OUTPUT = 'define 4 fail ' &ERRTYPE   :(DD4)
DS4     OUTPUT = 'define 4 ok'   :(DD4)
DD4
        &ERRTYPE = 0
        DATA('F5( A , B ) L1 , L2 ')   :S(TS4)F(TF4)
TF4     OUTPUT = 'data 4 fail ' &ERRTYPE   :(TD4)
TS4     OUTPUT = 'data 4 ok'   :(TD4)
TD4
        &ERRTYPE = 0
        DEFINE('F6(,A)')   :S(DS5)F(DF5)
DF5     OUTPUT = 'define 5 fail ' &ERRTYPE   :(DD5)
DS5     OUTPUT = 'define 5 ok'   :(DD5)
DD5
        &ERRTYPE = 0
        DATA('F6(,A)')   :S(TS5)F(TF5)
TF5     OUTPUT = 'data 5 fail ' &ERRTYPE   :(TD5)
TS5     OUTPUT = 'data 5 ok'   :(TD5)
TD5
        &ERRTYPE = 0
        DEFINE('F7(A,)L1')   :S(DS6)F(DF6)
DF6     OUTPUT = 'define 6 fail ' &ERRTYPE   :(DD6)
DS6     OUTPUT = 'define 6 ok'   :(DD6)
DD6
        &ERRTYPE = 0
        DATA('F7(A,)L1')   :S(TS6)F(TF6)
TF6     OUTPUT = 'data 6 fail ' &ERRTYPE   :(TD6)
TS6     OUTPUT = 'data 6 ok'   :(TD6)
TD6
        &ERRTYPE = 0
        DEFINE('F8(A,,B)')   :S(DS7)F(DF7)
DF7     OUTPUT = 'define 7 fail ' &ERRTYPE   :(DD7)
DS7     OUTPUT = 'define 7 ok'   :(DD7)
DD7
        &ERRTYPE = 0
        DATA('F8(A,,B)')   :S(TS7)F(TF7)
TF7     OUTPUT = 'data 7 fail ' &ERRTYPE   :(TD7)
TS7     OUTPUT = 'data 7 ok'   :(TD7)
TD7
        &ERRTYPE = 0
        DEFINE('F9(A,B')   :S(DS8)F(DF8)
DF8     OUTPUT = 'define 8 fail ' &ERRTYPE   :(DD8)
DS8     OUTPUT = 'define 8 ok'   :(DD8)
DD8
        &ERRTYPE = 0
        DATA('F9(A,B')   :S(TS8)F(TF8)
TF8     OUTPUT = 'data 8 fail ' &ERRTYPE   :(TD8)
TS8     OUTPUT = 'data 8 ok'   :(TD8)
TD8
        &ERRTYPE = 0
        DEFINE('F10 A')   :S(DS9)F(DF9)
DF9     OUTPUT = 'define 9 fail ' &ERRTYPE   :(DD9)
DS9     OUTPUT = 'define 9 ok'   :(DD9)
DD9
        &ERRTYPE = 0
        DATA('F10 A')   :S(TS9)F(TF9)
TF9     OUTPUT = 'data 9 fail ' &ERRTYPE   :(TD9)
TS9     OUTPUT = 'data 9 ok'   :(TD9)
TD9
        &ERRTYPE = 0
        DEFINE('F11')   :S(DS10)F(DF10)
DF10     OUTPUT = 'define 10 fail ' &ERRTYPE   :(DD10)
DS10     OUTPUT = 'define 10 ok'   :(DD10)
DD10
        &ERRTYPE = 0
        DATA('F11')   :S(TS10)F(TF10)
TF10     OUTPUT = 'data 10 fail ' &ERRTYPE   :(TD10)
TS10     OUTPUT = 'data 10 ok'   :(TD10)
TD10
        &ERRTYPE = 0
        DEFINE('F12(')   :S(DS11)F(DF11)
DF11     OUTPUT = 'define 11 fail ' &ERRTYPE   :(DD11)
DS11     OUTPUT = 'define 11 ok'   :(DD11)
DD11
        &ERRTYPE = 0
        DATA('F12(')   :S(TS11)F(TF11)
TF11     OUTPUT = 'data 11 fail ' &ERRTYPE   :(TD11)
TS11     OUTPUT = 'data 11 ok'   :(TD11)
TD11
        &ERRTYPE = 0
        DEFINE('(A)')   :S(DS12)F(DF12)
DF12     OUTPUT = 'define 12 fail ' &ERRTYPE   :(DD12)
DS12     OUTPUT = 'define 12 ok'   :(DD12)
DD12
        &ERRTYPE = 0
        DATA('(A)')   :S(TS12)F(TF12)
TF12     OUTPUT = 'data 12 fail ' &ERRTYPE   :(TD12)
TS12     OUTPUT = 'data 12 ok'   :(TD12)
TD12
        &ERRTYPE = 0
        DEFINE('  ')   :S(DS13)F(DF13)
DF13     OUTPUT = 'define 13 fail ' &ERRTYPE   :(DD13)
DS13     OUTPUT = 'define 13 ok'   :(DD13)
DD13
        &ERRTYPE = 0
        DATA('  ')   :S(TS13)F(TF13)
TF13     OUTPUT = 'data 13 fail ' &ERRTYPE   :(TD13)
TS13     OUTPUT = 'data 13 ok'   :(TD13)
TD13
        &ERRTYPE = 0
        DEFINE('F13(A,B,C)')   :S(DS14)F(DF14)
DF14     OUTPUT = 'define 14 fail ' &ERRTYPE   :(DD14)
DS14     OUTPUT = 'define 14 ok'   :(DD14)
DD14
        &ERRTYPE = 0
        DATA('F13(A,B,C)')   :S(TS14)F(TF14)
TF14     OUTPUT = 'data 14 fail ' &ERRTYPE   :(TD14)
TS14     OUTPUT = 'data 14 ok'   :(TD14)
TD14
        &ERRTYPE = 0
        DEFINE('F14(A)L1,,L2')   :S(DS15)F(DF15)
DF15     OUTPUT = 'define 15 fail ' &ERRTYPE   :(DD15)
DS15     OUTPUT = 'define 15 ok'   :(DD15)
DD15
        &ERRTYPE = 0
        DATA('F14(A)L1,,L2')   :S(TS15)F(TF15)
TF15     OUTPUT = 'data 15 fail ' &ERRTYPE   :(TD15)
TS15     OUTPUT = 'data 15 ok'   :(TD15)
TD15
        &ERRTYPE = 0
        DEFINE('F15(A,B)L1,L2,')   :S(DS16)F(DF16)
DF16     OUTPUT = 'define 16 fail ' &ERRTYPE   :(DD16)
DS16     OUTPUT = 'define 16 ok'   :(DD16)
DD16
        &ERRTYPE = 0
        DATA('F15(A,B)L1,L2,')   :S(TS16)F(TF16)
TF16     OUTPUT = 'data 16 fail ' &ERRTYPE   :(TD16)
TS16     OUTPUT = 'data 16 ok'   :(TD16)
TD16
        &ERRTYPE = 0
        DEFINE('f16(a,b)l1')   :S(DS17)F(DF17)
DF17     OUTPUT = 'define 17 fail ' &ERRTYPE   :(DD17)
DS17     OUTPUT = 'define 17 ok'   :(DD17)
DD17
        &ERRTYPE = 0
        DATA('f16(a,b)l1')   :S(TS17)F(TF17)
TF17     OUTPUT = 'data 17 fail ' &ERRTYPE   :(TD17)
TS17     OUTPUT = 'data 17 ok'   :(TD17)
TD17
        &ERRTYPE = 0
        DEFINE('F17(A B)')   :S(DS18)F(DF18)
DF18     OUTPUT = 'define 18 fail ' &ERRTYPE   :(DD18)
DS18     OUTPUT = 'define 18 ok'   :(DD18)
DD18
        &ERRTYPE = 0
        DATA('F17(A B)')   :S(TS18)F(TF18)
TF18     OUTPUT = 'data 18 fail ' &ERRTYPE   :(TD18)
TS18     OUTPUT = 'data 18 ok'   :(TD18)
TD18
        &ERRTYPE = 0
        DEFINE('F18(A;B)')   :S(DS19)F(DF19)
DF19     OUTPUT = 'define 19 fail ' &ERRTYPE   :(DD19)
DS19     OUTPUT = 'define 19 ok'   :(DD19)
DD19
        &ERRTYPE = 0
        DATA('F18(A;B)')   :S(TS19)F(TF19)
TF19     OUTPUT = 'data 19 fail ' &ERRTYPE   :(TD19)
TS19     OUTPUT = 'data 19 ok'   :(TD19)
TD19
        &ERRTYPE = 0
        DEFINE('F19(A)B')   :S(DS20)F(DF20)
DF20     OUTPUT = 'define 20 fail ' &ERRTYPE   :(DD20)
DS20     OUTPUT = 'define 20 ok'   :(DD20)
DD20
        &ERRTYPE = 0
        DATA('F19(A)B')   :S(TS20)F(TF20)
TF20     OUTPUT = 'data 20 fail ' &ERRTYPE   :(TD20)
TS20     OUTPUT = 'data 20 ok'   :(TD20)
TD20
        &ERRTYPE = 0
        DEFINE('F20(A,B)C(D)')   :S(DS21)F(DF21)
DF21     OUTPUT = 'define 21 fail ' &ERRTYPE   :(DD21)
DS21     OUTPUT = 'define 21 ok'   :(DD21)
DD21
        &ERRTYPE = 0
        DATA('F20(A,B)C(D)')   :S(TS21)F(TF21)
TF21     OUTPUT = 'data 21 fail ' &ERRTYPE   :(TD21)
TS21     OUTPUT = 'data 21 ok'   :(TD21)
TD21
        &ERRTYPE = 0
        DEFINE('SIZE(A)')   :S(DS22)F(DF22)
DF22     OUTPUT = 'define 22 fail ' &ERRTYPE   :(DD22)
DS22     OUTPUT = 'define 22 ok'   :(DD22)
DD22
        &ERRTYPE = 0
        DATA('SIZE(A)')   :S(TS22)F(TF22)
TF22     OUTPUT = 'data 22 fail ' &ERRTYPE   :(TD22)
TS22     OUTPUT = 'data 22 ok'   :(TD22)
TD22
        &ERRTYPE = 0
        DEFINE('F21(A) ( B )')   :S(DS23)F(DF23)
DF23     OUTPUT = 'define 23 fail ' &ERRTYPE   :(DD23)
DS23     OUTPUT = 'define 23 ok'   :(DD23)
DD23
        &ERRTYPE = 0
        DATA('F21(A) ( B )')   :S(TS23)F(TF23)
TF23     OUTPUT = 'data 23 fail ' &ERRTYPE   :(TD23)
TS23     OUTPUT = 'data 23 ok'   :(TD23)
TD23
        &ERRTYPE = 0
        DEFINE('F22(1,2)3,4')   :S(DS24)F(DF24)
DF24     OUTPUT = 'define 24 fail ' &ERRTYPE   :(DD24)
DS24     OUTPUT = 'define 24 ok'   :(DD24)
DD24
        &ERRTYPE = 0
        DATA('F22(1,2)3,4')   :S(TS24)F(TF24)
TF24     OUTPUT = 'data 24 fail ' &ERRTYPE   :(TD24)
TS24     OUTPUT = 'data 24 ok'   :(TD24)
TD24
        &ERRTYPE = 0
        DEFINE('F23(+,-)')   :S(DS25)F(DF25)
DF25     OUTPUT = 'define 25 fail ' &ERRTYPE   :(DD25)
DS25     OUTPUT = 'define 25 ok'   :(DD25)
DD25
        &ERRTYPE = 0
        DATA('F23(+,-)')   :S(TS25)F(TF25)
TF25     OUTPUT = 'data 25 fail ' &ERRTYPE   :(TD25)
TS25     OUTPUT = 'data 25 ok'   :(TD25)
TD25
        &ERRTYPE = 0
        DEFINE('F24(A)  ')   :S(DS26)F(DF26)
DF26     OUTPUT = 'define 26 fail ' &ERRTYPE   :(DD26)
DS26     OUTPUT = 'define 26 ok'   :(DD26)
DD26
        &ERRTYPE = 0
        DATA('F24(A)  ')   :S(TS26)F(TF26)
TF26     OUTPUT = 'data 26 fail ' &ERRTYPE   :(TD26)
TS26     OUTPUT = 'data 26 ok'   :(TD26)
TD26
        OUTPUT = 'args ' ARG('F1',1) ARG('F1',2) LOCAL('F1',1) LOCAL('F1',2)
        OUTPUT = 'args5 ' ARG('F5',1) ARG('F5',2) LOCAL('F5',1) LOCAL('F5',2)
        OUTPUT = 'args6 ' ARG('F6',1) ARG('F6',2)
        OUTPUT = 'args7 ' ARG('F7',1) LOCAL('F7',1)
        OUTPUT = 'args14 ' LOCAL('F14',1) LOCAL('F14',2)
        OUTPUT = 'field ' FIELD('F1',1) FIELD('F1',2) FIELD('F5',2)
END";
        Assert.AreEqual(@"define 0 ok
data 0 ok
define 1 ok
data 1 ok
define 2 ok
data 2 fail 80
define 3 ok
data 3 fail 80
define 4 ok
data 4 ok
define 5 ok
data 5 fail 80
define 6 fail 85
data 6 fail 80
define 7 ok
data 7 fail 80
define 8 fail 85
data 8 fail 79
define 9 fail 83
data 9 fail 77
define 10 fail 83
data 10 fail 77
define 11 fail 85
data 11 fail 79
define 12 fail 84
data 12 fail 78
define 13 fail 83
data 13 fail 77
define 14 ok
data 14 ok
define 15 ok
data 15 ok
define 16 ok
data 16 ok
define 17 ok
data 17 ok
define 18 ok
data 18 ok
define 19 ok
data 19 ok
define 20 ok
data 20 ok
define 21 ok
data 21 ok
define 22 fail 248
data 22 fail 248
define 23 ok
data 23 ok
define 24 ok
data 24 ok
define 25 ok
data 25 ok
define 26 ok
data 26 ok
field ABB", SetupTests.RunWithInput(s));
    }
}
