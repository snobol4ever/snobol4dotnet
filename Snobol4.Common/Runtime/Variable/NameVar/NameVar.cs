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

    // What makes a name of a keyword or of a deferred expression one and the same as a table key: its own, kept by its copies
    internal object Identity = new();

    // What the name names when it is the name of an element: the collection and the key. They are the name's own, apart from Key and
    // Collection, which say where this value is stored, as for every value: a name stored in a table or an array element keeps what
    // it names (T<'n'> = .hold, then IDENT(T<'n'>, .hold) succeeds and $T<'n'> reads hold, as in SPITBOL)
    internal object? TargetKey;
    internal Var? TargetCollection;

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
        TargetKey = key;
        TargetCollection = collection;
    }


    internal NameVar(NameVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        Pointer = template.Pointer;
        TargetKey = template.TargetKey;
        TargetCollection = template.TargetCollection;
        Expression = template.Expression;
        Identity = template.Identity;
    }

    #endregion

    #region Name-Specific Methods

    // A name has text when it names a variable: the name of an element (no pointer) or of a keyword (.&ERRTEXT) has none, so it is no
    // string, number or pattern (SIZE(.&ERRTEXT) is 189, .&ERRTEXT 'z' 8, .&ERRTEXT ? 'x' 241) and prints as NAME, as in SPITBOL
    internal bool HasText => Pointer.Length > 0 && Pointer[0] != '&';

            
    public Var Dereference(Executive executive)
    {
        // Fast path: pointer dereference (most common case)
        if (TargetCollection is null)
        {
            return executive.IdentifierTable[Pointer];
        }

        // Slower path: collection element access
        return TargetCollection switch
        {
            ArrayVar arrayVar => arrayVar.Data[(int)(long)TargetKey!],
            TableVar tableVar => tableVar.GetOrDefault(TargetKey!),
            ProgramDefinedDataVar pdVar => pdVar.FieldValues.Data[(int)(long)TargetKey!],
            _ => StringVar.Null()
        };
    }

            
    public bool IsCollectionReference()
    {
        return TargetCollection is not null;
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