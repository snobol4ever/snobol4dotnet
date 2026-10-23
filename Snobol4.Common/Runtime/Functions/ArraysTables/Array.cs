namespace Snobol4.Common;

public partial class Executive
{
    //"array first argument is not integer or string" /* 64 */,
    //"array first argument lower bound is not integer" /* 65 */,
    //"array first argument upper bound is not integer" /* 66 */,
    //"array dimension is zero, negative or out of range" /* 67 */,
    //"array size exceeds maximum permitted" /* 68 */,

                    public void CreateArray(List<Var> arguments)
    {
        // SPITBOL takes the first argument as an integer before it takes it as a prototype, so ARRAY(3), ARRAY(3.5), ARRAY('3')
        // and ARRAY(' 3') are all the vector of three
        string prototype;
        if (arguments[0].Convert(VarType.INTEGER, out _, out var dimension, this))
            prototype = ((long)dimension).ToString(System.Globalization.CultureInfo.InvariantCulture);
        else if (arguments[0].Convert(VarType.STRING, out _, out var prototypeString, this))
            prototype = (string)prototypeString;
        else
        {
            LogRuntimeException(64);
            return;
        }

        ArrayVar av = new();
        var result = av.ConfigurePrototype(prototype, arguments[1], MaxObjectLength);

        if (result == 0)
        {
            av.BlockId = NewBlockId();
            SystemStack.Push(av);
            return;
        }

        LogRuntimeException(result);
    }

    // The subscripts of an array or table reference: `count` of them, and the object they subscript, are on the stack. The count is the
    // lexer's (commas + 1), so a subscripted value that is neither an array nor a table, or an index that is itself one, is known for
    // what it is.
    public void IndexCollection(int count)
    {
        if (count <= 0)
        {
            IndexCollection();
            return;
        }

        // After a failure the element is not taken, but its subscripts and the collection still leave the stack and one failure value
        // takes their place, as a call's arguments do; left there, a choice or a negation after it read them for its own operand
        // ((NE(1, 1) U['v'], 'alt') was null, ~(NE(1, 1) U['v']) failed)
        if (Failure)
        {
            for (var i = 0; i <= count; ++i)
                SystemStack.Pop();
            PushFailedValue();
            return;
        }

        if (Parent.BuildOptions.TraceStatements)
            Console.Error.WriteLine(@"IndexCollection");

        List<Var> varIndices = new(count);
        for (var i = 0; i < count; ++i)
            varIndices.Add(SystemStack.Pop());

        Subscript(SystemStack.Pop(), varIndices);
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
            PushFailedValue();
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

        Subscript(SystemStack.Pop(), varIndices);
    }

    private void Subscript(Var collection, List<Var> varIndices)
    {
        // An input-associated subscript reads a line, and at end of input the statement fails, as in
        // SPITBOL (T[W] = 0 :S(L) loops until W's file ends)
        if (InputArguments(varIndices))
            return;

        switch (collection)
        {
            case ArrayVar arrayVar:
                IndexArray(arrayVar, varIndices);
                break;

            case TableVar tableVar:
                IndexTable(tableVar, varIndices);
                break;

            default:
                LogRuntimeException(235);
                break;
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
        var v = stored is ArrayVar or TableVar ? Handle(stored) : stored.Clone();
        v.Key        = arrayKey;
        v.Collection = arrayVar;
        v.LvalueMark = StatementSerial;
        SystemStack.Push(v);
    }
}