using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// SORT and RSORT keep the order of rows whose keys are equal, as SPITBOL's do (its sortc compares the rows' offsets when the keys
/// are equal, for the reverse sort too), however many rows there are: List.Sort is not stable beyond sixteen elements, and a word
/// count that sorts its table by frequency printed its ties in another order (Budne's tab). The expected text is what sbl -b printed
/// for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SortTies
{
    [TestMethod]
    public void TEST_SortTies_rows_with_equal_keys_keep_their_order_in_sort_and_rsort()
    {
        var s = @"        A = ARRAY('24,2')
        A<1,1> = 'a0'; A<1,2> = 5
        A<2,1> = 'b1'; A<2,2> = 5
        A<3,1> = 'c2'; A<3,2> = 5
        A<4,1> = 'd3'; A<4,2> = 2
        A<5,1> = 'e4'; A<5,2> = 2
        A<6,1> = 'f5'; A<6,2> = 5
        A<7,1> = 'g6'; A<7,2> = 2
        A<8,1> = 'h7'; A<8,2> = 1
        A<9,1> = 'i8'; A<9,2> = 5
        A<10,1> = 'j9'; A<10,2> = 3
        A<11,1> = 'k10'; A<11,2> = 2
        A<12,1> = 'l11'; A<12,2> = 1
        A<13,1> = 'm12'; A<13,2> = 1
        A<14,1> = 'n13'; A<14,2> = 5
        A<15,1> = 'o14'; A<15,2> = 5
        A<16,1> = 'p15'; A<16,2> = 2
        A<17,1> = 'q16'; A<17,2> = 1
        A<18,1> = 'r17'; A<18,2> = 1
        A<19,1> = 's18'; A<19,2> = 1
        A<20,1> = 't19'; A<20,2> = 1
        A<21,1> = 'u20'; A<21,2> = 2
        A<22,1> = 'v21'; A<22,2> = 2
        A<23,1> = 'w22'; A<23,2> = 1
        A<24,1> = 'x23'; A<24,2> = 5
        R = RSORT(A, 2)
        S = SORT(A, 2)
        I = 1
L       OUTPUT = R<I,1> ' ' R<I,2> '   ' S<I,1> ' ' S<I,2>  :F(END)
        I = I + 1  :(L)
END";
        Assert.AreEqual(@"a0 5   h7 1
b1 5   l11 1
c2 5   m12 1
f5 5   q16 1
i8 5   r17 1
n13 5   s18 1
o14 5   t19 1
x23 5   w22 1
j9 3   d3 2
d3 2   e4 2
e4 2   g6 2
g6 2   k10 2
k10 2   p15 2
p15 2   u20 2
u20 2   v21 2
v21 2   j9 3
h7 1   a0 5
l11 1   b1 5
m12 1   c2 5
q16 1   f5 5
r17 1   i8 5
s18 1   n13 5
t19 1   o14 5
w22 1   x23 5", SetupTests.RunWithInput(s));
    }
}
