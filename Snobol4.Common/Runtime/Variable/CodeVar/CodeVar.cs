#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class CodeVar : Var
{
    #region Data

    public int StatementNumber;
    public string Data;

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly CodeArithmeticStrategy _arithmeticStrategy = new();
    private static readonly CodeComparisonStrategy _comparisonStrategy = new();
    private static readonly CodeConversionStrategy _conversionStrategy = new();
    private static readonly CodeCloningStrategy _cloningStrategy = new();
    private static readonly CodeFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors


    public CodeVar()
    {
        StatementNumber = -1;
        Data = string.Empty;
    }


    public CodeVar(int statementNumber, string data)
    {
        StatementNumber = statementNumber;
        Data = data;
    }


    internal CodeVar(CodeVar template)
    {
        OutputChannel = template.OutputChannel;
        InputChannel = template.InputChannel;
        Symbol = template.Symbol;
        StatementNumber = template.StatementNumber;
        Data = template.Data;
    }

    #endregion

    #region Code-Specific Methods

    /// <summary>
    /// Execute this code block in the context of the executive
    /// </summary>

    public int Execute(Executive executive)
    {
        return executive.ExecuteLoop(StatementNumber);
    }

    /// <summary>
    /// Get the statement number for direct goto
    /// </summary>

    public int GetStatementNumber()
    {
        return StatementNumber;
    }

    #endregion

    #region Double Dispatch Methods

    // Code doesn't support arithmetic operations with other types


    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(2); // RightPattern operand of + is not numeric
        return StringVar.Null();
    }


    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(2); // RightPattern operand of + is not numeric
        return StringVar.Null();
    }


    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(33); // RightPattern operand of - is not numeric
        return StringVar.Null();
    }


    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(33); // RightPattern operand of - is not numeric
        return StringVar.Null();
    }


    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(27); // RightPattern operand of * is not numeric
        return StringVar.Null();
    }


    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(27); // RightPattern operand of * is not numeric
        return StringVar.Null();
    }


    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(13); // RightPattern operand of / is not numeric
        return StringVar.Null();
    }


    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(13); // RightPattern operand of / is not numeric
        return StringVar.Null();
    }

    #endregion
}