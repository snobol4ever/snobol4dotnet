namespace Snobol4.Common;


// TODO: Determine error codes

public partial class Executive
{
    internal void Name(List<Var> arguments)
    {
        if (arguments[0].Symbol == "" && (arguments[0].Key == null || arguments[0].Collection == null))
        {
            LogRuntimeException(212);
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