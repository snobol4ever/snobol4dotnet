namespace Snobol4.Common;

public partial class Executive
{
    // Lock objects for thread synchronization
    private readonly Lock _arrayCreationLock = new();
    private readonly Lock _indexCollectionLock = new();

    #region Factories

    /// <summary>
    /// Factory to create an array (Thread-Safe)
    /// </summary>
    /// <param name="arguments">List of arguments</param>
    public void CreateArray(List<Var> arguments)
    {
        lock (_arrayCreationLock)
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
    }

    #endregion

    #region Methods

    /// <summary>
    /// Convert list of indices on the stack to a single index (key)
    /// for a table or array (Thread-Safe)
    /// </summary>
    public void IndexCollection()
    {
        lock (_indexCollectionLock)
        {
            // Do not delete. Used by DLL
            if (Failure)
                return;
            if (Builder.TraceStatements)
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
    }

    private void IndexArray(ArrayVar arrayVar, List<Var> varIndices)
    {
        // Called from within lock, no additional locking needed
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

    #endregion
}