using System.ComponentModel;

namespace Snobol4.Common;

//"opsyn third argument is not integer" /* 152 */,
//"opsyn third argument is negative or too large" /* 153 */,
//"opsyn second arg is not natural variable name" /* 154 */,
//"opsyn first arg is not natural variable name" /* 155 */,
//"opsyn first arg is not correct operator name" /* 156 */,

public partial class Executive
{

    internal static readonly List<string> UnusedUnary =
    [
        "!",
        "%",
        "/",
        "#",
        "=",
        "|"
    ];

    internal static readonly List<string> UnusedBinary =
    [
        "&",
        "@",
        "#",
        "%",
        "~"
    ];

    internal void OperatorSynonym(List<Var> arguments)
    {
        var errorCode = CheckArgument3(arguments[2], out var operands, this);

        if (errorCode != 0)
        {
            LogRuntimeException(errorCode);
            return;
        }

        errorCode = CheckArgument2(arguments[1], out var existingFunction, this);

        if (errorCode != 0)
        {
            LogRuntimeException(errorCode);
            return;
        }

        errorCode = CheckArgument1(arguments[0], out var newFunction, this);

        if (errorCode != 0)
        {
            LogRuntimeException(errorCode);
            return;
        }

        switch (operands)
        {
            case 0:
                // If first argument is defined, it must not be a protected function
                var entry = FunctionTable[newFunction];
                //if (FunctionTable.ContainsKey(newFunction) && FunctionTable[newFunction].IsProtected)
                if(entry!=null && entry.IsProtected)
                {
                    LogRuntimeException(155);
                    return;
                }

                // First argument cannot be an operator
                if (UnusedUnary.Contains(newFunction) || UnusedBinary.Contains(newFunction))
                {
                    LogRuntimeException(155);
                    return;
                }

                FunctionTable.Remove(newFunction);
                var existingFunctionEntry = FunctionTable[existingFunction];
                FunctionTable.Add(newFunction, new FunctionTableEntry(this, newFunction, existingFunctionEntry.Handler, existingFunctionEntry.ArgumentCount, false));
                break;

            case 1:
                // First argument must be an unused unary operator
                if (!UnusedUnary.Contains(newFunction))
                {
                    LogRuntimeException(156);
                    return;
                }

                newFunction = "_" + newFunction;
                FunctionTable.Remove(newFunction);
                var unusedUnaryOperator = FunctionTable[existingFunction];
                FunctionTable.Add(newFunction, new FunctionTableEntry(this, newFunction, unusedUnaryOperator.Handler, unusedUnaryOperator.ArgumentCount, false));
                break;

            case 2:
                // First argument must be an unused binary operator
                if (!UnusedBinary.Contains(newFunction))
                {
                    LogRuntimeException(156);
                    return;
                }

                newFunction = "__" + newFunction;
                FunctionTable.Remove(newFunction);
                var unusedBinaryOperator = FunctionTable[existingFunction];
                FunctionTable.Add(newFunction, new FunctionTableEntry(this, newFunction, unusedBinaryOperator.Handler, unusedBinaryOperator.ArgumentCount, false));
                break;
        }

        SystemStack.Push(StringVar.Null());
    }

    private static int CheckArgument3(Var arg, out int operands, Executive exec)
    {
        operands = -1;

        // Third argument must be numeric
        if (!arg.Convert(VarType.INTEGER, out _, out var count, exec))
            return 152;

        // Third argument must be 0, 1 or 2
        operands = (int)(long)count;
        return operands is > 2 or < 0 ? 153 : 0;
    }

    private int CheckArgument2(Var arg, out string existingFunction, Executive exec)
    {
        existingFunction = "";

        // Second argument must be a string
        if (!arg.Convert(VarType.STRING, out _, out var function, exec))
            return 154;

        existingFunction = (string)function;

        // Second argument must be defined function
        return !FunctionTable.ContainsKey(existingFunction) ? 154 : 0;
    }

    private static int CheckArgument1(Var arg, out string newFunction, Executive exec)
    {
        newFunction = "";

        // First argument must be a string
        if (!arg.Convert(VarType.STRING, out _, out var synonym, exec))
            return 155;

        newFunction = (string)synonym;
        return 0;
    }
}