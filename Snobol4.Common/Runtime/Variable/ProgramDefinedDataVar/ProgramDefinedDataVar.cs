using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class ProgramDefinedDataVar : Var
{
    #region Data

    internal UserDataDefinition Definition;
    internal string DataName;                           //  Name of the user-defined data type
    internal ArrayVar FieldValues;                      //  Field name to value mapping
    internal List<StringVar> FieldNames = new();                //  List of field names

    #endregion

    #region Strategy Instances (Lazy-loaded singletons for performance)

    private static readonly ProgramDefinedDataArithmeticStrategy _arithmeticStrategy = new();
    private static readonly ProgramDefinedDataComparisonStrategy _comparisonStrategy = new();
    private static readonly ProgramDefinedDataConversionStrategy _conversionStrategy = new();
    private static readonly ProgramDefinedDataCloningStrategy _cloningStrategy = new();
    private static readonly ProgramDefinedDataFormattingStrategy _formattingStrategy = new();

    protected override IArithmeticStrategy ArithmeticStrategy => _arithmeticStrategy;
    protected override IComparisonStrategy ComparisonStrategy => _comparisonStrategy;
    protected override IConversionStrategy ConversionStrategy => _conversionStrategy;
    protected override ICloningStrategy CloningStrategy => _cloningStrategy;
    protected override IFormattingStrategy FormattingStrategy => _formattingStrategy;

    #endregion

    #region Constructors

    internal ProgramDefinedDataVar(string dataName, string prototype, List<string> fieldNames)
    {
        Definition = new UserDataDefinition(prototype, fieldNames);
        DataName = dataName;
        FieldValues = new ArrayVar();
        FieldValues.ConfigurePrototype("0:" + (fieldNames.Count - 1), new StringVar(""));

        //foreach (var index in Definition.FieldNames.Select(name => Definition.FieldNames.IndexOf(name)))
        for (var index = 0; index < Definition.FieldNames.Count; index++)
        {
            FieldValues.Data[index] = new StringVar("");
        }
    }

    internal ProgramDefinedDataVar(ProgramDefinedDataVar template)
    {
        Definition = template.Definition;
        DataName = template.DataName;
        FieldNames = template.FieldNames;
        FieldValues = template.FieldValues;
        Symbol = template.Symbol;
        InputChannel = template.InputChannel;
        OutputChannel = template.OutputChannel;
    }

    #endregion

    #region Double Dispatch Methods

    // User-defined data doesn't support arithmetic operations with other types

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