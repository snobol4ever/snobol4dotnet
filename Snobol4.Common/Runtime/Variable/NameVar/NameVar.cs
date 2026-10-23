#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class NameVar : Var
{
    #region Data

    internal string Pointer;

    // The name of a deferred expression (.*A<I>, .*SIZE(X)): the expression, evaluated by name each time the name is used, as in
    // SPITBOL ($N reads A<I> for the I of that moment, $N = V assigns it); such a name has no text
    internal ExpressionVar? Expression;

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
        Expression = template.Expression;
    }

    #endregion

    #region Name-Specific Methods

    // A name has text when it names a variable: the name of an element (no pointer) or of a keyword (.&ERRTEXT) has none, so it is no
    // string, number or pattern (SIZE(.&ERRTEXT) is 189, .&ERRTEXT 'z' 8, .&ERRTEXT ? 'x' 241) and prints as NAME, as in SPITBOL
    internal bool HasText => Pointer.Length > 0 && Pointer[0] != '&';

            
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
            ProgramDefinedDataVar pdVar => pdVar.FieldValues.Data[(int)(long)Key!],
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