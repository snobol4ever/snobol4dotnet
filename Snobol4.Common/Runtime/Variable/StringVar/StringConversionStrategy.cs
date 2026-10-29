using System.Runtime.CompilerServices;

namespace Snobol4.Common;

public class StringConversionStrategy : IConversionStrategy
{

    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var stringSelf = (StringVar)self;
        varOut = StringVar.Null();
        valueOut = string.Empty;

        switch (targetType)
        {
            case Executive.VarType.STRING:
                varOut = stringSelf;
                valueOut = stringSelf.Data;
                return true;

            case Executive.VarType.INTEGER:
                // A string that reads as a real converts too, truncated, as SPITBOL converts
                // CHAR('65.0') and CONVERT('6.5E1', 'INTEGER'); the INTEGER predicate stays strict
                if (!Var.ToInteger(stringSelf.Data, out var intValue))
                {
                    if (!Var.ToReal(stringSelf.Data, out var asReal))
                        return false;
                    return new RealVar(asReal).Convert(Executive.VarType.INTEGER, out varOut, out valueOut, exec);
                }
                varOut = new IntegerVar(intValue);
                valueOut = intValue;
                return true;

            case Executive.VarType.REAL:
                if (!Var.ToReal(stringSelf.Data, out var realValue))
                    return false;
                varOut = new RealVar(realValue);
                valueOut = realValue;
                return true;

            case Executive.VarType.PATTERN:
                varOut = new PatternVar(new LiteralPattern(stringSelf.Data));
                valueOut = ((PatternVar)varOut).Data;
                return true;

            // while &CASE folds, the name's text is folded as every name is, as in SPITBOL: CONVERT('abc', 'NAME') is .ABC
            case Executive.VarType.NAME:
                if (stringSelf.Data.Length == 0)
                    return false;
                varOut = new NameVar(exec.AmpCaseFolding != 0 ? stringSelf.Data.ToUpperInvariant() : stringSelf.Data, stringSelf.Key,
                    stringSelf.Collection);
                valueOut = stringSelf.Data;
                return true;

            // the null string is the expression whose value is the null string, as SPITBOL converts it
            case Executive.VarType.EXPRESSION:
                if (stringSelf.Data.Length == 0)
                {
                    varOut = new ExpressionVar(x => x.SystemStack.Push(StringVar.Null()));
                    valueOut = ((ExpressionVar)varOut).FunctionName;
                    return true;
                }

                return ConvertToExpression(stringSelf, exec, out varOut, out valueOut);

            case Executive.VarType.CODE:
                return ConvertToCode(stringSelf, exec, out varOut, out valueOut);

            case Executive.VarType.ARRAY:
            case Executive.VarType.TABLE:
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ConvertToExpression(StringVar stringSelf, Executive exec, out Var varOut, out object valueOut)
    {
        // one expression, as EVAL takes it: a comma outside every bracket and quote is error 223, a compile error of the conversion
        // (it compiled *(1,000), a selection)
        if (Executive.HasTopLevelComma(stringSelf.Data))
        {
            exec.AmpErrorType = 223;
            exec.AmpErrorText = CompilerException.ErrorMessage[223];
            exec.CompileErrorSerial++;
            varOut = StringVar.Null();
            valueOut = string.Empty;
            return false;
        }

        // a colon or a semicolon outside every bracket and quote ends the expression, and the conversion fails leaving &ERRTYPE as it
        // was (CONVERT('4 :(L)', 'EXPRESSION'), CONVERT('0:35', 'EXPRESSION')); but where the mark is not the text's last character, a
        // syntax error in the expression before it is recorded as any compile error is (CONVERT('4 + ; ', 'EXPRESSION') is 221), as in
        // SPITBOL, which scans that far
        var mark = Executive.TopLevelStatementMark(stringSelf.Data);
        if (mark >= 0 && stringSelf.Data[^1] is ':' or ';')
        {
            varOut = StringVar.Null();
            valueOut = string.Empty;
            return false;
        }

        var text = mark >= 0 ? stringSelf.Data[..mark] : stringSelf.Data;
        var previousCaseFolding = exec.Parent.BuildOptions.CaseFolding;
        exec.Parent.BuildOptions.CaseFolding = exec.AmpCaseFolding != 0;
        exec.Parent.CodeMode = true;
        exec.Parent.Code = new SourceCode(exec.Parent);
        exec.Parent.Code.ReadCodeInString($" A_ = *({text.Trim()})", exec.Parent.FilesToCompile[^1], oneLine: true);
        // text that does not compile fails the conversion with &ERRTYPE set, printing nothing, as in SPITBOL
        exec.Parent.SyntaxOnly = mark >= 0;
        bool compiled;
        try { compiled = exec.CompileQuietly(() => { exec.Parent.BuildEval(); return true; }); }
        finally { exec.Parent.SyntaxOnly = false; }
        exec.Parent.BuildOptions.CaseFolding = previousCaseFolding;
        exec.Parent.CodeMode = false;
        if (!compiled || mark >= 0)
        {
            varOut = StringVar.Null();
            valueOut = string.Empty;
            return false;
        }

        varOut = new ExpressionVar(exec.StarFunctionList[^1]);
        valueOut = ((ExpressionVar)varOut).FunctionName;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ConvertToCode(StringVar stringSelf, Executive exec, out Var varOut, out object valueOut)
    {
        var code = exec.CompileCodeText(stringSelf.Data);
        if (code is null)
        {
            varOut = StringVar.Null();
            valueOut = string.Empty;
            return false;
        }

        varOut = code;
        valueOut = code.Data;
        return true;
    }


    public string GetDataType(Var self)
    {
        return "STRING";
    }


    public object GetTableKey(Var self)
    {
        var stringSelf = (StringVar)self;
        return stringSelf.Data;
    }
}