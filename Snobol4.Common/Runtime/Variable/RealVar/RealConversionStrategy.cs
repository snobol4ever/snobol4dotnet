using System.Globalization;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Conversion strategy for real (floating-point) variables
/// </summary>
public sealed class RealConversionStrategy : IConversionStrategy
{
    public bool TryConvert(Var self, Executive.VarType targetType, out Var varOut, out object valueOut, Executive exec)
    {
        RealVar realSelf = (RealVar)self;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ConvertToString(RealVar realSelf, out Var varOut, out object valueOut)
    {
        valueOut = realSelf.Data.ToString(CultureInfo.CurrentCulture);
        varOut = new StringVar((string)valueOut);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ConvertToInteger(RealVar realSelf, out Var varOut, out object valueOut)
    {
        // Round towards zero (truncate)
        double rounded = Math.Round(realSelf.Data, MidpointRounding.ToZero);

        try
        {
            long intValue = checked((long)rounded);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ConvertToReal(RealVar realSelf, out Var varOut, out object valueOut)
    {
        varOut = realSelf;
        valueOut = realSelf.Data;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ConvertToPattern(RealVar realSelf, out Var varOut, out object valueOut)
    {
        valueOut = realSelf.Data.ToString(CultureInfo.CurrentCulture);
        varOut = new PatternVar(new LiteralPattern((string)valueOut));
        return true;
    }

    private static bool ConvertToName(RealVar realSelf, out Var varOut, out object valueOut)
    {
        string stringData = realSelf.Data.ToString(CultureInfo.InvariantCulture);
        
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
        bool previousCaseFolding = exec.Parent.CaseFolding;
        exec.Parent.CaseFolding = ((IntegerVar)exec.IdentifierTable["&case"]).Data != 0;
        exec.Parent.CodeMode = true;
        exec.Parent.Code = new SourceCode(exec.Parent);
        
        string realString = realSelf.Data.ToString(CultureInfo.InvariantCulture).Trim();
        exec.Parent.Code.ReadCodeInString($" A = *({realString})", exec.Parent.FilesToCompile[^1]);
        exec.Parent.BuildEval();
        
        exec.Parent.CaseFolding = previousCaseFolding;
        exec.Parent.CodeMode = false;
        
        varOut = new ExpressionVar(exec.StarFunctionList[^1]);
        valueOut = ((ExpressionVar)varOut).FunctionName;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDataType(Var self)
    {
        return "real";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetTableKey(Var self)
    {
        RealVar realSelf = (RealVar)self;
        return realSelf.Data;
    }
}