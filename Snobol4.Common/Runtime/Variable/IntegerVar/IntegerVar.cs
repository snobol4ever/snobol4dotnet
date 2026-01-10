#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugString()}")]
public sealed class IntegerVar : Var
{
    #region Data

    public static readonly IntegerVar Zero = new(0);
    public static readonly IntegerVar One = new(1);
    public static readonly IntegerVar MinusOne = new(-1);

    // Integer pool for common small values to reduce allocations
    private static readonly IntegerVar[] _pool = InitializePool();
    private const int _poolMin = -128;
    private const int _poolMax = 127;

    public long Data;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly IntegerArithmeticStrategy _arithmeticStrategy = new();
    private static readonly IntegerComparisonStrategy _comparisonStrategy = new();
    private static readonly IntegerConversionStrategy _conversionStrategy = new();
    private static readonly IntegerCloningStrategy _cloningStrategy = new();
    private static readonly IntegerFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Integer Pool

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IntegerVar[] InitializePool()
    {
        var pool = new IntegerVar[_poolMax - _poolMin + 1];
        for (var i = 0; i < pool.Length; i++)
        {
            pool[i] = new IntegerVar(i + _poolMin);
        }
        return pool;
    }

    /// <summary>
    /// Creates an IntegerVar, using pooled instances for small values
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntegerVar Create(long value)
    {
        if ((ulong)(value - _poolMin) <= (ulong)(_poolMax - _poolMin))
        {
            return _pool[value - _poolMin];
        }
        return new IntegerVar(value);
    }

    #endregion

    #region Constructors

    public IntegerVar(long data)
    {
        Data = data;
    }

    public IntegerVar(
        long data = 0,
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

    internal IntegerVar(IntegerVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        Data = template.Data;
    }

    #endregion

    #region Double Dispatch Methods

    // These methods handle type-specific arithmetic with integers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        var leftData = left.Data;
        var rightData = Data;

        // Use checked arithmetic for overflow detection
        try
        {
            var result = checked(leftData + rightData);
            return Create(result);
        }
        catch (OverflowException)
        {
            executive.LogRuntimeException(3);
            return StringVar.Null();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data + Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        var leftData = left.Data;
        var rightData = Data;

        // Use checked arithmetic for overflow detection
        try
        {
            var result = checked(leftData - rightData);
            return Create(result);
        }
        catch (OverflowException)
        {
            executive.LogRuntimeException(34);
            return StringVar.Null();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data - Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        var leftData = left.Data;
        var rightData = Data;

        // Fast path for common cases using pattern matching
        switch (leftData, rightData)
        {
            case (0, _) or (_, 0):
                return Zero;
            case (1, _):
                return Create(rightData);
            case (_, 1):
                return Create(leftData);
            case (-1, _):
                return Create(-rightData);
            case (_, -1):
                return Create(-leftData);
        }

        // Use checked arithmetic for overflow detection
        try
        {
            var result = checked(leftData * rightData);
            return Create(result);
        }
        catch (OverflowException)
        {
            executive.LogRuntimeException(28);
            return StringVar.Null();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data * Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        var divisor = Data;

        if (divisor == 0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        var dividend = left.Data;

        // Check for overflow case: MinValue / -1 causes overflow
        if (dividend == long.MinValue && divisor == -1)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        return Create(dividend / divisor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        if (Data == 0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        return new RealVar(left.Data / Data);
    }

    #endregion
}