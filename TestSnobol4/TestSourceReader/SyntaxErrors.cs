using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// A syntax error is found as SPITBOL finds it, by reading the statement element by element: the code is that of the pair of an
/// element and the state of the analyzer it was met in, and the column is where that element begins (a missing operand is reported
/// at the operator that came where an operand should be, a misplaced operator under itself, an unbalanced paren at the terminator). A
/// binary operator needs blanks before it and a unary one too unless it follows an operator, an opening bracket or a comma, and + and -
/// make a number of what they follow (1+2) and end no name (A+B). Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SyntaxErrors
{
    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_before_a_second_binary_operator()
    {
        var program = @"        X = 1 + * 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 + * 2
                        !
Test.sno(1,17) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_before_a_second_star()
    {
        var program = @"        X = 1 * * 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 * * 2
                        !
Test.sno(1,17) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_before_a_plus()
    {
        var program = @"        X = 1 + + 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 + + 1
                        !
Test.sno(1,17) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_before_a_bar()
    {
        var program = @"        X = 1 | | 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 | | 2
                        !
Test.sno(1,17) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_after_equal_then_equal()
    {
        var program = @"        X = = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = = 1
                    !
Test.sno(1,13) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_after_a_dot_then_equal()
    {
        var program = @"        X . = 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X . = 1
                    !
Test.sno(1,13) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operand_before_a_bang()
    {
        var program = @"        X = ! Y
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = ! Y
                    !
Test.sno(1,13) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_binary_operator_first_is_a_missing_operand()
    {
        var program = @"        = 
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               =
                !
Test.sno(1,9) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_bar_first_is_a_missing_operand()
    {
        var program = @"        X = | 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = | 1
                    !
Test.sno(1,13) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_two_equals()
    {
        var program = @"        X == 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X == 1
                   !
Test.sno(1,12) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_equal_after_a_dot()
    {
        var program = @"        .= X
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               .= X
                 !
Test.sno(1,10) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_bar_equal()
    {
        var program = @"        X |= 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X |= 1
                   !
Test.sno(1,12) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_two_dollars()
    {
        var program = @"        X $$ Y
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X $$ Y
                   !
Test.sno(1,12) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_dollar_dot()
    {
        var program = @"        X = $.
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = $.
                     !
Test.sno(1,14) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_two_bangs()
    {
        var program = @"        !!
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               !!
                 !
Test.sno(1,10) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_two_pluses()
    {
        var program = @"        X = 1 ++ 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 ++ 1
                       !
Test.sno(1,16) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_plus_minus()
    {
        var program = @"        X = 1 +- 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 +- 1
                       !
Test.sno(1,16) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_two_bars()
    {
        var program = @"        X = 1 || 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 || 2
                       !
Test.sno(1,16) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_after_a_paren()
    {
        var program = @"        X = (| 1)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = (| 1)
                     !
Test.sno(1,14) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_invalid_operator_dollar_closing_a_goto()
    {
        var program = @"        X = A :S($)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S($)
                         !
Test.sno(1,18) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_plus_ends_no_name()
    {
        var program = @"        X = A+B
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A+B
                    !
Test.sno(1,13) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_star_with_no_blanks()
    {
        var program = @"        X = A*B
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A*B
                     !
Test.sno(1,14) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_an_equal_with_no_blanks()
    {
        var program = @"        X=1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X=1
                 !
Test.sno(1,10) : ERROR 233 -- syntax error: invalid use of operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_number_the_plus_goes_on()
    {
        var program = @"        X = 1+2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1+2
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_number_the_minus_goes_on()
    {
        var program = @"        X = 1-
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1-
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_number_the_plus_ends_the_statement()
    {
        var program = @"        X = 1+
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1+
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_real_with_two_points()
    {
        var program = @"        X = 1.2.3
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1.2.3
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_an_exponent_with_no_digits()
    {
        var program = @"        X = 1e
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1e
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_an_exponent_with_a_sign_and_no_digits()
    {
        var program = @"        X = 1e+
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1e+
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_two_exponents()
    {
        var program = @"        X = 1e5e
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1e5e
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_letter_after_a_digit()
    {
        var program = @"        X = 3A
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 3A
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_a_letter_after_a_digit_then_more()
    {
        var program = @"        X = 3A + 1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 3A + 1
                    !
Test.sno(1,13) : ERROR 231 -- syntax error: invalid numeric item", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_right_paren_at_the_end()
    {
        var program = @"        X = (1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = (1
                     !
Test.sno(1,14) : ERROR 226 -- syntax error: missing right paren", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_right_paren_after_a_blank_paren()
    {
        var program = @"        X ? ( 
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X ? (
                    !
Test.sno(1,13) : ERROR 226 -- syntax error: missing right paren", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_right_paren_of_a_call()
    {
        var program = @"        X = F( 1 
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = F( 1
                       !
Test.sno(1,16) : ERROR 226 -- syntax error: missing right paren", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_right_paren_of_a_goto()
    {
        var program = @"        X = A :(END
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(END
                        !
Test.sno(1,17) : ERROR 227 -- syntax error: right paren missing from goto", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_right_paren_of_a_goto_in_a_conditional()
    {
        var program = @"        X = A :S(L
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S(L
                         !
Test.sno(1,18) : ERROR 227 -- syntax error: right paren missing from goto", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_array_bracket()
    {
        var program = @"        X = A<1><
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A<1><
                        !
Test.sno(1,17) : ERROR 229 -- syntax error: missing right array bracket", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_array_bracket_at_a_blank()
    {
        var program = @"        X = A<1>< 
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A<1><
                        !
Test.sno(1,17) : ERROR 229 -- syntax error: missing right array bracket", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_missing_operator_before_a_quote()
    {
        var program = @"        X = A 'abc'def
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A 'abc'def
                           !
Test.sno(1,20) : ERROR 220 -- syntax error: missing operator", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_goto_field_after_a_colon()
    {
        var program = @"        : S
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               : S
                  !
Test.sno(1,11) : ERROR 234 -- syntax error: goto field incorrect", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_goto_field_that_is_a_label()
    {
        var program = @"        X = A :L
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :L
                       !
Test.sno(1,16) : ERROR 234 -- syntax error: goto field incorrect", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_goto_field_that_is_a_number()
    {
        var program = @"        X = A :1
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :1
                       !
Test.sno(1,16) : ERROR 234 -- syntax error: goto field incorrect", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_empty_goto_field()
    {
        var program = @"        X = A :;
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :;
                       !
Test.sno(1,16) : ERROR 219 -- syntax error: empty goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_empty_goto_field_at_the_end()
    {
        var program = @"        X = A :
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :
                      !
Test.sno(1,15) : ERROR 219 -- syntax error: empty goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_duplicated_goto()
    {
        var program = @"        X = A :(L)(M)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(L)(M)
                            !
Test.sno(1,21) : ERROR 218 -- syntax error: duplicated goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_duplicated_success_goto()
    {
        var program = @"        X = A :S(L)S(M)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S(L)S(M)
                              !
Test.sno(1,23) : ERROR 218 -- syntax error: duplicated goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_duplicated_failure_goto()
    {
        var program = @"        X = A :F(L)F(M)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :F(L)F(M)
                              !
Test.sno(1,23) : ERROR 218 -- syntax error: duplicated goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_success_goto_after_a_goto()
    {
        var program = @"        X = A :(L)S(M)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(L)S(M)
                             !
Test.sno(1,22) : ERROR 218 -- syntax error: duplicated goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_comma_in_a_goto()
    {
        var program = @"        X = A :S(L,)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S(L,)
                          !
Test.sno(1,19) : ERROR 223 -- syntax error: invalid use of comma", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_comma_at_the_top()
    {
        var program = @"        X = 1, 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1, 2
                     !
Test.sno(1,14) : ERROR 223 -- syntax error: invalid use of comma", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_unbalanced_right_paren()
    {
        var program = @"        X = 1)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1)
                     !
Test.sno(1,14) : ERROR 224 -- syntax error: unbalanced right parenthesis", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_unbalanced_right_bracket()
    {
        var program = @"        X = A<1>>
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A<1>>
                        !
Test.sno(1,17) : ERROR 225 -- syntax error: unbalanced right bracket", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_left_bracket_after_an_operator()
    {
        var program = @"        X = 1 + <2>
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 + <2>
                        !
Test.sno(1,17) : ERROR 222 -- syntax error: invalid use of left bracket", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_error_at_a_semicolon_takes_the_next_statement()
    {
        var program = @"        X = 1 + ; Y = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 + ; Y = 2
                        !
Test.sno(1,17) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_error_at_a_colon_goto_semicolon()
    {
        var program = @"        X = A :; Y = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :; Y = 2
                       !
Test.sno(1,16) : ERROR 219 -- syntax error: empty goto field", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_SyntaxErrors_error_before_a_semicolon_leaves_the_next_statement()
    {
        var program = @"        X = 1 + * 2 ; Y = 2
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 1 + * 2 ; Y = 2
                        !
Test.sno(1,17) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
