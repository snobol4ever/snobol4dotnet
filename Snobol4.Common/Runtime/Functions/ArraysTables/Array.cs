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
        if (!arguments[0].Convert(VarType.STRING, out _, out var prototypeString, this))
        {
            LogRuntimeException(64);
            return;
        }

        ArrayVar av = new();
        var result = av.ConfigurePrototype((string)prototypeString, arguments[1]);

        if (result == 0)
        {
            SystemStack.Push(av);
            return;
        }

        LogRuntimeException(result);
    }

                    public void IndexCollection()
    {
        // Do not delete. Used by DLL
        if (Failure)
            return;
        if (Parent.TraceStatements)
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
        var v = arrayVar.Data[(int)arrayKey];
        v.Key = arrayKey;
        v.Collection = arrayVar;
        SystemStack.Push(arrayVar.Data[(int)arrayKey]);
    }
}