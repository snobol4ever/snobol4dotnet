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
        var strOut = realSelf.Data.ToString(CultureInfo.InvariantCulture);
        strOut = TweakRealString(strOut);
        varOut = new StringVar(strOut);
        valueOut = strOut;
        return true;
    }

    /// <summary>
    /// Ensures that the string form of a real value is always distinguishable
    /// from an integer by guaranteeing a decimal point or exponent marker is present.
    ///
    /// <para>
    /// .NET's <c>double.ToString()</c> drops the decimal point for whole-number
    /// values — e.g. <c>25.0</c> becomes <c>"25"</c> — which would make it
    /// indistinguishable from the integer 25.  When neither a <c>'.'</c> nor an
    /// <c>'E'</c> is already present we append a single trailing dot.
    /// </para>
    ///
    /// <para>
    /// The correct suffix is <c>"."</c> (trailing dot only), <b>not</b> <c>".0"</c>.
    /// This was verified against both reference implementations:
    /// <list type="bullet">
    ///   <item>
    ///     <b>CSNOBOL4</b> (<c>lib/realst.c</c>): formats with <c>%lg</c>, then
    ///     walks the result; if no <c>'.'</c> or exponent character is found it
    ///     appends a single <c>'.'</c>.  The source comment reads: <i>"%g format
    ///     can print an integer for exact values — make sure we have an exponent
    ///     or a dot."</i>
    ///   </item>
    ///   <item>
    ///     <b>SPITBOL</b> (<c>sbl.min</c>, label <c>gts27</c>): after suppressing
    ///     all trailing fractional zeros (label <c>gts24</c> loop), it unconditionally
    ///     emits a single decimal point character.  The MINIMAL spec comment states:
    ///     <i>"there are never any trailing zeros in the fractional part."</i>
    ///   </item>
    /// </list>
    /// Both oracles therefore produce <c>"25."</c> for the real value 25.0, and the
    /// LoadTests assertions have been updated to match.
    /// </para>
    /// </summary>
    public static string TweakRealString(string str)
    {
        return str.Contains('.') || str.Contains('E') ? str : str + ".";
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