#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class NameVar : Var
{
    #region Data

    internal string Pointer;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly NameArithmeticStrategy _arithmeticStrategy = new();
    private static readonly NameComparisonStrategy _comparisonStrategy = new();
    private static readonly NameConversionStrategy _conversionStrategy = new();
    private static readonly NameCloningStrategy _cloningStrategy = new();
    private static readonly NameFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors


    internal NameVar(string pointer, object? key, Var? collection)
    {
        Pointer = pointer;
        Key = key;
        Collection = collection;
    }


    internal NameVar(NameVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        Pointer = template.Pointer;
        Key = template.Key;
        Collection = template.Collection;
    }

    #endregion

    #region Name-Specific Methods

            
    public Var Dereference(Executive executive)
    {
        // Fast path: pointer dereference (most common case)
        if (Collection is null)
        {
            return executive.IdentifierTable[Pointer];
        }

        // Slower path: collection element access
        return Collection switch
        {
            ArrayVar arrayVar => arrayVar.Data[(int)(long)Key!],
            TableVar tableVar => tableVar.GetOrDefault(Key!),
            _ => StringVar.Null()
        };
    }

            
    public bool IsCollectionReference()
    {
        return Collection is not null;
    }

            
    public string GetTargetName()
    {
        return Pointer;
    }

    #endregion

    #region Double Dispatch Methods

    // Names support arithmetic through dereferencing
    // The arithmetic strategy handles this automatically


    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        // When name is the right operand, dereference it
        var target = Dereference(executive);
        return left.Add(target, executive);
    }


    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Add(target, executive);
    }


    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Subtract(target, executive);
    }


    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Subtract(target, executive);
    }


    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Multiply(target, executive);
    }


    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Multiply(target, executive);
    }


    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Divide(target, executive);
    }


    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        var target = Dereference(executive);
        return left.Divide(target, executive);
    }

    #endregion
}