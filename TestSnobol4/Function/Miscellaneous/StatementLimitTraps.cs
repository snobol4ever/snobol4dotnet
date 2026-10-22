using Test.TestLexer;

namespace Test.Keywords;

/// <summary>
/// Error 244 when &amp;ERRLIMIT lets the run go on: the first two times it is an error like any other (the statement fails, &amp;ERRLIMIT counts
/// down), the count is set to the limit that was passed and the limit goes up by ten, and the budget restarts at the new limit; the third
/// time, or when &amp;ERRLIMIT is 0, it ends the run. The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class StatementLimitTraps
{
    [TestMethod]
    public void TEST_StatementLimitTraps_errlimit_0()
    {
        var program = @"        &ERRLIMIT = 0
        &STLIMIT = 20
L       N = N + 1
        OUTPUT = N
        :(L)
END";
        Assert.AreEqual(@"1
2
3
4
5
6



Test.sno(4) : ERROR 244 -- statement count exceeds value of stlimit keyword





in file              Test.sno
in line              4
in statement         4
stmts executed       20
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StatementLimitTraps_errlimit_1()
    {
        var program = @"        &ERRLIMIT = 1
        &STLIMIT = 20
L       N = N + 1
        OUTPUT = N
        :(L)
END";
        Assert.AreEqual(@"1
2
3
4
5
6
8
9
10
11
12
13
14
15
16



Test.sno(4) : ERROR 244 -- statement count exceeds value of stlimit keyword





in file              Test.sno
in line              4
in statement         4
stmts executed       30
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StatementLimitTraps_errlimit_2()
    {
        var program = @"        &ERRLIMIT = 2
        &STLIMIT = 20
L       N = N + 1
        OUTPUT = N
        :(L)
END";
        Assert.AreEqual(@"1
2
3
4
5
6
8
9
10
11
12
13
14
15
16
18
19
20
21
22
23
24
25
26
27
28
29
30



Test.sno(5) : ERROR 244 -- statement count exceeds value of stlimit keyword





in file              Test.sno
in line              5
in statement         5
stmts executed       40
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StatementLimitTraps_errlimit_3()
    {
        var program = @"        &ERRLIMIT = 3
        &STLIMIT = 20
L       N = N + 1
        OUTPUT = N
        :(L)
END";
        Assert.AreEqual(@"1
2
3
4
5
6
8
9
10
11
12
13
14
15
16
18
19
20
21
22
23
24
25
26
27
28
29
30



Test.sno(5) : ERROR 244 -- statement count exceeds value of stlimit keyword





in file              Test.sno
in line              5
in statement         5
stmts executed       40
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_StatementLimitTraps_errlimit_1000()
    {
        var program = @"        &ERRLIMIT = 1000
        &STLIMIT = 20
L       N = N + 1
        OUTPUT = N
        :(L)
END";
        Assert.AreEqual(@"1
2
3
4
5
6
8
9
10
11
12
13
14
15
16
18
19
20
21
22
23
24
25
26
27
28
29
30



Test.sno(5) : ERROR 244 -- statement count exceeds value of stlimit keyword





in file              Test.sno
in line              5
in statement         5
stmts executed       40
execution time msec  -
REGENERATIONS        -
memory used (bytes)  -
memory left (bytes)  -", SetupTests.MaskReport(SetupTests.RunWithInput(program)));
    }
}
