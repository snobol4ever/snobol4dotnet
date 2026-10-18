using System.Globalization;

namespace Snobol4.Common;

public sealed class IntegerConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var intSelf = (IntegerVar)self;

        // Fast path: INTEGER→INTEGER is identity — no allocation needed.
        if (targetType == Executive.VarType.INTEGER)
        {
            varOut = intSelf;
            valueOut = intSelf.Data;
            return true;
        }

        // Defaults for non-fast-path branches (only reached when actually converting).
        varOut = IntegerVar.Create(0);
        valueOut = 0;

        switch (targetType)
        {
            case Executive.VarType.STRING:
                {
                    // InvariantCulture: SPITBOL integers are locale-independent.
                    var stringValue = intSelf.Data.ToString(CultureInfo.InvariantCulture);
                    varOut = new StringVar(stringValue);
                    valueOut = stringValue;
                    return true;
                }

            case Executive.VarType.INTEGER:
                // Already handled above; unreachable.
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
                    var patternString = intSelf.Data.ToString(CultureInfo.InvariantCulture);
                    var pattern = new LiteralPattern(patternString);
                    varOut = new PatternVar(pattern);
                    valueOut = pattern;
                    return true;
                }

            case Executive.VarType.NAME:
                {
                    var nameString = intSelf.Data.ToString(CultureInfo.InvariantCulture);
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
                    var previousCaseFolding = exec.Parent.BuildOptions.CaseFolding;
                    exec.Parent.BuildOptions.CaseFolding = exec.AmpCaseFolding != 0;
                    exec.Parent.CodeMode = true;
                    exec.Parent.Code = new SourceCode(exec.Parent);
                    var trimmedValue = intSelf.Data.ToString(CultureInfo.CurrentCulture).Trim();
                    exec.Parent.Code.ReadCodeInString($" A_ = *({trimmedValue})", exec.Parent.FilesToCompile[^1]);
                    exec.Parent.BuildEval();
                    exec.Parent.BuildOptions.CaseFolding = previousCaseFolding;
                    varOut = new ExpressionVar(exec.StarFunctionList[^1]);
                    valueOut = ((ExpressionVar)varOut).FunctionName;
                    exec.Parent.CodeMode = false;
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
        return "INTEGER";
    }


    public object GetTableKey(Var self)
    {
        var intSelf = (IntegerVar)self;
        return intSelf.Data;
    }
}