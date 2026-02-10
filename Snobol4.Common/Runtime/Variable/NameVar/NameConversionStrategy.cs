namespace Snobol4.Common;

public class NameConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var nameSelf = (NameVar)self;
        
        // Fast path optimization for common conversions
        switch (targetType)
        {
            case Executive.VarType.STRING:
                return ConvertToString(nameSelf, out varOut, out valueOut);

            case Executive.VarType.NAME:
                varOut = nameSelf;
                valueOut = nameSelf.Pointer;
                return true;

            case Executive.VarType.INTEGER:
                return ConvertToInteger(nameSelf, out varOut, out valueOut, exec);

            case Executive.VarType.REAL:
                return ConvertToReal(nameSelf, out varOut, out valueOut, exec);

            case Executive.VarType.PATTERN:
                varOut = new PatternVar(new LiteralPattern(nameSelf.Pointer));
                valueOut = ((PatternVar)varOut).Data;
                return true;

            case Executive.VarType.EXPRESSION:
                return ConvertToExpression(nameSelf, out varOut, out valueOut, exec);

            case Executive.VarType.CODE:
                return ConvertToCode(nameSelf, out varOut, out valueOut, exec);

            case Executive.VarType.ARRAY:
            case Executive.VarType.TABLE:
            default:
                varOut = StringVar.Null();
                valueOut = "";
                return false;
        }
    }


    private static bool ConvertToString(NameVar nameSelf, out Var varOut, out object valueOut)
    {
        if (nameSelf.Pointer.Length == 0)
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }

        varOut = new StringVar(nameSelf.Pointer)
        {
            Collection = nameSelf.Collection,
            Key = nameSelf.Key
        };
        valueOut = nameSelf.Pointer;
        return true;
    }


    private static bool ConvertToInteger(NameVar nameSelf, out Var varOut, out object valueOut, Executive exec)
    {
        var stringVar = new StringVar(nameSelf.Pointer);
        if (!stringVar.Convert(Executive.VarType.INTEGER, out varOut, out valueOut, exec))
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }
        return true;
    }


    private static bool ConvertToReal(NameVar nameSelf, out Var varOut, out object valueOut, Executive exec)
    {
        var stringVar = new StringVar(nameSelf.Pointer);
        if (!stringVar.Convert(Executive.VarType.REAL, out varOut, out valueOut, exec))
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }
        return true;
    }

    private static bool ConvertToExpression(NameVar nameSelf, out Var varOut, out object valueOut, Executive exec)
    {
        var argExpression = exec.IdentifierTable[nameSelf.Pointer];

        if (argExpression is not StringVar stringVarExpression || stringVarExpression.Symbol.Length == 0)
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }

        var previousCaseFolding = exec.Parent.CaseFolding;
        exec.Parent.CaseFolding = exec.AmpCaseFolding != 0;
        exec.Parent.CodeMode = true;
        exec.Parent.Code = new SourceCode(exec.Parent);
        exec.Parent.Code.ReadCodeInString($" A = *({stringVarExpression.Data.Trim()})", exec.Parent.FilesToCompile[^1]);
        exec.Parent.BuildEval();
        exec.Parent.CaseFolding = previousCaseFolding;
        exec.Parent.CodeMode = false;
        varOut = new ExpressionVar(exec.StarFunctionList[^1]);
        valueOut = ((ExpressionVar)varOut).FunctionName;
        return true;
    }

    private static bool ConvertToCode(NameVar nameSelf, out Var varOut, out object valueOut, Executive exec)
    {
        var argCode = exec.IdentifierTable[nameSelf.Pointer];

        if (argCode is not StringVar stringVarCode || stringVarCode.Symbol.Length == 0)
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }

        CodeVar code = new()
        {
            StatementNumber = exec.Statements.Count,
            Data = stringVarCode.Symbol
        };

        exec.Parent.Code = new SourceCode(exec.Parent);
        exec.Parent.Code.ReadCodeInString(code.Data, exec.Parent.FilesToCompile[^1]);

        if (!exec.Parent.BuildCode())
        {
            varOut = StringVar.Null();
            valueOut = "";
            return false;
        }

        varOut = code;
        valueOut = code.Data;
        return true;
    }


    public string GetDataType(Var self)
    {
        return "name";
    }


    public object GetTableKey(Var self)
    {
        var nameSelf = (NameVar)self;
        return nameSelf.Pointer;
    }
}