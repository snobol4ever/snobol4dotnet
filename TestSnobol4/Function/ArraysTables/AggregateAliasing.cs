using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// An array, table or DATA object is one object however many variables hold it, and a variable assigned later is only that variable:
/// after L = V, V = TABLE() leaves L the array. Every expected line is what sbl -b printed for the same program. They were once
/// swapped: the object carried the name of the last variable it was assigned to, and V = TABLE() assigned L.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AggregateAliasing
{
    [TestMethod]
    public void TEST_AggregateAliasing_assigning_a_variable_that_holds_an_aggregate_leaves_the_other_holders()
    {
        var s = @"
        V = ARRAY(3)
        L = V
        V = TABLE()
        OUTPUT = '1: V=' DATATYPE(V) ' L=' DATATYPE(L)
        A = ARRAY(3)
        B = COPY(A)
        A = TABLE()
        OUTPUT = '2: A=' DATATYPE(A) ' B=' DATATYPE(B)
        C = ARRAY(3)
        D = CONVERT(C, 'ARRAY')
        C = TABLE()
        OUTPUT = '3: C=' DATATYPE(C) ' D=' DATATYPE(D)
        E = ARRAY(3)
        F = IDENT(E, E) E
        E = TABLE()
        OUTPUT = '4: E=' DATATYPE(E) ' F=' DATATYPE(F)
        G = TABLE()
        H = G
        G = 5
        OUTPUT = '5: G=' DATATYPE(G) ' H=' DATATYPE(H)
END";
        Assert.AreEqual(@"1: V=TABLE L=ARRAY
2: A=TABLE B=ARRAY
3: C=TABLE D=ARRAY
4: E=TABLE F=ARRAY
5: G=INTEGER H=TABLE", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_AggregateAliasing_names_conditional_and_immediate_assignment_and_function_parameters()
    {
        var s = @"
        DEFINE('F(X)')   :(SKIP)
F       X = 'changed'   :(RETURN)
SKIP    DEFINE('G(X)')   :(SKIP2)
G       G = X   :(RETURN)
SKIP2   DEFINE('H(X)Y')   :(SKIP3)
H       Y = X
        X = 5
        H = Y   :(RETURN)
SKIP3   V = ARRAY(3)
        L = V
        F(V)
        OUTPUT = '1 V=' DATATYPE(V) ' L=' DATATYPE(L)
        W = G(V)
        OUTPUT = '2 V=' DATATYPE(V) ' W=' DATATYPE(W)
        V = TABLE()
        OUTPUT = '3 V=' DATATYPE(V) ' L=' DATATYPE(L) ' W=' DATATYPE(W)
        Q = TABLE()
        R = Q
        'abc' ? LEN(1) . Q
        OUTPUT = '4 Q=' DATATYPE(Q) ' R=' DATATYPE(R)
        Q2 = TABLE()
        R2 = Q2
        'abc' ? LEN(2) $ Q2
        OUTPUT = '5 Q2=' DATATYPE(Q2) ' R2=' DATATYPE(R2)
        Q3 = TABLE()
        R3 = Q3
        $'Q3' = 'indirect'
        OUTPUT = '6 Q3=' DATATYPE(Q3) ' R3=' DATATYPE(R3)
        Q4 = TABLE()
        R4 = Q4
        N = .Q4
        $N = 'viaName'
        OUTPUT = '7 Q4=' DATATYPE(Q4) ' R4=' DATATYPE(R4)
        Q5 = TABLE()
        R5 = Q5
        R5 = 'r5'
        OUTPUT = '8 Q5=' DATATYPE(Q5) ' R5=' DATATYPE(R5)
        T = TABLE()
        Q6 = ARRAY(2)
        T<'k'> = Q6
        Q6 = 'gone'
        OUTPUT = '9 Q6=' DATATYPE(Q6) ' T<k>=' DATATYPE(T<'k'>)
        Q7 = ARRAY(2)
        R7 = Q7
        Q7<1> = 'e1'
        OUTPUT = '10 R7<1>=' R7<1>
        Q8 = ARRAY(2)
        R8 = Q8
        R8 = H(Q8)
        OUTPUT = '11 Q8=' DATATYPE(Q8) ' R8=' DATATYPE(R8)
        DATA('NODE(VAL,NEXT)')
        Q9 = NODE(1, 2)
        R9 = Q9
        Q9 = 'x'
        OUTPUT = '12 Q9=' DATATYPE(Q9) ' R9=' DATATYPE(R9)
        Q10 = NODE(1, 2)
        R10 = Q10
        R10 = NODE(5, 6)
        OUTPUT = '13 Q10=' VAL(Q10) ' R10=' VAL(R10)
        Q11 = NODE(1, 2)
        R11 = Q11
        VAL(R11) = 99
        OUTPUT = '14 Q11=' VAL(Q11) ' R11=' VAL(R11)
        Q12 = TABLE()
        Q12<'a'> = 1
        R12 = Q12
        Q12 = TABLE()
        OUTPUT = '15 R12<a>=' R12<'a'> ' Q12<a>=' Q12<'a'>
END";
        Assert.AreEqual(@"1 V=ARRAY L=ARRAY
2 V=ARRAY W=ARRAY
3 V=TABLE L=ARRAY W=ARRAY
4 Q=STRING R=TABLE
5 Q2=STRING R2=TABLE
6 Q3=STRING R3=TABLE
7 Q4=STRING R4=TABLE
8 Q5=TABLE R5=STRING
9 Q6=STRING T<k>=ARRAY
10 R7<1>=e1
11 Q8=ARRAY R8=ARRAY
12 Q9=STRING R9=NODE
13 Q10=1 R10=5
14 Q11=99 R11=99
15 R12<a>=1 Q12<a>=", SetupTests.RunWithInput(s));
    }
}
