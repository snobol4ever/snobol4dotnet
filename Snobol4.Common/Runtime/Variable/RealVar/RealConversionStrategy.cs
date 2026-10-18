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
    /// A real's string form, computed as SPITBOL computes it (sbl.min gtstg, label gts10 on): scale the
    /// value into [0.1, 1) by steps of 10**10 and then the powers-of-ten table, add the rounding bias
    /// 0.5 * 10**-15, take the 15 digits as trunc(x * 10**15), and write them in fixed form from 0.1 up to
    /// below 1e15 (100., 0.333333333333333) or else as 0.ddd and a signed exponent (0.1e-2, 0.1e+16),
    /// trailing zeros dropped. The double arithmetic is SPITBOL's own, step for step, so the 15th digit
    /// is SPITBOL's too, not a correctly rounded one; matched against sbl -b on 3,000 computed values.
    /// </summary>
    public static string SpitbolRealString(double value)
    {
        if (value == 0)
            return "0.";

        var x = Math.Abs(value);
        var exponent = 0;
        while (x - 0.1 < 0) { x *= 1e10; exponent -= 10; }
        if (x - 1.0 >= 0)
        {
            while (x - 1e10 >= 0) { x /= 1e10; exponent += 10; }
            var k = 0;
            do { k++; exponent++; } while (x - _powersOfTen[k] >= 0);
            x /= _powersOfTen[k];
        }

        x += _roundingBias;
        if (x - 1.0 >= 0) { exponent++; x /= 10.0; }

        var digits = ((long)(x * _digitScale)).ToString(CultureInfo.InvariantCulture).PadLeft(15, '0');
        var sign = value < 0 ? "-" : "";
        if (exponent is < 0 or > 15)
        {
            var mantissa = digits.TrimEnd('0');
            return $"{sign}0.{(mantissa.Length == 0 ? "0" : mantissa)}e{(exponent >= 0 ? "+" : "-")}{Math.Abs(exponent)}";
        }

        return $"{sign}{(exponent > 0 ? digits[..exponent] : "0")}.{digits[exponent..].TrimEnd('0')}";
    }

    private static readonly double[] _powersOfTen = [1.0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10];

    // 10**15 by fifteen multiplications by 10.0 and 0.5 / 10**15, as SPITBOL's initialisation forms them
    private static readonly double _digitScale = Enumerable.Repeat(10.0, 15).Aggregate(1.0, (a, b) => a * b);
    private static readonly double _roundingBias = 0.5 / _digitScale;


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