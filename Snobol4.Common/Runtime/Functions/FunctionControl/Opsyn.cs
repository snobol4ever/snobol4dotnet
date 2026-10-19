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
                // If first argument is defined, it must not be a protected function.
                // Exception: INPUT and OUTPUT may be aliased via OPSYN (io.sno wraps them).
                var entry = FunctionTable[newFunction];
                var opsynAllowedRedefs = new HashSet<string> { "INPUT", "OUTPUT" };
                if (entry != null && entry.IsProtected && !opsynAllowedRedefs.Contains(newFunction))
                {
                    LogRuntimeException(248);
                    return;
                }

                FunctionTable.Remove(newFunction);
                UserFunctionTable.Remove(newFunction);
                var existingFunctionEntry = FunctionTable[existingFunction];
                if (existingFunctionEntry == null)
                    break;   // an undefined function's synonym is undefined too: a call is error 22
                FunctionTable.Add(newFunction, new FunctionTableEntry(this, newFunction, existingFunctionEntry.Handler, existingFunctionEntry.ArgumentCount, false));
                // If aliasing a program-defined function, copy its UserFunctionTableEntry
                // under the new name. The FunctionName field stays as the ORIGINAL name
                // so ExecuteProgramDefinedFunction resolves the return variable correctly:
                // e.g. opsyn(.facto,'fact') → facto body still writes to 'fact', not 'facto'.
                if (UserFunctionTable.TryGetValue(existingFunction, out var existingUserEntry))
                    UserFunctionTable[newFunction] = new UserFunctionTableEntry(
                        existingUserEntry.FunctionName,   // original name = return var name
                        existingUserEntry.Prototype,
                        existingUserEntry.Parameters, existingUserEntry.Locals,
                        existingUserEntry.EntryLabel);
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
                var unusedUnaryOperator = FunctionTable[existingFunction] ?? UndefinedOperator(newFunction, 1);
                FunctionTable.Add(newFunction, new FunctionTableEntry(this, newFunction, unusedUnaryOperator.Handler, unusedUnaryOperator.ArgumentCount, false));
                // a program-defined function needs its definition under the operator's key too, as in the binary case
                if (UserFunctionTable.TryGetValue(existingFunction, out var existingUnaryUserEntry))
                    UserFunctionTable[newFunction] = new UserFunctionTableEntry(
                        existingUnaryUserEntry.FunctionName,
                        existingUnaryUserEntry.Prototype,
                        existingUnaryUserEntry.Parameters, existingUnaryUserEntry.Locals,
                        existingUnaryUserEntry.EntryLabel);
                InvalidateOperatorHandler(newFunction);
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
                var unusedBinaryOperator = FunctionTable[existingFunction] ?? UndefinedOperator(newFunction, 2);
                FunctionTable.Add(newFunction, new FunctionTableEntry(this, newFunction, unusedBinaryOperator.Handler, unusedBinaryOperator.ArgumentCount, false));
                // Mirror case 0: copy UserFunctionTableEntry under the operator name so
                // that ExecuteProgramDefinedFunction can find the definition when the
                // OPSYN'd binary operator is dispatched via OperatorFast (which bypasses
                // the normal Function() call and looks up by operator name, not by the
                // original function name).
                if (UserFunctionTable.TryGetValue(existingFunction, out var existingBinaryUserEntry))
                    UserFunctionTable[newFunction] = new UserFunctionTableEntry(
                        existingBinaryUserEntry.FunctionName,
                        existingBinaryUserEntry.Prototype,
                        existingBinaryUserEntry.Parameters, existingBinaryUserEntry.Locals,
                        existingBinaryUserEntry.EntryLabel);
                InvalidateOperatorHandler(newFunction);
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

        // 0 is a function, 1 a unary operator and any larger count a binary one, as in SPITBOL (s_ops);
        // a negative count is error 153
        var n = (long)count;
        if (n < 0)
            return 153;
        operands = n > 2 ? 2 : (int)n;
        return 0;
    }

    private int CheckArgument2(Var arg, out string existingFunction, Executive exec)
    {
        existingFunction = "";

        // Second argument must be a string
        if (!arg.Convert(VarType.STRING, out _, out var function, exec))
            return 154;

        existingFunction = Parent.FoldCase((string)function);

        // Any non-null name will do, as in SPITBOL (gtnvr): a function that is not defined, or an
        // operator's symbol such as '+', which names no function, makes the synonym undefined
        return existingFunction == "" ? 154 : 0;
    }

    // An operator made a synonym of an undefined function: using it is error 22, as in SPITBOL
    private FunctionTableEntry UndefinedOperator(string name, int operands) =>
        new(this, name, _ => LogRuntimeException(22), operands, false);

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