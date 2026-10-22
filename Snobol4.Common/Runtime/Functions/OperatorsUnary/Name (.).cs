namespace Snobol4.Common;


//"function called by name returned a value" /* 21 */,
//"syntax error: value used where name is required" /* 212 */,

public partial class Executive
{
    internal void Name(List<Var> arguments)
    {
        // A call in the place of a name that handed back a value, a builtin's (.SIZE('abc')) or a RETURN's (.G()), is error 21,
        // as in SPITBOL; every other value that is no name (.'abc', .(X Y)) is error 212, found as its statement is compiled
        if (ReferenceEquals(arguments[0], ValueReturned) || arguments[0].Symbol == "" && (arguments[0].Key == null || arguments[0].Collection == null))
        {
            LogRuntimeException(21);
            return;
        }

        // An element this statement read (.A[K], a DATA field) names the element, even when its value
        // still carries the name of the variable it was copied from (SWAP = ...; $ARG = SWAP left
        // Symbol SWAP on the element, and .A[K] named SWAP); anything else names its variable
        var operand = arguments[0];
        var isElement = operand.Key != null && operand.Collection != null && operand.LvalueMark == StatementSerial;
        var nameVar = new NameVar(isElement ? "" : operand.Symbol, operand.Key, operand.Collection);
        SystemStack.Push(nameVar);
    }
}