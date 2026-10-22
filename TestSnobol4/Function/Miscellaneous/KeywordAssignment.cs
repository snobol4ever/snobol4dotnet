using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// What a keyword takes when it is assigned, as sbl takes it: a value that is no integer is 208 (&ERRTEXT takes any string, 211 for none),
/// one that is negative or larger than the biggest object is 210, &MAXLNGTH below 1024 is 287, &FULLSCAN of 0 is 274, a protected keyword
/// (&STCOUNT) is 209 only after its value passes those, and assigning &ERRTYPE signals that error at once (its own text for a number
/// SPITBOL uses, &ERRTEXT's for one it does not). Reading &INPUT reads no line. Every expected line is what sbl -b printed for the same
/// statements, "KEYWORD=value result &ERRTYPE", the result being the keyword's value after or "fail".
/// </summary>
[DoNotParallelize]
[TestClass]
public class KeywordAssignment
{
    [TestMethod]
    public void TEST_KeywordAssignment_each_value_gives_the_result_and_error_sbl_gives()
    {
        var s = @"
        &ERRLIMIT = 1000000
        &TRIM = 0
        &ERRTYPE = 0
        OLD1 = &ANCHOR
        &ANCHOR = 0   :S(S1)F(F1)
S1     R1 = &ANCHOR   :(P1)
F1     R1 = 'fail'
P1     OUTPUT = 'ANCHOR=0 ' R1 ' ' &ERRTYPE
        &ANCHOR = OLD1
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD2 = &ANCHOR
        &ANCHOR = 1   :S(S2)F(F2)
S2     R2 = &ANCHOR   :(P2)
F2     R2 = 'fail'
P2     OUTPUT = 'ANCHOR=1 ' R2 ' ' &ERRTYPE
        &ANCHOR = OLD2
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD3 = &ANCHOR
        &ANCHOR = -1   :S(S3)F(F3)
S3     R3 = &ANCHOR   :(P3)
F3     R3 = 'fail'
P3     OUTPUT = 'ANCHOR=-1 ' R3 ' ' &ERRTYPE
        &ANCHOR = OLD3
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD4 = &ANCHOR
        &ANCHOR = 1024   :S(S4)F(F4)
S4     R4 = &ANCHOR   :(P4)
F4     R4 = 'fail'
P4     OUTPUT = 'ANCHOR=1024 ' R4 ' ' &ERRTYPE
        &ANCHOR = OLD4
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD5 = &ANCHOR
        &ANCHOR = 1023   :S(S5)F(F5)
S5     R5 = &ANCHOR   :(P5)
F5     R5 = 'fail'
P5     OUTPUT = 'ANCHOR=1023 ' R5 ' ' &ERRTYPE
        &ANCHOR = OLD5
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD6 = &ANCHOR
        &ANCHOR = 16777217   :S(S6)F(F6)
S6     R6 = &ANCHOR   :(P6)
F6     R6 = 'fail'
P6     OUTPUT = 'ANCHOR=16777217 ' R6 ' ' &ERRTYPE
        &ANCHOR = OLD6
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD7 = &ANCHOR
        &ANCHOR = 'x'   :S(S7)F(F7)
S7     R7 = &ANCHOR   :(P7)
F7     R7 = 'fail'
P7     OUTPUT = 'ANCHOR=QxQ ' R7 ' ' &ERRTYPE
        &ANCHOR = OLD7
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD8 = &ANCHOR
        &ANCHOR = ''   :S(S8)F(F8)
S8     R8 = &ANCHOR   :(P8)
F8     R8 = 'fail'
P8     OUTPUT = 'ANCHOR=QQ ' R8 ' ' &ERRTYPE
        &ANCHOR = OLD8
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD9 = &ANCHOR
        &ANCHOR = 1.5   :S(S9)F(F9)
S9     R9 = &ANCHOR   :(P9)
F9     R9 = 'fail'
P9     OUTPUT = 'ANCHOR=1.5 ' R9 ' ' &ERRTYPE
        &ANCHOR = OLD9
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD10 = &ERRLIMIT
        &ERRLIMIT = 0   :S(S10)F(F10)
S10     R10 = &ERRLIMIT   :(P10)
F10     R10 = 'fail'
P10     OUTPUT = 'ERRLIMIT=0 ' R10 ' ' &ERRTYPE
        &ERRLIMIT = OLD10
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD11 = &ERRLIMIT
        &ERRLIMIT = 1   :S(S11)F(F11)
S11     R11 = &ERRLIMIT   :(P11)
F11     R11 = 'fail'
P11     OUTPUT = 'ERRLIMIT=1 ' R11 ' ' &ERRTYPE
        &ERRLIMIT = OLD11
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD12 = &ERRLIMIT
        &ERRLIMIT = -1   :S(S12)F(F12)
S12     R12 = &ERRLIMIT   :(P12)
F12     R12 = 'fail'
P12     OUTPUT = 'ERRLIMIT=-1 ' R12 ' ' &ERRTYPE
        &ERRLIMIT = OLD12
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD13 = &ERRLIMIT
        &ERRLIMIT = 1024   :S(S13)F(F13)
S13     R13 = &ERRLIMIT   :(P13)
F13     R13 = 'fail'
P13     OUTPUT = 'ERRLIMIT=1024 ' R13 ' ' &ERRTYPE
        &ERRLIMIT = OLD13
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD14 = &ERRLIMIT
        &ERRLIMIT = 1023   :S(S14)F(F14)
S14     R14 = &ERRLIMIT   :(P14)
F14     R14 = 'fail'
P14     OUTPUT = 'ERRLIMIT=1023 ' R14 ' ' &ERRTYPE
        &ERRLIMIT = OLD14
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD15 = &ERRLIMIT
        &ERRLIMIT = 16777217   :S(S15)F(F15)
S15     R15 = &ERRLIMIT   :(P15)
F15     R15 = 'fail'
P15     OUTPUT = 'ERRLIMIT=16777217 ' R15 ' ' &ERRTYPE
        &ERRLIMIT = OLD15
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD16 = &ERRLIMIT
        &ERRLIMIT = 'x'   :S(S16)F(F16)
S16     R16 = &ERRLIMIT   :(P16)
F16     R16 = 'fail'
P16     OUTPUT = 'ERRLIMIT=QxQ ' R16 ' ' &ERRTYPE
        &ERRLIMIT = OLD16
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD17 = &ERRLIMIT
        &ERRLIMIT = ''   :S(S17)F(F17)
S17     R17 = &ERRLIMIT   :(P17)
F17     R17 = 'fail'
P17     OUTPUT = 'ERRLIMIT=QQ ' R17 ' ' &ERRTYPE
        &ERRLIMIT = OLD17
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD18 = &ERRLIMIT
        &ERRLIMIT = 1.5   :S(S18)F(F18)
S18     R18 = &ERRLIMIT   :(P18)
F18     R18 = 'fail'
P18     OUTPUT = 'ERRLIMIT=1.5 ' R18 ' ' &ERRTYPE
        &ERRLIMIT = OLD18
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD19 = &FULLSCAN
        &FULLSCAN = 0   :S(S19)F(F19)
S19     R19 = &FULLSCAN   :(P19)
F19     R19 = 'fail'
P19     OUTPUT = 'FULLSCAN=0 ' R19 ' ' &ERRTYPE
        &FULLSCAN = OLD19
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD20 = &FULLSCAN
        &FULLSCAN = 1   :S(S20)F(F20)
S20     R20 = &FULLSCAN   :(P20)
F20     R20 = 'fail'
P20     OUTPUT = 'FULLSCAN=1 ' R20 ' ' &ERRTYPE
        &FULLSCAN = OLD20
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD21 = &FULLSCAN
        &FULLSCAN = -1   :S(S21)F(F21)
S21     R21 = &FULLSCAN   :(P21)
F21     R21 = 'fail'
P21     OUTPUT = 'FULLSCAN=-1 ' R21 ' ' &ERRTYPE
        &FULLSCAN = OLD21
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD22 = &FULLSCAN
        &FULLSCAN = 1024   :S(S22)F(F22)
S22     R22 = &FULLSCAN   :(P22)
F22     R22 = 'fail'
P22     OUTPUT = 'FULLSCAN=1024 ' R22 ' ' &ERRTYPE
        &FULLSCAN = OLD22
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD23 = &FULLSCAN
        &FULLSCAN = 1023   :S(S23)F(F23)
S23     R23 = &FULLSCAN   :(P23)
F23     R23 = 'fail'
P23     OUTPUT = 'FULLSCAN=1023 ' R23 ' ' &ERRTYPE
        &FULLSCAN = OLD23
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD24 = &FULLSCAN
        &FULLSCAN = 16777217   :S(S24)F(F24)
S24     R24 = &FULLSCAN   :(P24)
F24     R24 = 'fail'
P24     OUTPUT = 'FULLSCAN=16777217 ' R24 ' ' &ERRTYPE
        &FULLSCAN = OLD24
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD25 = &FULLSCAN
        &FULLSCAN = 'x'   :S(S25)F(F25)
S25     R25 = &FULLSCAN   :(P25)
F25     R25 = 'fail'
P25     OUTPUT = 'FULLSCAN=QxQ ' R25 ' ' &ERRTYPE
        &FULLSCAN = OLD25
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD26 = &FULLSCAN
        &FULLSCAN = ''   :S(S26)F(F26)
S26     R26 = &FULLSCAN   :(P26)
F26     R26 = 'fail'
P26     OUTPUT = 'FULLSCAN=QQ ' R26 ' ' &ERRTYPE
        &FULLSCAN = OLD26
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD27 = &FULLSCAN
        &FULLSCAN = 1.5   :S(S27)F(F27)
S27     R27 = &FULLSCAN   :(P27)
F27     R27 = 'fail'
P27     OUTPUT = 'FULLSCAN=1.5 ' R27 ' ' &ERRTYPE
        &FULLSCAN = OLD27
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD28 = &MAXLNGTH
        &MAXLNGTH = 0   :S(S28)F(F28)
S28     R28 = &MAXLNGTH   :(P28)
F28     R28 = 'fail'
P28     OUTPUT = 'MAXLNGTH=0 ' R28 ' ' &ERRTYPE
        &MAXLNGTH = OLD28
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD29 = &MAXLNGTH
        &MAXLNGTH = 1   :S(S29)F(F29)
S29     R29 = &MAXLNGTH   :(P29)
F29     R29 = 'fail'
P29     OUTPUT = 'MAXLNGTH=1 ' R29 ' ' &ERRTYPE
        &MAXLNGTH = OLD29
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD30 = &MAXLNGTH
        &MAXLNGTH = -1   :S(S30)F(F30)
S30     R30 = &MAXLNGTH   :(P30)
F30     R30 = 'fail'
P30     OUTPUT = 'MAXLNGTH=-1 ' R30 ' ' &ERRTYPE
        &MAXLNGTH = OLD30
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD31 = &MAXLNGTH
        &MAXLNGTH = 1024   :S(S31)F(F31)
S31     R31 = &MAXLNGTH   :(P31)
F31     R31 = 'fail'
P31     OUTPUT = 'MAXLNGTH=1024 ' R31 ' ' &ERRTYPE
        &MAXLNGTH = OLD31
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD32 = &MAXLNGTH
        &MAXLNGTH = 1023   :S(S32)F(F32)
S32     R32 = &MAXLNGTH   :(P32)
F32     R32 = 'fail'
P32     OUTPUT = 'MAXLNGTH=1023 ' R32 ' ' &ERRTYPE
        &MAXLNGTH = OLD32
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD33 = &MAXLNGTH
        &MAXLNGTH = 16777217   :S(S33)F(F33)
S33     R33 = &MAXLNGTH   :(P33)
F33     R33 = 'fail'
P33     OUTPUT = 'MAXLNGTH=16777217 ' R33 ' ' &ERRTYPE
        &MAXLNGTH = OLD33
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD34 = &MAXLNGTH
        &MAXLNGTH = 'x'   :S(S34)F(F34)
S34     R34 = &MAXLNGTH   :(P34)
F34     R34 = 'fail'
P34     OUTPUT = 'MAXLNGTH=QxQ ' R34 ' ' &ERRTYPE
        &MAXLNGTH = OLD34
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD35 = &MAXLNGTH
        &MAXLNGTH = ''   :S(S35)F(F35)
S35     R35 = &MAXLNGTH   :(P35)
F35     R35 = 'fail'
P35     OUTPUT = 'MAXLNGTH=QQ ' R35 ' ' &ERRTYPE
        &MAXLNGTH = OLD35
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD36 = &MAXLNGTH
        &MAXLNGTH = 1.5   :S(S36)F(F36)
S36     R36 = &MAXLNGTH   :(P36)
F36     R36 = 'fail'
P36     OUTPUT = 'MAXLNGTH=1.5 ' R36 ' ' &ERRTYPE
        &MAXLNGTH = OLD36
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD37 = &TRIM
        &TRIM = 0   :S(S37)F(F37)
S37     R37 = &TRIM   :(P37)
F37     R37 = 'fail'
P37     OUTPUT = 'TRIM=0 ' R37 ' ' &ERRTYPE
        &TRIM = OLD37
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD38 = &TRIM
        &TRIM = 1   :S(S38)F(F38)
S38     R38 = &TRIM   :(P38)
F38     R38 = 'fail'
P38     OUTPUT = 'TRIM=1 ' R38 ' ' &ERRTYPE
        &TRIM = OLD38
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD39 = &TRIM
        &TRIM = -1   :S(S39)F(F39)
S39     R39 = &TRIM   :(P39)
F39     R39 = 'fail'
P39     OUTPUT = 'TRIM=-1 ' R39 ' ' &ERRTYPE
        &TRIM = OLD39
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD40 = &TRIM
        &TRIM = 1024   :S(S40)F(F40)
S40     R40 = &TRIM   :(P40)
F40     R40 = 'fail'
P40     OUTPUT = 'TRIM=1024 ' R40 ' ' &ERRTYPE
        &TRIM = OLD40
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD41 = &TRIM
        &TRIM = 1023   :S(S41)F(F41)
S41     R41 = &TRIM   :(P41)
F41     R41 = 'fail'
P41     OUTPUT = 'TRIM=1023 ' R41 ' ' &ERRTYPE
        &TRIM = OLD41
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD42 = &TRIM
        &TRIM = 16777217   :S(S42)F(F42)
S42     R42 = &TRIM   :(P42)
F42     R42 = 'fail'
P42     OUTPUT = 'TRIM=16777217 ' R42 ' ' &ERRTYPE
        &TRIM = OLD42
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD43 = &TRIM
        &TRIM = 'x'   :S(S43)F(F43)
S43     R43 = &TRIM   :(P43)
F43     R43 = 'fail'
P43     OUTPUT = 'TRIM=QxQ ' R43 ' ' &ERRTYPE
        &TRIM = OLD43
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD44 = &TRIM
        &TRIM = ''   :S(S44)F(F44)
S44     R44 = &TRIM   :(P44)
F44     R44 = 'fail'
P44     OUTPUT = 'TRIM=QQ ' R44 ' ' &ERRTYPE
        &TRIM = OLD44
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD45 = &TRIM
        &TRIM = 1.5   :S(S45)F(F45)
S45     R45 = &TRIM   :(P45)
F45     R45 = 'fail'
P45     OUTPUT = 'TRIM=1.5 ' R45 ' ' &ERRTYPE
        &TRIM = OLD45
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD46 = &INPUT
        &INPUT = 0   :S(S46)F(F46)
S46     R46 = &INPUT   :(P46)
F46     R46 = 'fail'
P46     OUTPUT = 'INPUT=0 ' R46 ' ' &ERRTYPE
        &INPUT = OLD46
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD47 = &INPUT
        &INPUT = 1   :S(S47)F(F47)
S47     R47 = &INPUT   :(P47)
F47     R47 = 'fail'
P47     OUTPUT = 'INPUT=1 ' R47 ' ' &ERRTYPE
        &INPUT = OLD47
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD48 = &INPUT
        &INPUT = -1   :S(S48)F(F48)
S48     R48 = &INPUT   :(P48)
F48     R48 = 'fail'
P48     OUTPUT = 'INPUT=-1 ' R48 ' ' &ERRTYPE
        &INPUT = OLD48
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD49 = &INPUT
        &INPUT = 1024   :S(S49)F(F49)
S49     R49 = &INPUT   :(P49)
F49     R49 = 'fail'
P49     OUTPUT = 'INPUT=1024 ' R49 ' ' &ERRTYPE
        &INPUT = OLD49
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD50 = &INPUT
        &INPUT = 1023   :S(S50)F(F50)
S50     R50 = &INPUT   :(P50)
F50     R50 = 'fail'
P50     OUTPUT = 'INPUT=1023 ' R50 ' ' &ERRTYPE
        &INPUT = OLD50
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD51 = &INPUT
        &INPUT = 16777217   :S(S51)F(F51)
S51     R51 = &INPUT   :(P51)
F51     R51 = 'fail'
P51     OUTPUT = 'INPUT=16777217 ' R51 ' ' &ERRTYPE
        &INPUT = OLD51
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD52 = &INPUT
        &INPUT = 'x'   :S(S52)F(F52)
S52     R52 = &INPUT   :(P52)
F52     R52 = 'fail'
P52     OUTPUT = 'INPUT=QxQ ' R52 ' ' &ERRTYPE
        &INPUT = OLD52
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD53 = &INPUT
        &INPUT = ''   :S(S53)F(F53)
S53     R53 = &INPUT   :(P53)
F53     R53 = 'fail'
P53     OUTPUT = 'INPUT=QQ ' R53 ' ' &ERRTYPE
        &INPUT = OLD53
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD54 = &INPUT
        &INPUT = 1.5   :S(S54)F(F54)
S54     R54 = &INPUT   :(P54)
F54     R54 = 'fail'
P54     OUTPUT = 'INPUT=1.5 ' R54 ' ' &ERRTYPE
        &INPUT = OLD54
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD55 = &STCOUNT
        &STCOUNT = 0   :S(S55)F(F55)
S55     R55 = &STCOUNT   :(P55)
F55     R55 = 'fail'
P55     OUTPUT = 'STCOUNT=0 ' R55 ' ' &ERRTYPE
        &STCOUNT = OLD55
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD56 = &STCOUNT
        &STCOUNT = 1   :S(S56)F(F56)
S56     R56 = &STCOUNT   :(P56)
F56     R56 = 'fail'
P56     OUTPUT = 'STCOUNT=1 ' R56 ' ' &ERRTYPE
        &STCOUNT = OLD56
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD57 = &STCOUNT
        &STCOUNT = -1   :S(S57)F(F57)
S57     R57 = &STCOUNT   :(P57)
F57     R57 = 'fail'
P57     OUTPUT = 'STCOUNT=-1 ' R57 ' ' &ERRTYPE
        &STCOUNT = OLD57
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD58 = &STCOUNT
        &STCOUNT = 1024   :S(S58)F(F58)
S58     R58 = &STCOUNT   :(P58)
F58     R58 = 'fail'
P58     OUTPUT = 'STCOUNT=1024 ' R58 ' ' &ERRTYPE
        &STCOUNT = OLD58
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD59 = &STCOUNT
        &STCOUNT = 1023   :S(S59)F(F59)
S59     R59 = &STCOUNT   :(P59)
F59     R59 = 'fail'
P59     OUTPUT = 'STCOUNT=1023 ' R59 ' ' &ERRTYPE
        &STCOUNT = OLD59
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD60 = &STCOUNT
        &STCOUNT = 16777217   :S(S60)F(F60)
S60     R60 = &STCOUNT   :(P60)
F60     R60 = 'fail'
P60     OUTPUT = 'STCOUNT=16777217 ' R60 ' ' &ERRTYPE
        &STCOUNT = OLD60
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD61 = &STCOUNT
        &STCOUNT = 'x'   :S(S61)F(F61)
S61     R61 = &STCOUNT   :(P61)
F61     R61 = 'fail'
P61     OUTPUT = 'STCOUNT=QxQ ' R61 ' ' &ERRTYPE
        &STCOUNT = OLD61
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD62 = &STCOUNT
        &STCOUNT = ''   :S(S62)F(F62)
S62     R62 = &STCOUNT   :(P62)
F62     R62 = 'fail'
P62     OUTPUT = 'STCOUNT=QQ ' R62 ' ' &ERRTYPE
        &STCOUNT = OLD62
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD63 = &STCOUNT
        &STCOUNT = 1.5   :S(S63)F(F63)
S63     R63 = &STCOUNT   :(P63)
F63     R63 = 'fail'
P63     OUTPUT = 'STCOUNT=1.5 ' R63 ' ' &ERRTYPE
        &STCOUNT = OLD63
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD64 = &ERRTEXT
        &ERRTEXT = 0   :S(S64)F(F64)
S64     R64 = &ERRTEXT   :(P64)
F64     R64 = 'fail'
P64     OUTPUT = 'ERRTEXT=0 ' R64 ' ' &ERRTYPE
        &ERRTEXT = OLD64
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD65 = &ERRTEXT
        &ERRTEXT = 1   :S(S65)F(F65)
S65     R65 = &ERRTEXT   :(P65)
F65     R65 = 'fail'
P65     OUTPUT = 'ERRTEXT=1 ' R65 ' ' &ERRTYPE
        &ERRTEXT = OLD65
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD66 = &ERRTEXT
        &ERRTEXT = -1   :S(S66)F(F66)
S66     R66 = &ERRTEXT   :(P66)
F66     R66 = 'fail'
P66     OUTPUT = 'ERRTEXT=-1 ' R66 ' ' &ERRTYPE
        &ERRTEXT = OLD66
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD67 = &ERRTEXT
        &ERRTEXT = 1024   :S(S67)F(F67)
S67     R67 = &ERRTEXT   :(P67)
F67     R67 = 'fail'
P67     OUTPUT = 'ERRTEXT=1024 ' R67 ' ' &ERRTYPE
        &ERRTEXT = OLD67
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD68 = &ERRTEXT
        &ERRTEXT = 1023   :S(S68)F(F68)
S68     R68 = &ERRTEXT   :(P68)
F68     R68 = 'fail'
P68     OUTPUT = 'ERRTEXT=1023 ' R68 ' ' &ERRTYPE
        &ERRTEXT = OLD68
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD69 = &ERRTEXT
        &ERRTEXT = 16777217   :S(S69)F(F69)
S69     R69 = &ERRTEXT   :(P69)
F69     R69 = 'fail'
P69     OUTPUT = 'ERRTEXT=16777217 ' R69 ' ' &ERRTYPE
        &ERRTEXT = OLD69
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD70 = &ERRTEXT
        &ERRTEXT = 'x'   :S(S70)F(F70)
S70     R70 = &ERRTEXT   :(P70)
F70     R70 = 'fail'
P70     OUTPUT = 'ERRTEXT=QxQ ' R70 ' ' &ERRTYPE
        &ERRTEXT = OLD70
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD71 = &ERRTEXT
        &ERRTEXT = ''   :S(S71)F(F71)
S71     R71 = &ERRTEXT   :(P71)
F71     R71 = 'fail'
P71     OUTPUT = 'ERRTEXT=QQ ' R71 ' ' &ERRTYPE
        &ERRTEXT = OLD71
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD72 = &ERRTEXT
        &ERRTEXT = 1.5   :S(S72)F(F72)
S72     R72 = &ERRTEXT   :(P72)
F72     R72 = 'fail'
P72     OUTPUT = 'ERRTEXT=1.5 ' R72 ' ' &ERRTYPE
        &ERRTEXT = OLD72
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD73 = &ERRTYPE
        &ERRTYPE = 0   :S(S73)F(F73)
S73     R73 = &ERRTYPE   :(P73)
F73     R73 = 'fail'
P73     OUTPUT = 'ERRTYPE=0 ' R73 ' ' &ERRTYPE
        &ERRTYPE = OLD73
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD74 = &ERRTYPE
        &ERRTYPE = 1   :S(S74)F(F74)
S74     R74 = &ERRTYPE   :(P74)
F74     R74 = 'fail'
P74     OUTPUT = 'ERRTYPE=1 ' R74 ' ' &ERRTYPE
        &ERRTYPE = OLD74
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD75 = &ERRTYPE
        &ERRTYPE = -1   :S(S75)F(F75)
S75     R75 = &ERRTYPE   :(P75)
F75     R75 = 'fail'
P75     OUTPUT = 'ERRTYPE=-1 ' R75 ' ' &ERRTYPE
        &ERRTYPE = OLD75
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD76 = &ERRTYPE
        &ERRTYPE = 1024   :S(S76)F(F76)
S76     R76 = &ERRTYPE   :(P76)
F76     R76 = 'fail'
P76     OUTPUT = 'ERRTYPE=1024 ' R76 ' ' &ERRTYPE
        &ERRTYPE = OLD76
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD77 = &ERRTYPE
        &ERRTYPE = 1023   :S(S77)F(F77)
S77     R77 = &ERRTYPE   :(P77)
F77     R77 = 'fail'
P77     OUTPUT = 'ERRTYPE=1023 ' R77 ' ' &ERRTYPE
        &ERRTYPE = OLD77
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD78 = &ERRTYPE
        &ERRTYPE = 16777217   :S(S78)F(F78)
S78     R78 = &ERRTYPE   :(P78)
F78     R78 = 'fail'
P78     OUTPUT = 'ERRTYPE=16777217 ' R78 ' ' &ERRTYPE
        &ERRTYPE = OLD78
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD79 = &ERRTYPE
        &ERRTYPE = 'x'   :S(S79)F(F79)
S79     R79 = &ERRTYPE   :(P79)
F79     R79 = 'fail'
P79     OUTPUT = 'ERRTYPE=QxQ ' R79 ' ' &ERRTYPE
        &ERRTYPE = OLD79
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD80 = &ERRTYPE
        &ERRTYPE = ''   :S(S80)F(F80)
S80     R80 = &ERRTYPE   :(P80)
F80     R80 = 'fail'
P80     OUTPUT = 'ERRTYPE=QQ ' R80 ' ' &ERRTYPE
        &ERRTYPE = OLD80
        &ERRLIMIT = 1000000
        &ERRTYPE = 0
        OLD81 = &ERRTYPE
        &ERRTYPE = 1.5   :S(S81)F(F81)
S81     R81 = &ERRTYPE   :(P81)
F81     R81 = 'fail'
P81     OUTPUT = 'ERRTYPE=1.5 ' R81 ' ' &ERRTYPE
        &ERRTYPE = OLD81
        &ERRLIMIT = 1000000
END";
        Assert.AreEqual(@"ANCHOR=0 0 0
ANCHOR=1 1 0
ANCHOR=-1 fail 210
ANCHOR=1024 1024 0
ANCHOR=1023 1023 0
ANCHOR=16777217 fail 210
ANCHOR=QxQ fail 208
ANCHOR=QQ 0 0
ANCHOR=1.5 1 0
ERRLIMIT=0 0 0
ERRLIMIT=1 1 0
ERRLIMIT=-1 fail 210
ERRLIMIT=1024 1024 0
ERRLIMIT=1023 1023 0
ERRLIMIT=16777217 fail 210
ERRLIMIT=QxQ fail 208
ERRLIMIT=QQ 0 0
ERRLIMIT=1.5 1 0
FULLSCAN=0 fail 274
FULLSCAN=1 1 0
FULLSCAN=-1 fail 210
FULLSCAN=1024 1024 0
FULLSCAN=1023 1023 0
FULLSCAN=16777217 fail 210
FULLSCAN=QxQ fail 208
FULLSCAN=QQ fail 274
FULLSCAN=1.5 1 0
MAXLNGTH=0 fail 287
MAXLNGTH=1 fail 287
MAXLNGTH=-1 fail 210
MAXLNGTH=1024 1024 0
MAXLNGTH=1023 fail 287
MAXLNGTH=16777217 fail 210
MAXLNGTH=QxQ fail 208
MAXLNGTH=QQ fail 287
MAXLNGTH=1.5 fail 287
TRIM=0 0 0
TRIM=1 1 0
TRIM=-1 fail 210
TRIM=1024 1024 0
TRIM=1023 1023 0
TRIM=16777217 fail 210
TRIM=QxQ fail 208
TRIM=QQ 0 0
TRIM=1.5 1 0
INPUT=0 0 0
INPUT=1 1 0
INPUT=-1 fail 210
INPUT=1024 1024 0
INPUT=1023 1023 0
INPUT=16777217 fail 210
INPUT=QxQ fail 208
INPUT=QQ 0 0
INPUT=1.5 1 0
STCOUNT=0 fail 209
STCOUNT=1 fail 209
STCOUNT=-1 fail 210
STCOUNT=1024 fail 209
STCOUNT=1023 fail 209
STCOUNT=16777217 fail 210
STCOUNT=QxQ fail 208
STCOUNT=QQ fail 209
STCOUNT=1.5 fail 209
ERRTEXT=0 0 0
ERRTEXT=1 1 0
ERRTEXT=-1 -1 0
ERRTEXT=1024 1024 0
ERRTEXT=1023 1023 0
ERRTEXT=16777217 16777217 0
ERRTEXT=QxQ x 0
ERRTEXT=QQ  0
ERRTEXT=1.5 1.5 0
ERRTYPE=0 fail 0
ERRTYPE=1 fail 1
ERRTYPE=-1 fail 210
ERRTYPE=1024 fail 210
ERRTYPE=1023 fail 210
ERRTYPE=16777217 fail 210
ERRTYPE=QxQ fail 208
ERRTYPE=QQ fail 0
ERRTYPE=1.5 fail 1", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_KeywordAssignment_a_program_signals_its_own_error_with_errtype_and_errtext()
    {
        Assert.AreEqual("after 500 my own text\nafter5 5 alternation right operand is not pattern", SetupTests.RunWithInput(
            "        &ERRLIMIT = 5\n        &ERRTEXT = 'my own text'\n        &ERRTYPE = 500\n        OUTPUT = 'after ' &ERRTYPE ' ' &ERRTEXT\n" +
            "        &ERRTYPE = 5\n        OUTPUT = 'after5 ' &ERRTYPE ' ' &ERRTEXT\nEND"));
    }
}
