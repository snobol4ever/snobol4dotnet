using System.Globalization;

namespace Snobol4.Common;

public sealed class RealConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        var realSelf = (RealVar)self;
        varOut = StringVar.Null();
        valueOut = string.Empty;

        return targetType switch
        {
            Executive.VarType.STRING => ConvertToString(realSelf, out varOut, out valueOut),
            Executive.VarType.INTEGER => ConvertToInteger(realSelf, out varOut, out valueOut),
            Executive.VarType.REAL => ConvertToReal(realSelf, out varOut, out valueOut),
            Executive.VarType.PATTERN => ConvertToPattern(realSelf, out varOut, out valueOut),
            Executive.VarType.NAME => ConvertToName(realSelf, out varOut, out valueOut),
            Executive.VarType.EXPRESSION => ConvertToExpression(realSelf, exec, out varOut, out valueOut),
            _ => false
        };
    }


    private static bool ConvertToString(RealVar realSelf, out Var varOut, out object valueOut)
    {
        var strOut = realSelf.Data.ToString(CultureInfo.CurrentCulture);
        strOut = TweakRealString(strOut);
        varOut = new StringVar(strOut);
        valueOut = strOut;
        return true;
    }

    public static string TweakRealString(string str)
    {
        // This is to ensure that the string representation of a real is distinguishable from an integer
        return str.Contains('.') || str.Contains('E') ? str : str + ".0";
    }

    private static bool ConvertToInteger(RealVar realSelf, out Var varOut, out object valueOut)
    {
        // Round towards zero (truncate)
        var rounded = Math.Round(realSelf.Data, MidpointRounding.ToZero);

        try
        {
            var intValue = checked((long)rounded);
            varOut = IntegerVar.Create(intValue);
            valueOut = intValue;
            return true;
        }
        catch (OverflowException)
        {
            varOut = StringVar.Null();
            valueOut = string.Empty;
            return false;
        }
    }


    private static bool ConvertToReal(RealVar realSelf, out Var varOut, out object valueOut)
    {
        varOut = realSelf;
        valueOut = realSelf.Data;
        return true;
    }


    private static bool ConvertToPattern(RealVar realSelf, out Var varOut, out object valueOut)
    {
        valueOut = realSelf.Data.ToString(CultureInfo.CurrentCulture);
        varOut = new PatternVar(new LiteralPattern((string)valueOut));
        return true;
    }

    private static bool ConvertToName(RealVar realSelf, out Var varOut, out object valueOut)
    {
        var stringData = realSelf.Data.ToString(CultureInfo.InvariantCulture);
        
        if (stringData.Length == 0)
        {
            varOut = StringVar.Null();
            valueOut = string.Empty;
            return false;
        }

        varOut = new NameVar(stringData, realSelf.Key, realSelf.Collection);
        valueOut = realSelf.Data;
        return true;
    }

    private static bool ConvertToExpression(RealVar realSelf, Executive exec, out Var varOut, out object valueOut)
    {
        var previousCaseFolding = exec.Parent.BuildOptions.CaseFolding;
        exec.Parent.BuildOptions.CaseFolding = exec.AmpCaseFolding != 0;
        exec.Parent.CodeMode = true;
        exec.Parent.Code = new SourceCode(exec.Parent);
        
        var realString = realSelf.Data.ToString(CultureInfo.InvariantCulture).Trim();
        exec.Parent.Code.ReadCodeInString($" A_ = *({realString})", exec.Parent.FilesToCompile[^1]);
        exec.Parent.BuildEval();
        
        exec.Parent.BuildOptions.CaseFolding = previousCaseFolding;
        exec.Parent.CodeMode = false;
        
        varOut = new ExpressionVar(exec.StarFunctionList[^1]);
        valueOut = ((ExpressionVar)varOut).FunctionName;
        return true;
    }


    public string GetDataType(Var self)
    {
        return "real";
    }


    public object GetTableKey(Var self)
    {
        var realSelf = (RealVar)self;
        return realSelf.Data;
    }
}