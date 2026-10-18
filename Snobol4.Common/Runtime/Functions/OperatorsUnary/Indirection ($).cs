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
                // The NAME of an element (.A[J], .T<'k'>, a DATA field) carries its collection: read the
                // element there, marked as this statement's lvalue as an index read marks it, rather than
                // look the collection up by the name of whichever variable last held it
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
                            SystemStack.Push(v);
                            return;
                        }

                        case TableVar tableVar:
                        {
                            var v = tableVar.Data.TryGetValue(nameVar.Key, out var stored)
                                ? stored is ArrayVar or TableVar ? stored : stored.Clone()
                                : tableVar.Fill.Clone();
                            v.Key = nameVar.Key;
                            v.Collection = tableVar;
                            v.LvalueMark = StatementSerial;
                            SystemStack.Push(v);
                            return;
                        }
                    }
                }

                // The NAME of a keyword (K = .&ANCHOR) reaches the keyword itself, so $K reads and
                // assigns &ANCHOR, as in SPITBOL
                if (KeywordTable.TryGetValue(nameVar.Pointer, out var keywordHandler))
                {
                    keywordHandler(nameVar, false);
                    return;
                }

                SystemStack.Push(IdentifierTable[nameVar.Pointer]);
                return;

            default:
                LogRuntimeException(239);
                return;
        }
    }
}