using System.Globalization;

namespace Snobol4.Common;

//"indirection operand is not name" /* 239 */,

public partial class Executive
{
    internal void Indirection(List<Var> arguments)
    {
        switch (arguments[0])
        {
            case RealVar:
                var symbol1 = Parent.FoldCase(((RealVar)arguments[0]).Data.ToString(CultureInfo.InvariantCulture));
                SystemStack.Push(IdentifierTable[symbol1]);
                return;

            case IntegerVar:
                var symbol2 = Parent.FoldCase(((IntegerVar)arguments[0]).Data.ToString(CultureInfo.InvariantCulture));
                SystemStack.Push(IdentifierTable[symbol2]);
                return;

            case StringVar:
                var symbol3 = Parent.FoldCase(((StringVar)arguments[0]).Data);

                if (symbol3 == "")
                {
                    LogRuntimeException(239);
                    return;
                }

                SystemStack.Push(IdentifierTable[symbol3]);
                return;

            case NameVar nameVar:
                if (nameVar.Pointer == "")
                {
                    var v = IdentifierTable[nameVar.Collection.Symbol];


                    switch (v)
                    {
                        case ArrayVar arrayVar:
                            SystemStack.Push(arrayVar.Data[(int)(long)nameVar.Key]);
                            return;

                        case TableVar tableVar:
                            SystemStack.Push(tableVar.Data[nameVar.Key]);
                            return;

                    }
                }
                SystemStack.Push(IdentifierTable[nameVar.Pointer]);
                return;

            default:
                LogRuntimeException(239);
                return;
        }
    }
}