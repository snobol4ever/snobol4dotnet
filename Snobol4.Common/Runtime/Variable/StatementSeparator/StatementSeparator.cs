#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

/// <summary>
/// Special marker variable used to delineate statement boundaries on the system stack
/// </summary>
[DebuggerDisplay("{DebugString()}")]
public class StatementSeparator : Var
{
    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly StatementSeparatorArithmeticStrategy _arithmeticStrategy = new();
    private static readonly StatementSeparatorComparisonStrategy _comparisonStrategy = new();
    private static readonly StatementSeparatorConversionStrategy _conversionStrategy = new();
    private static readonly StatementSeparatorCloningStrategy _cloningStrategy = new();
    private static readonly StatementSeparatorFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

    public StatementSeparator()
    {
        // Statement separators have no data
        Symbol = "<separator>";
    }

    #endregion

    #region StatementSeparator-Specific Methods

    /// <summary>
    /// Check if this is a statement separator (always true for this type)
    /// </summary>
    public static bool IsStatementSeparator(Var var)
    {
        return var is StatementSeparator;
    }

    #endregion

    #region Double Dispatch Methods

    // Statement separators should never participate in arithmetic operations
    // These throw exceptions to catch programming errors

    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        throw new InvalidOperationException("Statement separator cannot participate in arithmetic");
    }

    #endregion
}