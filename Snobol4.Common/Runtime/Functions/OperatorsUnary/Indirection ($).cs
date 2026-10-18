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
                SystemStack.Push(ElementOrVariable(nameVar) ?? IdentifierTable[nameVar.Pointer]);
                return;

            default:
                LogRuntimeException(239);
                return;
        }
    }

    /// <summary>
    /// What a NAME stands for, read for use as a value or an assignment target: an element (.A[J],
    /// .T<'k'>, a DATA field) from the collection the NAME carries, marked as this statement's lvalue as an
    /// index read marks it, rather than looked up by the name of whichever variable last held the
    /// collection; a keyword (.&ANCHOR) through its handler; null for a natural variable.
    /// </summary>
    private Var? ElementOrVariable(NameVar nameVar)
    {
        if (nameVar.Pointer == "" && nameVar.Key is not null)
        {
            switch (nameVar.Collection)
            {
                case ArrayVar arrayVar:
                {
                    var key = (long)nameVar.Key;
                    var stored = arrayVar.Data[(int)key];
                    var v = stored is ArrayVar or TableVar or ProgramDefinedDataVar ? stored : stored.Clone();
                    v.Key = key;
                    v.Collection = arrayVar;
                    v.LvalueMark = StatementSerial;
                    return v;
                }

                case TableVar tableVar:
                {
                    var v = tableVar.Data.TryGetValue(nameVar.Key, out var stored)
                        ? stored is ArrayVar or TableVar ? stored : stored.Clone()
                        : tableVar.Fill.Clone();
                    v.Key = nameVar.Key;
                    v.Collection = tableVar;
                    v.LvalueMark = StatementSerial;
                    return v;
                }
            }
        }

        // The NAME of a keyword (K = .&ANCHOR) reaches the keyword itself, so $K reads and assigns
        // &ANCHOR, as in SPITBOL
        if (KeywordTable.TryGetValue(nameVar.Pointer, out var keywordHandler))
        {
            keywordHandler(nameVar, false);
            return SystemStack.Pop();
        }

        return null;
    }

    /// <summary>
    /// The result of a call that NRETURNs a NAME: the named variable's value, as SPITBOL gives it in a
    /// value context (N = TOP() copies the value, TOP() 'x' concatenates it), still carrying where it
    /// came from so the call can be an assignment target (TOP() = X) or the operand of the name operator.
    /// </summary>
    internal Var NameReturnValue(NameVar nameVar)
    {
        if (ElementOrVariable(nameVar) is { } element)
            return element;

        var stored = IdentifierTable[nameVar.Pointer];
        var value = stored is ArrayVar or TableVar or ProgramDefinedDataVar ? stored : stored.Clone();
        // the variable's name is its lvalue; no statement stamp, so a NAME it holds is not taken as a
        // target of its own (V = .Z; TOP() = X sets V)
        value.Symbol = nameVar.Pointer;
        return value;
    }
}
