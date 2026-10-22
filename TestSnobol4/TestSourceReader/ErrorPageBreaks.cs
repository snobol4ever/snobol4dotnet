using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// The pages of compile errors that SPITBOL prints on standard output break between the errors, never inside one: the check for a new page
/// is made before the physical line that opens an error's report, not before its pointer, its message or its empty line, so a page holds
/// 13 errors (the first, under the head) and then 14, the empty line that -e leaves before the pointer not counted. The expected text is what
/// sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ErrorPageBreaks
{
    [TestMethod]
    public void TEST_ErrorPageBreaks_fourteen_errors_break_after_the_thirteenth()
    {
        var program = @"        X = 0 +
        X = 1 +
        X = 2 +
        X = 3 +
        X = 4 +
        X = 5 +
        X = 6 +
        X = 7 +
        X = 8 +
        X = 9 +
        X = 10 +
        X = 11 +
        X = 12 +
        X = 13 +
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 0 +
                      !
Test.sno(1,15) : ERROR 221 -- syntax error: missing operand

2               X = 1 +
                      !
Test.sno(2,15) : ERROR 221 -- syntax error: missing operand

3               X = 2 +
                      !
Test.sno(3,15) : ERROR 221 -- syntax error: missing operand

4               X = 3 +
                      !
Test.sno(4,15) : ERROR 221 -- syntax error: missing operand

5               X = 4 +
                      !
Test.sno(5,15) : ERROR 221 -- syntax error: missing operand

6               X = 5 +
                      !
Test.sno(6,15) : ERROR 221 -- syntax error: missing operand

7               X = 6 +
                      !
Test.sno(7,15) : ERROR 221 -- syntax error: missing operand

8               X = 7 +
                      !
Test.sno(8,15) : ERROR 221 -- syntax error: missing operand

9               X = 8 +
                      !
Test.sno(9,15) : ERROR 221 -- syntax error: missing operand

10              X = 9 +
                      !
Test.sno(10,15) : ERROR 221 -- syntax error: missing operand

11              X = 10 +
                       !
Test.sno(11,16) : ERROR 221 -- syntax error: missing operand

12              X = 11 +
                       !
Test.sno(12,16) : ERROR 221 -- syntax error: missing operand

13              X = 12 +
                       !
Test.sno(13,16) : ERROR 221 -- syntax error: missing operand




                                                                                                                page 2

14              X = 13 +
                       !
Test.sno(14,16) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ErrorPageBreaks_thirty_errors_break_every_fourteen()
    {
        var program = @"        X = 0 +
        X = 1 +
        X = 2 +
        X = 3 +
        X = 4 +
        X = 5 +
        X = 6 +
        X = 7 +
        X = 8 +
        X = 9 +
        X = 10 +
        X = 11 +
        X = 12 +
        X = 13 +
        X = 14 +
        X = 15 +
        X = 16 +
        X = 17 +
        X = 18 +
        X = 19 +
        X = 20 +
        X = 21 +
        X = 22 +
        X = 23 +
        X = 24 +
        X = 25 +
        X = 26 +
        X = 27 +
        X = 28 +
        X = 29 +
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 0 +
                      !
Test.sno(1,15) : ERROR 221 -- syntax error: missing operand

2               X = 1 +
                      !
Test.sno(2,15) : ERROR 221 -- syntax error: missing operand

3               X = 2 +
                      !
Test.sno(3,15) : ERROR 221 -- syntax error: missing operand

4               X = 3 +
                      !
Test.sno(4,15) : ERROR 221 -- syntax error: missing operand

5               X = 4 +
                      !
Test.sno(5,15) : ERROR 221 -- syntax error: missing operand

6               X = 5 +
                      !
Test.sno(6,15) : ERROR 221 -- syntax error: missing operand

7               X = 6 +
                      !
Test.sno(7,15) : ERROR 221 -- syntax error: missing operand

8               X = 7 +
                      !
Test.sno(8,15) : ERROR 221 -- syntax error: missing operand

9               X = 8 +
                      !
Test.sno(9,15) : ERROR 221 -- syntax error: missing operand

10              X = 9 +
                      !
Test.sno(10,15) : ERROR 221 -- syntax error: missing operand

11              X = 10 +
                       !
Test.sno(11,16) : ERROR 221 -- syntax error: missing operand

12              X = 11 +
                       !
Test.sno(12,16) : ERROR 221 -- syntax error: missing operand

13              X = 12 +
                       !
Test.sno(13,16) : ERROR 221 -- syntax error: missing operand




                                                                                                                page 2

14              X = 13 +
                       !
Test.sno(14,16) : ERROR 221 -- syntax error: missing operand

15              X = 14 +
                       !
Test.sno(15,16) : ERROR 221 -- syntax error: missing operand

16              X = 15 +
                       !
Test.sno(16,16) : ERROR 221 -- syntax error: missing operand

17              X = 16 +
                       !
Test.sno(17,16) : ERROR 221 -- syntax error: missing operand

18              X = 17 +
                       !
Test.sno(18,16) : ERROR 221 -- syntax error: missing operand

19              X = 18 +
                       !
Test.sno(19,16) : ERROR 221 -- syntax error: missing operand

20              X = 19 +
                       !
Test.sno(20,16) : ERROR 221 -- syntax error: missing operand

21              X = 20 +
                       !
Test.sno(21,16) : ERROR 221 -- syntax error: missing operand

22              X = 21 +
                       !
Test.sno(22,16) : ERROR 221 -- syntax error: missing operand

23              X = 22 +
                       !
Test.sno(23,16) : ERROR 221 -- syntax error: missing operand

24              X = 23 +
                       !
Test.sno(24,16) : ERROR 221 -- syntax error: missing operand

25              X = 24 +
                       !
Test.sno(25,16) : ERROR 221 -- syntax error: missing operand

26              X = 25 +
                       !
Test.sno(26,16) : ERROR 221 -- syntax error: missing operand

27              X = 26 +
                       !
Test.sno(27,16) : ERROR 221 -- syntax error: missing operand




                                                                                                                page 3

28              X = 27 +
                       !
Test.sno(28,16) : ERROR 221 -- syntax error: missing operand

29              X = 28 +
                       !
Test.sno(29,16) : ERROR 221 -- syntax error: missing operand

30              X = 29 +
                       !
Test.sno(30,16) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ErrorPageBreaks_under_e_the_empty_line_before_the_pointer_is_not_counted()
    {
        var program = @"        X = 0 +
        X = 1 +
        X = 2 +
        X = 3 +
        X = 4 +
        X = 5 +
        X = 6 +
        X = 7 +
        X = 8 +
        X = 9 +
        X = 10 +
        X = 11 +
        X = 12 +
        X = 13 +
        X = 14 +
        X = 15 +
        X = 16 +
        X = 17 +
        X = 18 +
        X = 19 +
        X = 20 +
        X = 21 +
        X = 22 +
        X = 23 +
        X = 24 +
        X = 25 +
        X = 26 +
        X = 27 +
        X = 28 +
        X = 29 +
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = 0 +

                      !
Test.sno(1,15) : ERROR 221 -- syntax error: missing operand

2               X = 1 +

                      !
Test.sno(2,15) : ERROR 221 -- syntax error: missing operand

3               X = 2 +

                      !
Test.sno(3,15) : ERROR 221 -- syntax error: missing operand

4               X = 3 +

                      !
Test.sno(4,15) : ERROR 221 -- syntax error: missing operand

5               X = 4 +

                      !
Test.sno(5,15) : ERROR 221 -- syntax error: missing operand

6               X = 5 +

                      !
Test.sno(6,15) : ERROR 221 -- syntax error: missing operand

7               X = 6 +

                      !
Test.sno(7,15) : ERROR 221 -- syntax error: missing operand

8               X = 7 +

                      !
Test.sno(8,15) : ERROR 221 -- syntax error: missing operand

9               X = 8 +

                      !
Test.sno(9,15) : ERROR 221 -- syntax error: missing operand

10              X = 9 +

                      !
Test.sno(10,15) : ERROR 221 -- syntax error: missing operand

11              X = 10 +

                       !
Test.sno(11,16) : ERROR 221 -- syntax error: missing operand

12              X = 11 +

                       !
Test.sno(12,16) : ERROR 221 -- syntax error: missing operand

13              X = 12 +

                       !
Test.sno(13,16) : ERROR 221 -- syntax error: missing operand




                                                                                                                page 2

14              X = 13 +

                       !
Test.sno(14,16) : ERROR 221 -- syntax error: missing operand

15              X = 14 +

                       !
Test.sno(15,16) : ERROR 221 -- syntax error: missing operand

16              X = 15 +

                       !
Test.sno(16,16) : ERROR 221 -- syntax error: missing operand

17              X = 16 +

                       !
Test.sno(17,16) : ERROR 221 -- syntax error: missing operand

18              X = 17 +

                       !
Test.sno(18,16) : ERROR 221 -- syntax error: missing operand

19              X = 18 +

                       !
Test.sno(19,16) : ERROR 221 -- syntax error: missing operand

20              X = 19 +

                       !
Test.sno(20,16) : ERROR 221 -- syntax error: missing operand

21              X = 20 +

                       !
Test.sno(21,16) : ERROR 221 -- syntax error: missing operand

22              X = 21 +

                       !
Test.sno(22,16) : ERROR 221 -- syntax error: missing operand

23              X = 22 +

                       !
Test.sno(23,16) : ERROR 221 -- syntax error: missing operand

24              X = 23 +

                       !
Test.sno(24,16) : ERROR 221 -- syntax error: missing operand

25              X = 24 +

                       !
Test.sno(25,16) : ERROR 221 -- syntax error: missing operand

26              X = 25 +

                       !
Test.sno(26,16) : ERROR 221 -- syntax error: missing operand

27              X = 26 +

                       !
Test.sno(27,16) : ERROR 221 -- syntax error: missing operand




                                                                                                                page 3

28              X = 27 +

                       !
Test.sno(28,16) : ERROR 221 -- syntax error: missing operand

29              X = 28 +

                       !
Test.sno(29,16) : ERROR 221 -- syntax error: missing operand

30              X = 29 +

                       !
Test.sno(30,16) : ERROR 221 -- syntax error: missing operand", SetupTests.MaskListingHead(SetupTests.RunWithInputDirectives("-b -e", program)));
    }
}
