using System.Globalization;

namespace Snobol4.Common;

public sealed class IntegerConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var intSelf = (IntegerVar)self;
        varOut = IntegerVar.Create(0);
        valueOut = 0;

        switch (targetType)
        {
            case Executive.VarType.STRING:
                {
                    var stringValue = intSelf.Data.ToString(CultureInfo.CurrentCulture);
                    varOut = new StringVar(stringValue);
                    valueOut = stringValue;
                    return true;
                }

            case Executive.VarType.INTEGER:
                varOut = intSelf;
                valueOut = intSelf.Data;
                return true;

            case Executive.VarType.REAL:
                {
                    double realValue = intSelf.Data;
                    varOut = new RealVar(realValue);
                    valueOut = realValue;
                    return true;
                }

            case Executive.VarType.PATTERN:
                {
                    var patternString = intSelf.Data.ToString(CultureInfo.CurrentCulture);
                    var pattern = new LiteralPattern(patternString);
                    varOut = new PatternVar(pattern);
                    valueOut = pattern;
                    return true;
                }

            case Executive.VarType.NAME:
                {
                    var nameString = intSelf.Data.ToString(CultureInfo.CurrentCulture);
                    if (nameString.Length == 0)
                    {
                        return false;
                    }
                    varOut = new NameVar(nameString, intSelf.Key, intSelf.Collection);
                    valueOut = intSelf.Data;
                    return true;
                }

            case Executive.VarType.EXPRESSION:
                {
                    var previousCaseFolding = exec.Parent.CaseFolding;
                    exec.Parent.CaseFolding = exec.AmpCaseFolding != 0;
                    exec.Parent.CodeMode = true;
                    exec.Parent.Code = new SourceCode(exec.Parent);
                    
                    var trimmedValue = intSelf.Data.ToString(CultureInfo.CurrentCulture).Trim();
                    exec.Parent.Code.ReadCodeInString($" A = *({trimmedValue})", exec.Parent.FilesToCompile[^1]);
                    exec.Parent.BuildEval();
                    
                    exec.Parent.CaseFolding = previousCaseFolding;
                    exec.Parent.CodeMode = false;
                    
                    varOut = new ExpressionVar(exec.StarFunctionList[^1]);
                    valueOut = ((ExpressionVar)varOut).FunctionName;
                    return true;
                }

            case Executive.VarType.ARRAY:
            case Executive.VarType.TABLE:
            case Executive.VarType.CODE:
            default:
                return false;
        }
    }


    public string GetDataType(Var self)
    {
        return "integer";
    }


    public object GetTableKey(Var self)
    {
        var intSelf = (IntegerVar)self;
        return intSelf.Data;
    }
}