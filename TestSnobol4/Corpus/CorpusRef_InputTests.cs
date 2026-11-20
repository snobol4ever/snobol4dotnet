using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Corpus tests requiring stdin INPUT — uses RunWithInput() with embedded input.
/// Covers: arith/fileinfo, arith/triplet, control/expr_eval,
///         strings/word1, word2, word3, word4, wordcount, strings/cross.
///
/// Note: strings/cross tests @N cursor capture (M-NET-P35-FIX bug).
/// </summary>
[DoNotParallelize]
[TestClass]
public class CorpusRef_InputTests
{

    [TestMethod, Ignore("M-NET-EVAL-COMPLETE: recursive expr evaluator uses NRETURN + *func() side-effects + EVAL — fix pending")]
    public void TEST_Corpus_control_expr_eval()
    {
        var s = @"
         DEFINE('Push(x)')
         stk      =  TABLE()                       :(PushEnd)
Push     stk[0]   =  stk[0] + 1
         Push     =  .stk[stk[0]]
         $Push    =  x                             :(NRETURN)
PushEnd
         DEFINE('Pop()')                           :(PopEnd)
Pop      Pop      =  stk[stk[0]]
         stk[0]   =  stk[0] - 1                    :(RETURN)
PopEnd
         DEFINE('Unary()arg,op')                   :(UnaryEnd)
Unary    arg      =  Pop()
         op       =  Pop()
         Push()   =  EVAL(op arg)
         Unary    =  .dummy                        :(NRETURN)
UnaryEnd
         DEFINE('Binary()op,left,right')           :(BinaryEnd)
Binary   right    =  Pop()
         op       =  Pop()
         left     =  Pop()
         Push()   =  EVAL(left ' ' op ' ' right)
         Binary   =  .dummy                        :(NRETURN)
BinaryEnd

         integer  =  SPAN('0123456789')
         exponent =  ANY('eEdD') (ANY('+-') | epsilon) integer
         real     =  integer '.' (integer | epsilon) (exponent | epsilon)
+                 |  integer exponent

         addop    =  ANY('+-') . *Push()
         mulop    =  ANY('*/') . *Push()
         constant =  (real | integer) . *Push()

         primary  =  constant | '(' *expr ')'

         factor   =  addop *factor . *Unary()
+                 |  *primary

         term     =  *factor mulop *term . *Binary()
+                 |  *factor

         expr     =  *term addop *expr . *Binary()
+                 |  *term

         &TRIM    =  1
loop     line     =  INPUT                         :F(END)
         line     POS(0) expr RPOS(0)              :F(error)
         OUTPUT   =  Pop()                         :(loop)
error    OUTPUT   = 'Bad input, try again'         :(loop)
END";
        var input = "1+2*3"+ Environment.NewLine + "(1+2)*3"+ Environment.NewLine + "2.5e1+0.5"+ Environment.NewLine + "-3+10"+ Environment.NewLine + "4*5+6";
        Assert.AreEqual("7"+ Environment.NewLine + "9"+ Environment.NewLine + "25.5"+ Environment.NewLine + "7"+ Environment.NewLine + "26", SetupTests.RunWithInput(s, input));
    }
}
