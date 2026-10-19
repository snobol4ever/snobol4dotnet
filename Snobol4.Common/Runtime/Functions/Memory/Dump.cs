using System.Globalization;
using System.Text;

namespace Snobol4.Common;

//"dump argument is not integer" /* 88 */,
//"dump argument is negative or too large" /* 89 */,

public partial class Executive
{
    // SPITBOL's dump keyword list (vdmkw), in its order
    private static readonly string[] DumpKeywords =
    [
        "ANCHOR", "CASE", "CODE", "DUMP", "ERRLIMIT", "ERRTEXT", "ERRTYPE", "FILE", "FNCLEVEL", "FTRACE",
        "FULLSCAN", "INPUT", "LASTFILE", "LASTLINE", "LASTNO", "LINE", "MAXLNGTH", "OUTPUT", "PROFILE",
        "RTNTYPE", "STCOUNT", "STLIMIT", "STNO", "TRACE", "TRIM"
    ];

    // The variable prtnm found last time (SPITBOL's prnmv), tried first on the next search
    private string? _lastNamedVariable;

    internal void DisplayVariableValues(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.INTEGER, out _, out var value, this))
        {
            LogRuntimeException(88);
            return;
        }

        DisplayVariableValues((long)value, Console.Out);
    }

    // The end-of-run dump that &DUMP asks for goes where SPITBOL's end-of-run report goes (stopr prints
    // on the listing channel): the -o file when one is named, else standard output
    internal void DisplayVariableValues()
    {
        DisplayVariableValues(AmpDump, Parent.BuildOptions.ListFileName != "" ? Parent.ListingWriter : Console.Out);
    }

    /// <summary>
    /// DUMP(n), written to standard output as SPITBOL's dumpr writes it: three blank lines, the natural
    /// variables whose values are not null (all of them for 3) in lexical order as NAME = value, the
    /// keyword values, then for 2 and 3 every array, table and DATA object with its elements, each
    /// headed by its value line (ARRAY(3) #1) and its elements named through a variable that holds it;
    /// three blank lines end the dump. 0 does nothing, and so does a larger count (SPITBOL's core dump).
    /// </summary>
    private void DisplayVariableValues(long level, TextWriter writer)
    {
        if (level < 0)
        {
            LogRuntimeException(89);
            return;
        }

        if (level is 0 or > 3)
            return;

        var w = new StringBuilder();
        w.Append("\n\n\ndump of natural variables\n\n");
        foreach (var (name, value) in NaturalVariables().OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (level != 3 && IsNullValue(value))
                continue;
            w.Append(name).Append(" = ").Append(PrintValue(value)).Append('\n');
        }

        w.Append("\n\ndump of keyword values\n\n");
        foreach (var keyword in DumpKeywords)
            w.Append('&').Append(keyword).Append(" = ").Append(PrintValue(KeywordValue(keyword))).Append('\n');

        if (level >= 2)
            DumpBlocks(w, level == 3);

        w.Append("\n\n\n");
        writer.Write(w.ToString());
        writer.Flush();
    }

    // The program's natural variables: every name but the keywords; a protected one (ARB, REM, ...)
    // only once the program has named it, as SPITBOL makes its variable block on first reference
    private IEnumerable<KeyValuePair<string, Var>> NaturalVariables()
    {
        var referenced = Parent.VariableSlots.Select(slot => slot.Symbol).ToHashSet();
        return IdentifierTable.Where(kv => kv.Key.Length > 0 && kv.Key[0] != '&'
                                           && (!kv.Value.IsReadOnly || referenced.Contains(kv.Key)));
    }

    private static bool IsNullValue(Var value) => value is StringVar { Data: "" };

    private Var KeywordValue(string keyword)
    {
        if (!KeywordTable.TryGetValue("&" + keyword, out var handler))
            return StringVar.Null();
        var depth = SystemStack.Count;
        handler(StringVar.Null(), false);
        var value = SystemStack.Count > depth ? SystemStack.Pop() : StringVar.Null();
        while (SystemStack.Count > depth)
            SystemStack.Pop();
        return value;
    }

    // Every array, table and DATA object reachable from the variables, in creation order, as SPITBOL
    // walks its heap: a blank line, the block's value line, then NAME = value for each element that is
    // not null (a table's null entries are never shown)
    private void DumpBlocks(StringBuilder w, bool includeNull)
    {
        var blocks = new Dictionary<object, Var>(ReferenceEqualityComparer.Instance);
        var pending = new Stack<Var>(NaturalVariables().Select(kv => kv.Value));
        while (pending.Count > 0)
        {
            var v = pending.Pop();
            switch (v)
            {
                case ArrayVar a when blocks.TryAdd(a, a):
                    foreach (var e in a.Data) pending.Push(e);
                    break;
                case TableVar t when blocks.TryAdd(t, t):
                    foreach (var e in t.Data.Values) pending.Push(e);
                    foreach (var k in t.Data.Keys) if (k is Var kv) pending.Push(kv);
                    break;
                case ProgramDefinedDataVar d when blocks.TryAdd(d.FieldValues, d):
                    foreach (var e in d.FieldValues.Data) pending.Push(e);
                    break;
            }
        }

        foreach (var block in blocks.Values.OrderBy(BlockIdOf))
        {
            w.Append('\n').Append(PrintValue(block)).Append('\n');
            switch (block)
            {
                case ArrayVar a:
                    for (var i = 0; i < a.Data.Count; i++)
                        if (includeNull || !IsNullValue(a.Data[i]))
                            w.Append(PrintBlockName(a)).Append('<').Append(ArraySubscripts(a, i)).Append("> = ")
                             .Append(PrintValue(a.Data[i])).Append('\n');
                    break;
                case TableVar t:
                    foreach (var (key, value) in TableEntriesInBucketOrder(t))
                        if (!IsNullValue(value))
                            w.Append(PrintBlockName(t)).Append('<').Append(PrintTableKey(key)).Append("> = ")
                             .Append(PrintValue(value)).Append('\n');
                    break;
                case ProgramDefinedDataVar d:
                    for (var i = 0; i < d.FieldValues.Data.Count; i++)
                        if (includeNull || !IsNullValue(d.FieldValues.Data[i]))
                            w.Append(d.Definition.FieldNames[i]).Append('(').Append(PrintBlockName(d)).Append(") = ")
                             .Append(PrintValue(d.FieldValues.Data[i])).Append('\n');
                    break;
            }
        }
    }

    // A block made outside the numbered creation sites gets its number when first printed
    private static long BlockIdOf(Var block)
    {
        switch (block)
        {
            case ArrayVar a:
                if (a.BlockId == 0) a.BlockId = NewBlockId();
                return a.BlockId;
            case TableVar t:
                if (t.BlockId == 0) t.BlockId = NewBlockId();
                return t.BlockId;
            case ProgramDefinedDataVar d:
                if (d.FieldValues.BlockId == 0) d.FieldValues.BlockId = NewBlockId();
                return d.FieldValues.BlockId;
            default:
                return 0;
        }
    }

    // The subscripts of an array's flat index, in declared order (dimensions are stored last first)
    private static string ArraySubscripts(ArrayVar a, int flat)
    {
        var parts = new string[a.Dimensions];
        for (var d = 0; d < a.Dimensions; d++)
            parts[a.Dimensions - 1 - d] = (a.LowerBounds[d] + flat / a.Multipliers[d] % a.Sizes[d]).ToString(CultureInfo.InvariantCulture);
        return string.Join(",", parts);
    }

    // A table's entries as SPITBOL keeps them: by bucket (the key's hash modulo the bucket count), then
    // in the order they were added
    private static IEnumerable<KeyValuePair<object, Var>> TableEntriesInBucketOrder(TableVar t)
    {
        return t.Data.Select((kv, order) => (kv, order))
                     .OrderBy(e => KeyBucket(e.kv.Key, t.Buckets))
                     .ThenBy(e => e.order)
                     .Select(e => e.kv);
    }

    private static long KeyBucket(object key, int buckets)
    {
        long hash = key switch
        {
            string s => SpitbolHash(s),
            long l => l == long.MinValue ? 0 : Math.Abs(l),
            double d => BitConverter.DoubleToInt64Bits(d) is var bits && bits == long.MinValue ? 0 : Math.Abs(BitConverter.DoubleToInt64Bits(d)),
            _ => long.MaxValue   // other keys hash on an address SPITBOL alone knows; they go last
        };
        return hash == long.MaxValue ? long.MaxValue : hash % buckets;
    }

    /// <summary>
    /// SPITBOL's hashs on x64: the length, exclusive-or'ed with the string's first three 8-byte words
    /// (little endian, zero padded), masked to a non-negative integer.
    /// </summary>
    internal static long SpitbolHash(string s)
    {
        long hash = s.Length;
        var words = Math.Min((s.Length + 7) / 8, 3);
        for (var w = 0; w < words; w++)
        {
            ulong word = 0;
            for (var b = 0; b < 8; b++)
            {
                var i = w * 8 + b;
                if (i < s.Length)
                    word |= (ulong)(byte)s[i] << (8 * b);
            }

            hash ^= (long)word;
        }

        return hash & long.MaxValue;
    }

    private string PrintTableKey(object key) => key switch
    {
        string s => $"'{s}'",
        long l => l.ToString(CultureInfo.InvariantCulture),
        double d => RealConversionStrategy.SpitbolRealString(d),
        Var v => PrintValue(v),
        _ => key.ToString() ?? ""
    };

    /// <summary>SPITBOL's prtvl: how DUMP, the dump of names and the trace lines print a value.</summary>
    internal string PrintValue(Var value)
    {
        switch (value)
        {
            case StringVar s:
                return $"'{s.Data}'";
            case IntegerVar or RealVar:
                return value.ToString() ?? "";
            case NameVar n:
                if (n.Pointer.StartsWith('&'))
                    return "NAME";
                return "." + PrintName(n);
            case ArrayVar a:
                return $"ARRAY({a.Prototype}) #{BlockIdOf(a)}";
            case TableVar t:
                return $"TABLE({t.Buckets}) #{BlockIdOf(t)}";
            case ProgramDefinedDataVar d:
                return $"{d.DataName} #{BlockIdOf(d)}";
            // a simple expression *X prints as such; any other is just EXPRESSION
            case ExpressionVar e when SimpleExpressionNames.TryGetValue(e.FunctionName, out var variable):
                return "*" + variable;
            default:
                return value.DataType().ToUpperInvariant();
        }
    }

    // The name a NAME stands for: a variable's own, an element's as BLOCK<subscripts>, a field's as
    // FIELD(BLOCK)
    private string PrintName(NameVar n)
    {
        switch (n.Collection)
        {
            case ArrayVar { DataOwner: { } owner } when n.Key is long field:
                return $"{owner.Definition.FieldNames[(int)field]}({PrintBlockName(owner)})";
            case ArrayVar a when n.Key is long flat:
                return $"{PrintBlockName(a)}<{ArraySubscripts(a, (int)flat)}>";
            case TableVar t when n.Key is not null:
                return $"{PrintBlockName(t)}<{PrintTableKey(n.Key)}>";
            default:
                return n.Pointer;
        }
    }

    /// <summary>
    /// SPITBOL's prtnm for a block: the name of a natural variable whose value it is, found by walking the
    /// variable hash table (buckets in order, each in creation order) after first trying the variable
    /// found last time; failing that, the block's own value line.
    /// </summary>
    private string PrintBlockName(Var block)
    {
        if (_lastNamedVariable is not null && IdentifierTable.TryGetValue(_lastNamedVariable, out var last) && SameBlock(last, block))
            return _lastNamedVariable;

        var match = NaturalVariables()
                    .Select((kv, order) => (kv, order))
                    .Where(e => SameBlock(e.kv.Value, block))
                    .OrderBy(e => SpitbolHash(e.kv.Key) % 257)
                    .ThenBy(e => e.order)
                    .Select(e => e.kv.Key)
                    .FirstOrDefault();
        if (match is null)
            return PrintValue(block);
        _lastNamedVariable = match;
        return match;
    }

    private static bool SameBlock(Var value, Var block) => (value, block) switch
    {
        (ProgramDefinedDataVar a, ProgramDefinedDataVar b) => ReferenceEquals(a.FieldValues, b.FieldValues),
        _ => ReferenceEquals(value, block)
    };
}
