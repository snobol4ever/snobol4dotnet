namespace Snobol4.Common;

public partial class Executive
{
    //"array first argument is not integer or string" /* 64 */,
    //"array first argument lower bound is not integer" /* 65 */,
    //"array first argument upper bound is not integer" /* 66 */,
    //"array dimension is zero negative or out of range" /* 67 */,
    //"array size exceeds maximum permitted" /* 68 UNUSED*/,

                    public void CreateArray(List<Var> arguments)
    {
        // A real bound is an integer one, truncated: ARRAY(3.5) has three elements, as in SPITBOL
        var prototype = arguments[0] is RealVar && arguments[0].Convert(VarType.INTEGER, out var bound, out _, this)
            ? bound
            : arguments[0];
        if (!prototype.Convert(VarType.STRING, out _, out var prototypeString, this))
        {
            LogRuntimeException(64);
            return;
        }

        ArrayVar av = new();
        var result = av.ConfigurePrototype((string)prototypeString, arguments[1]);

        if (result == 0)
        {
            av.BlockId = NewBlockId();
            SystemStack.Push(av);
            return;
        }

        LogRuntimeException(result);
    }

                    public void IndexCollection()
    {
        // Do not delete. Used by DLL
        // When already in failed state, we must push a failure sentinel before
        // returning so that the stack stays balanced.  Without the sentinel,
        // the operands that were pushed for this subscript expression (the
        // collection var plus each index) remain on the stack and are later
        // mis-consumed by BinaryEquals as if they were a legitimate lvalue/rvalue
        // pair, producing a spurious VALUE monitor event and a wrong assignment.
        // Mirrors the contract of NonExceptionFailure(): Failure=true + sentinel pushed.
        if (Failure)
        {
            SystemStack.Push(new StringVar(false) { Succeeded = false });
            return;
        }
        if (Parent.BuildOptions.TraceStatements)
            Console.Error.WriteLine(@"IndexCollection");

        List<Var> varIndices = [];

        while (SystemStack.Peek() is not ArrayVar && SystemStack.Peek() is not TableVar)
        {
            if (SystemStack.Peek() is StatementSeparator)
            {
                LogRuntimeException(235);
                return;
            }

            varIndices.Add(SystemStack.Pop());
        }

        switch (SystemStack.Pop())
        {
            case ArrayVar arrayVar:
                IndexArray(arrayVar, varIndices);
                break;

            case TableVar tableVar:
                IndexTable(tableVar, varIndices);
                break;

            default:
                throw new ApplicationException("IndexCollection()");
        }
    }

    private void IndexArray(ArrayVar arrayVar, List<Var> varIndices)
    {
        if (arrayVar.Dimensions != varIndices.Count)
        {
            LogRuntimeException(236);
            return;
        }

        List<long> indices = [];

        foreach (var vIndex in varIndices)
        {
            if (!vIndex.Convert(VarType.INTEGER, out _, out var value, this))
            {
                LogRuntimeException(238);
                return;
            }

            indices.Add((long)value);
        }

        for (var i = 0; i < arrayVar.Dimensions; ++i)
        {
            var index = indices[i];

            if (index >= arrayVar.LowerBounds[i] && index <= arrayVar.UpperBounds[i])
                continue;

            NonExceptionFailure();
            return;
        }

        var arrayKey = arrayVar.Index(indices);
        // Aggregate types (Array/Table) keep reference semantics so that
        // chained-subscript writes — e.g. m<i><j> = 1 — modify the real
        // inner aggregate, not a transient clone.  Scalars are cloned so
        // that the object stored in Data[] is never aliased to what's
        // on the stack or in a scalar variable slot.
        var stored = arrayVar.Data[(int)arrayKey];
        var v = stored is ArrayVar or TableVar ? stored : stored.Clone();
        v.Key        = arrayKey;
        v.Collection = arrayVar;
        v.LvalueMark = StatementSerial;
        SystemStack.Push(v);
    }
}