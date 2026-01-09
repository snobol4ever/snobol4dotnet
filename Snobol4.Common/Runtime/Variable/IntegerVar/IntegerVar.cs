#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugString()}")]
public class IntegerVar : Var
{
    #region Data

    public static readonly IntegerVar Zero = new(0);
    
    // Integer pool for common small values to reduce allocations
    private static readonly IntegerVar[] _pool = InitializePool();
    private const int PoolMin = -128;
    private const int PoolMax = 127;

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
        var pool = new IntegerVar[PoolMax - PoolMin + 1];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = new IntegerVar((long)(i + PoolMin));
        }
        return pool;
    }

    /// <summary>
    /// Creates an IntegerVar, using pooled instances for small values
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntegerVar Create(long value)
    {
        if (value >= PoolMin && value <= PoolMax)
        {
            return _pool[value - PoolMin];
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
        long leftData = left.Data;
        long rightData = Data;
        long result = leftData + rightData;

        // Fast overflow check: if signs are same and result sign differs, overflow occurred
        if (((leftData ^ result) & (rightData ^ result)) < 0)
        {
            executive.LogRuntimeException(3);
            return StringVar.Null();
        }

        return Create(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data + Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        long leftData = left.Data;
        long rightData = Data;
        long result = leftData - rightData;

        // Fast overflow check: if signs differ and result sign differs from left, overflow occurred
        if (((leftData ^ rightData) & (leftData ^ result)) < 0)
        {
            executive.LogRuntimeException(34);
            return StringVar.Null();
        }

        return Create(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data - Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        long leftData = left.Data;
        long rightData = Data;

        // Fast path for common cases
        if (leftData == 0 || rightData == 0)
        {
            return Zero;
        }

        if (leftData == 1)
        {
            return Create(rightData);
        }

        if (rightData == 1)
        {
            return Create(leftData);
        }

        // Check for overflow before multiplication
        // This is faster than try-catch for the non-overflow case
        if (leftData > 0)
        {
            if (rightData > 0)
            {
                if (leftData > long.MaxValue / rightData)
                {
                    executive.LogRuntimeException(28);
                    return StringVar.Null();
                }
            }
            else
            {
                if (rightData < long.MinValue / leftData)
                {
                    executive.LogRuntimeException(28);
                    return StringVar.Null();
                }
            }
        }
        else
        {
            if (rightData > 0)
            {
                if (leftData < long.MinValue / rightData)
                {
                    executive.LogRuntimeException(28);
                    return StringVar.Null();
                }
            }
            else
            {
                if (leftData != 0 && rightData < long.MaxValue / leftData)
                {
                    executive.LogRuntimeException(28);
                    return StringVar.Null();
                }
            }
        }

        return Create(leftData * rightData);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        return new RealVar(left.Data * Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        if (Data == 0)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        // Check for overflow case: MinValue / -1 causes overflow
        if (left.Data == long.MinValue && Data == -1)
        {
            executive.LogRuntimeException(14);
            return StringVar.Null();
        }

        return Create(left.Data / Data);
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