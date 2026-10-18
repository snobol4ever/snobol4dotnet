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
        var strOut = SpitbolRealString(realSelf.Data);
        varOut = new StringVar(strOut);
        valueOut = strOut;
        return true;
    }

    /// <summary>
    /// A real's string form, as SPITBOL writes it: 15 significant digits with trailing zeros dropped
    /// and the decimal point always present (100., 0.333333333333333); fixed notation from 0.1 up to
    /// below 1e15, otherwise 0.ddd followed by e and the signed exponent (0.1e-2, 0.123456789012346e+17).
    /// Measured against sbl -b on 19 values at the edges of both forms.
    /// </summary>
    public static string SpitbolRealString(double value)
    {
        if (value == 0)
            return "0.";

        var sign = value < 0 ? "-" : "";
        var scientific = Math.Abs(value).ToString("E14", CultureInfo.InvariantCulture);   // d.ddddddddddddddE+xxx
        var exponentAt = scientific.IndexOf('E');
        var digits = scientific[..exponentAt].Replace(".", "").TrimEnd('0');
        if (digits.Length == 0)
            digits = "0";
        var exponent = int.Parse(scientific[(exponentAt + 1)..], CultureInfo.InvariantCulture) + 1;   // value = 0.digits x 10^exponent

        if (exponent is < 0 or > 15)
            return $"{sign}0.{digits}e{(exponent >= 0 ? "+" : "-")}{Math.Abs(exponent)}";

        if (exponent == 0)
            return $"{sign}0.{digits}";

        var integerPart = digits.Length >= exponent ? digits[..exponent] : digits.PadRight(exponent, '0');
        var fractionPart = digits.Length > exponent ? digits[exponent..] : "";
        return $"{sign}{integerPart}.{fractionPart}";
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
        valueOut = realSelf.Data.ToString(CultureInfo.InvariantCulture);
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
        return "REAL";
    }


    public object GetTableKey(Var self)
    {
        var realSelf = (RealVar)self;
        return realSelf.Data;
    }
}