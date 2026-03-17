using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

[DoNotParallelize]
[TestClass]
public class Rung11_DataStructures
{
    /// <summary>
    /// Runs a script and returns PASS/FAIL lines written to OUTPUT (Console.Error in DOTNET).
    /// Uses a lock to be safe under the assembly-level parallel test runner.
    /// </summary>
    private static readonly object s_consoleLock = new();

    private static List<string> RunGetOutput(string script)
    {
        var lines = new List<string>();
        lock (s_consoleLock)
        {
            var old = Console.Error;
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms) { AutoFlush = true };
            Console.SetError(sw);
            try { SetupTests.SetupScript("-b", script); }
            finally { Console.SetError(old); }
            ms.Position = 0;
            using var sr = new System.IO.StreamReader(ms);
            foreach (var line in sr.ReadToEnd().Split('\n'))
            {
                var t = line.Trim();
                if (t.StartsWith("PASS") || t.StartsWith("FAIL")) lines.Add(t);
            }
        }
        return lines;
    }

    [TestMethod]
    public void TEST_Corpus_1110_array_1d()
    {
        var s = @"
        a = array(3)
        differ(a<1>)               :f(e001)
        output = 'FAIL 1110/001: array element init null' :(end)
e001
        a<2> = 4.5
        differ(a<2>, 4.5)                   :f(e002)
        output = 'FAIL 1110/002: array assign/read'    :(end)
e002
        a<4>                       :f(e003)
        output = 'FAIL 1110/003: OOB high should fail' :(end)
e003
        a<0>                       :f(e004)
        output = 'FAIL 1110/004: OOB zero should fail' :(end)
e004
        differ(prototype(a), '3')                   :f(e005)
        output = 'FAIL 1110/005: prototype(array(3))=3' :(end)
e005
        b = array('3')
        b<2> = 'x'
        differ(b<2>, 'x')                   :f(e006)
        output = 'FAIL 1110/006: array from string dim' :(end)
e006
        differ(prototype(b), '3')                   :f(e007)
        output = 'FAIL 1110/007: prototype string-dim array' :(end)
e007
        a<1> = 3.14
        differ(a<1>, 3.14)                   :f(e008)
        output = 'FAIL 1110/008: array stores real'    :(end)
e008
        a<3> = 'z'
        differ(a<3>, 'z')                   :f(e009)
        output = 'FAIL 1110/009: array stores string'  :(end)
e009
        output = 'PASS 1110_array_1d (9/9)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1111_array_default()
    {
        var s = @"
        b = array(3, 10)
        differ(b<2>, 10)                   :f(e001)
        output = 'FAIL 1111/001: array default value'  :(end)
e001
        differ(b<1>, 10)                   :f(e002)
        output = 'FAIL 1111/002: default fills all slots' :(end)
e002
        output = 'PASS 1111_array_default (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1112_array_multi()
    {
        var s = @"
        c = array('2,2')
        c<1,2> = '*'
        differ(c<1,2>, '*')                   :f(e001)
        output = 'FAIL 1112/001: 2D array assign/read' :(end)
e001
        differ(prototype(c), '2,2')                   :f(e002)
        output = 'FAIL 1112/002: prototype of 2D array' :(end)
e002
        d = array('-1:1,2')
        d<-1,1> = 0
        differ(d<-1,1>, 0)                   :f(e003)
        output = 'FAIL 1112/003: custom lower bound assign/read' :(end)
e003
        d<-2,1>                    :f(e004)
        output = 'FAIL 1112/004: below lower bound fails' :(end)
e004
        d<2,1>                     :f(e005)
        output = 'FAIL 1112/005: above upper bound fails' :(end)
e005
        output = 'PASS 1112_array_multi (5/5)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1113_table()
    {
        var s = @"
        t = table(10)
        differ(t<'cat'>)           :f(e001)
        output = 'FAIL 1113/001: absent key is null'   :(end)
e001
        t<'cat'> = 'dog'
        differ(t<'cat'>, 'dog')                   :f(e002)
        output = 'FAIL 1113/002: string key assign/read' :(end)
e002
        t<7> = 45
        differ(t<7>, 45)                   :f(e003)
        output = 'FAIL 1113/003: integer key assign/read' :(end)
e003
        differ(t<'cat'>, 'dog')                   :f(e004)
        output = 'FAIL 1113/004: string key survives int key add' :(end)
e004
        ta = convert(t, 'array')
        differ(prototype(ta), '2,2')                   :f(e005)
        output = 'FAIL 1113/005: table->array prototype 2,2' :(end)
e005
        ata = convert(ta, 'table')
        differ(ata<7>, 45)                   :f(e006)
        output = 'FAIL 1113/006: array->table int key roundtrip' :(end)
e006
        differ(ata<'cat'>, 'dog')                   :f(e007)
        output = 'FAIL 1113/007: array->table string key roundtrip' :(end)
e007
        t['cat'] = 'fish'
        differ(t<'cat'>, 'fish')                   :f(e008)
        output = 'FAIL 1113/008: [] and <> syntax equivalent' :(end)
e008
        output = 'PASS 1113_table (8/8)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1114_item()
    {
        var s = @"
        aaa = array(10)
        item(aaa, 1) = 5
        differ(item(aaa, 1), 5)                   :f(e001)
        output = 'FAIL 1114/001: item 1D assign/read'  :(end)
e001
        differ(aaa<1>, 5)                   :f(e002)
        output = 'FAIL 1114/002: item == bracket read' :(end)
e002
        aaa<2> = 22
        differ(item(aaa, 2), 22)                   :f(e003)
        output = 'FAIL 1114/003: bracket assign, item read' :(end)
e003
        ama = array('2,2,2,2')
        item(ama, 1,2,1,2) = 1212
        differ(item(ama, 1,2,1,2), 1212)                   :f(e004)
        output = 'FAIL 1114/004: item 4D assign/read'  :(end)
e004
        differ(ama<1,2,1,2>, 1212)                   :f(e005)
        output = 'FAIL 1114/005: item 4D == bracket'   :(end)
e005
        ama<2,1,2,1> = 2121
        differ(item(ama, 2,1,2,1), 2121)                   :f(e006)
        output = 'FAIL 1114/006: bracket 4D assign, item read' :(end)
e006
        tt = table()
        item(tt, 'key') = 'val'
        differ(item(tt, 'key'), 'val')                   :f(e007)
        output = 'FAIL 1114/007: item on table'        :(end)
e007
        output = 'PASS 1114_item (7/7)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1115_data_basic()
    {
        var s = @"
        data('node(val,lson,rson)')
        a = node('x', 'y', 'z')
        differ(datatype(a), 'node')                   :f(e001)
        output = 'FAIL 1115/001: datatype of node'     :(end)
e001
        differ(val(a), 'x')                   :f(e002)
        output = 'FAIL 1115/002: field accessor val'   :(end)
e002
        b = node()
        differ(rson(b))            :f(e003)
        output = 'FAIL 1115/003: unset field is null'  :(end)
e003
        lson(b) = a
        differ(rson(lson(b)), 'z')                   :f(e004)
        output = 'FAIL 1115/004: nested accessor after mutate' :(end)
e004
        differ(value('b'), b)                   :f(e005)
        output = 'FAIL 1115/005: value() by variable name' :(end)
e005
        val(a) = 'new'
        differ(val(a), 'new')                   :f(e006)
        output = 'FAIL 1115/006: mutate field and read back' :(end)
e006
        output = 'PASS 1115_data_basic (6/6)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1116_data_overlap()
    {
        var s = @"
        data('node(val,lson,rson)')
        data('clunk(value,lson)')
        a = node('x', 'y', 'z')
        b = node()
        lson(b) = a
        differ(rson(lson(b)), 'z')                   :f(e001)
        output = 'FAIL 1116/001: node.rson after clunk data def' :(end)
e001
        differ(value('b'), b)                   :f(e002)
        output = 'FAIL 1116/002: value() still works after clunk' :(end)
e002
        c = clunk('alpha', 'beta')
        differ(lson(c), 'beta')                   :f(e003)
        output = 'FAIL 1116/003: clunk.lson accessor'  :(end)
e003
        output = 'PASS 1116_data_overlap (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }
}
