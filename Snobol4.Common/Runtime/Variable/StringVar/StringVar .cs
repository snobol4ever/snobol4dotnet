#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugString()}")]
public sealed class StringVar : Var
{
    #region Data

    public string Data;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly StringArithmeticStrategy _arithmeticStrategy = new();
    private static readonly StringComparisonStrategy _comparisonStrategy = new();
    private static readonly StringConversionStrategy _conversionStrategy = new();
    private static readonly StringCloningStrategy _cloningStrategy = new();
    private static readonly StringFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringVar(bool succeeded)
    {
        InputChannel = string.Empty;
        OutputChannel = string.Empty;
        Symbol = string.Empty;
        Data = string.Empty;
        Succeeded = succeeded;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringVar Null(string symbol = "")
    {
        return new StringVar(true)
        {
            Symbol = symbol
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringVar(
        string data,
        string symbol = "",
        bool isKeyword = false,
        bool isReadOnly = false,
        string inputChannel = "",
        string outputChannel = "",
        ValidationDelegate? validation = null)
    {
        Data = data;
        Symbol = symbol;
        IsKeyword = isKeyword;
        IsReadOnly = isReadOnly;
        InputChannel = inputChannel;
        OutputChannel = outputChannel;
        Validation = validation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal StringVar(StringVar template)
    {
        Symbol = template.Symbol;
        Data = template.Data;
    }

    #endregion

    #region Double Dispatch Methods

    // Strings don't support arithmetic operations with other types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // Right operand of + is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // Right operand of + is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // Right operand of - is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // Right operand of - is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // Right operand of * is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // Right operand of * is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // Right operand of / is not numeric

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // Right operand of / is not numeric

    #endregion

    #region String-Specific Operations

    /// <summary>
    /// Concatenate this string with another (space operator in SNOBOL4)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Var Concatenate(Var other, Executive executive)
    {
        // Convert other to string if possible
        if (!other.Convert(Executive.VarType.STRING, out _, out var otherValue, executive))
        {
            executive.LogRuntimeException(8); // Concatenation requires string operands
            return StringVar.Null();
        }

        return new StringVar(Data + (string)otherValue);
    }

    #endregion
}