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

            case Executive.VarType.NAME:
                if (stringSelf.Data.Length == 0)
                    return false;
                varOut = new NameVar(stringSelf.Data, stringSelf.Key, stringSelf.Collection);
                valueOut = stringSelf.Data;
                return true;

            case Executive.VarType.EXPRESSION:
                if (stringSelf.Data.Length == 0)
                    return false;

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
        var previousCaseFolding = exec.Parent.BuildOptions.CaseFolding;
        exec.Parent.BuildOptions.CaseFolding = exec.AmpCaseFolding != 0;
        exec.Parent.CodeMode = true;
        exec.Parent.Code = new SourceCode(exec.Parent);
        exec.Parent.Code.ReadCodeInString($" A_ = *({stringSelf.Data.Trim()})", exec.Parent.FilesToCompile[^1]);
        exec.Parent.BuildEval();
        exec.Parent.BuildOptions.CaseFolding = previousCaseFolding;
        exec.Parent.CodeMode = false;
        varOut = new ExpressionVar(exec.StarFunctionList[^1]);
        valueOut = ((ExpressionVar)varOut).FunctionName;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ConvertToCode(StringVar stringSelf, Executive exec, out Var varOut, out object valueOut)
    {
        CodeVar code = new()
        {
            StatementNumber = exec.Parent.StatementCount,
            Data = stringSelf.Data
        };

        exec.Parent.Code = new SourceCode(exec.Parent);
        exec.Parent.Code.ReadCodeInString(code.Data, exec.Parent.FilesToCompile[^1]);

        if (!exec.Parent.BuildCode())
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