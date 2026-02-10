#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class TableVar : Var
{
    #region Data

    internal Dictionary<object, Var> Data;
    internal readonly Var Fill;

    #endregion

    #region Properties

                public int Count => Data.Count;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly TableArithmeticStrategy _arithmeticStrategy = new();
    private static readonly TableComparisonStrategy _comparisonStrategy = new();
    private static readonly TableConversionStrategy _conversionStrategy = new();
    private static readonly TableCloningStrategy _cloningStrategy = new();
    private static readonly TableFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

                        internal TableVar(Var fill)
    {
        ArgumentNullException.ThrowIfNull(fill);
        Data = [];
        Fill = fill;
    }

                            internal TableVar(Var fill, int capacity)
    {
        ArgumentNullException.ThrowIfNull(fill);
        Data = new Dictionary<object, Var>(capacity);
        Fill = fill;
    }

    #endregion

    #region Table-Specific Methods

                        
    internal Var GetOrDefault(object key)
    {
        ArgumentNullException.ThrowIfNull(key);

        // Use CollectionsMarshal.GetValueRefOrNullRef for faster lookups in .NET 9
        ref var value = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(Data, key);
        if (!System.Runtime.CompilerServices.Unsafe.IsNullRef(ref value))
        {
            return value;
        }

        // Return a clone to prevent shared reference issues
        var fillClone = Fill.Clone();
        fillClone.Key = key;
        fillClone.Collection = this;
        return fillClone;
    }

                        
    internal void Set(object key, Var value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        value.Key = key;
        value.Collection = this;
        Data[key] = value;
    }

                        
    internal bool ContainsKey(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Data.ContainsKey(key);
    }

                        
    internal bool Remove(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Data.Remove(key);
    }

            
    internal void Clear()
    {
        Data.Clear();
    }

                            
    internal bool TryGetValue(object key, out Var value)
    {
        ArgumentNullException.ThrowIfNull(key);
        return Data.TryGetValue(key, out value!);
    }

                
    internal IEnumerable<object> GetKeys()
    {
        return Data.Keys;
    }

                
    internal IEnumerable<Var> GetValues()
    {
        return Data.Values;
    }

    #endregion

    #region Double Dispatch Methods

    // Tables don't support arithmetic operations with other types


    protected internal override Var AddInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // RightPattern operand of + is not numeric


    protected internal override Var AddReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 2); // RightPattern operand of + is not numeric


    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // RightPattern operand of - is not numeric


    protected internal override Var SubtractReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 33); // RightPattern operand of - is not numeric


    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // RightPattern operand of * is not numeric


    protected internal override Var MultiplyReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 27); // RightPattern operand of * is not numeric


    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // RightPattern operand of / is not numeric


    protected internal override Var DivideReal(RealVar left, Executive executive)
        => LogArithmeticTypeError(executive, 13); // RightPattern operand of / is not numeric

    #endregion
}