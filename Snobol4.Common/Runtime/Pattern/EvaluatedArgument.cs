namespace Snobol4.Common;

/// <summary>
/// The argument of a pattern function written as *X, evaluated when the match reaches it, as SPITBOL evaluates it: an expression that
/// fails fails the match with no error; a variable holding an expression is evaluated again; a value that does not suit is the error
/// the function has for it (ANY 43, BREAK 44, BREAKX 45, NOTANY 49, SPAN 56 for a string that is null or no string; LEN 47 and 48,
/// POS 50 and 51, RPOS 52 and 53, RTAB 54 and 55, TAB 57 and 58 for an integer that is none, or negative or too large).
/// </summary>
internal static class EvaluatedArgument
{
    // The value the expression gives, or null when it failed (the failure is the match's, not the statement's)
    private static Var? Evaluate(Scanner scan, Executive.DeferredCode expression)
    {
        var exec = scan.Exec;
        for (var depth = 0; ; depth++)
        {
            expression(exec);
            var failed = exec.Failure;
            var result = exec.SystemStack.Pop();
            if (failed || !result.Succeeded)
            {
                exec.Failure = false;
                return null;
            }

            if (result is ExpressionVar nested && depth < 100)
            {
                expression = nested.FunctionName;
                continue;
            }

            return result;
        }
    }

    internal static bool Integer(Scanner scan, Executive.DeferredCode expression, int notInteger, int outOfRange, out long n)
    {
        n = 0;
        var value = Evaluate(scan, expression);
        if (value is null)
            return false;
        if (!value.Convert(Executive.VarType.INTEGER, out _, out var converted, scan.Exec))
        {
            scan.Exec.LogRuntimeException(notInteger);
            return false;
        }

        n = (long)converted;
        if (n >= 0 && n <= scan.Exec.MaxObjectLength)
            return true;
        scan.Exec.LogRuntimeException(outOfRange);
        return false;
    }

    internal static bool String(Scanner scan, Executive.DeferredCode expression, int error, out string text)
    {
        text = "";
        var value = Evaluate(scan, expression);
        if (value is null)
            return false;
        if (!value.Convert(Executive.VarType.STRING, out _, out var converted, scan.Exec) || string.IsNullOrEmpty((string)converted))
        {
            scan.Exec.LogRuntimeException(error);
            return false;
        }

        text = (string)converted;
        return true;
    }
}
