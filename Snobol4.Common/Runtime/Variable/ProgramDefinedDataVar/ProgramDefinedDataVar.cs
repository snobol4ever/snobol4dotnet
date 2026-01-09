#pragma warning disable CS8770 // Method lacks `[DoesNotReturn]` annotation to match implemented or overridden member.
using System.Diagnostics;

namespace Snobol4.Common;

[DebuggerDisplay("{DebugString()}")]
public class ProgramDefinedDataVar : Var
{
    #region Data

    internal string UserDefinedDataName;
    internal Dictionary<string, Var> ProgramDefinedData;

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

    internal ProgramDefinedDataVar(string userDefinedDataName, Dictionary<string, Var> programDefinedData)
    {
        UserDefinedDataName = userDefinedDataName;
        ProgramDefinedData = programDefinedData;
    }

    internal ProgramDefinedDataVar(ProgramDefinedDataVar template)
    {
        ProgramDefinedData = template.ProgramDefinedData;
        UserDefinedDataName = template.UserDefinedDataName;
        Symbol = template.Symbol;
        InputChannel = template.InputChannel;
        OutputChannel = template.OutputChannel;
    }

    #endregion

    #region ProgramDefinedData-Specific Methods

    /// <summary>
    /// Get the value of a field by name
    /// </summary>
    public Var GetField(string fieldName)
    {
        return ProgramDefinedData.TryGetValue(fieldName, out var value)
            ? value
            : StringVar.Null();
    }

    /// <summary>
    /// Set the value of a field by name
    /// </summary>
    public void SetField(string fieldName, Var value)
    {
        ProgramDefinedData[fieldName] = value;
    }

    /// <summary>
    /// Check if a field exists
    /// </summary>
    public bool HasField(string fieldName)
    {
        return ProgramDefinedData.ContainsKey(fieldName);
    }

    /// <summary>
    /// Get all field names
    /// </summary>
    public IEnumerable<string> GetFieldNames()
    {
        return ProgramDefinedData.Keys;
    }

    /// <summary>
    /// Get the number of fields
    /// </summary>
    public int FieldCount => ProgramDefinedData.Count;

    /// <summary>
    /// Get the user-defined type name
    /// </summary>
    public string TypeName => UserDefinedDataName;

    #endregion

    #region Double Dispatch Methods

    // User-defined data doesn't support arithmetic operations with other types

    protected internal override Var AddInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(2); // Right operand of + is not numeric
        return StringVar.Null();
    }

    protected internal override Var AddReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(2); // Right operand of + is not numeric
        return StringVar.Null();
    }

    protected internal override Var SubtractInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(33); // Right operand of - is not numeric
        return StringVar.Null();
    }

    protected internal override Var SubtractReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(33); // Right operand of - is not numeric
        return StringVar.Null();
    }

    protected internal override Var MultiplyInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(27); // Right operand of * is not numeric
        return StringVar.Null();
    }

    protected internal override Var MultiplyReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(27); // Right operand of * is not numeric
        return StringVar.Null();
    }

    protected internal override Var DivideInteger(IntegerVar left, Executive executive)
    {
        executive.LogRuntimeException(13); // Right operand of / is not numeric
        return StringVar.Null();
    }

    protected internal override Var DivideReal(RealVar left, Executive executive)
    {
        executive.LogRuntimeException(13); // Right operand of / is not numeric
        return StringVar.Null();
    }

    #endregion
}