namespace Snobol4.Common;

//"replace third argument is not a string" /* 168 */,
//"replace second argument is not a string" /* 169 */,
//"replace first argument is not a string" /* 170 */,
//"null or unequally long 2nd, 3rd args to replace" /* 171 */,

public partial class Executive
{
    public void Replace(List<Var> arguments)
    {
        // SPITBOL takes the third argument off the stack first, then the second, builds its table from the two (error 171),
        // and only then looks at the first
        if (!arguments[2].Convert(VarType.STRING, out _, out var to, this))
        {
            LogRuntimeException(168);
            return;
        }

        if (!arguments[1].Convert(VarType.STRING, out _, out var from, this))
        {
            LogRuntimeException(169);
            return;
        }

        var fromStr = (string)from;
        var toStr = (string)to;

        if (fromStr == "" || toStr == "" || fromStr.Length != toStr.Length)
        {
            LogRuntimeException(171);
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var target, this))
        {
            LogRuntimeException(170);
            return;
        }

        // a character that is in the second argument more than once is replaced by the last character paired with it
        var map = new Dictionary<char, char>();
        for (var j = 0; j < fromStr.Length; ++j)
            map[fromStr[j]] = toStr[j];

        var result = ((string)target).ToCharArray();
        for (var i = 0; i < result.Length; ++i)
            if (map.TryGetValue(result[i], out var replacement))
                result[i] = replacement;

        SystemStack.Push(new StringVar(new string(result)));
    }
}
