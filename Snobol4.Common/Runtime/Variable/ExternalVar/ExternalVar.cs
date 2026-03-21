using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Snobol4.Common;

/// <summary>
/// Opaque native pointer variable — wraps an IntPtr returned by a C external
/// function declared with return type EXTERNAL.  The pointer can be passed back
/// to subsequent C calls as a NOCONV argument (shares the IntPtr ABI slot).
///
/// This is the DOTNET equivalent of SPITBOL's XNBLK/XRBLK return mechanism:
/// a foreign function allocates an object, returns its address, and SNOBOL4
/// passes that address back on subsequent calls without touching the contents.
/// </summary>
[DebuggerDisplay("{FormattingStrategy.DebugVar(this)}")]
public sealed class ExternalVar : Var
{
    #region Data

    /// <summary>Raw native pointer returned by the C function.</summary>
    public IntPtr Pointer;

    #endregion

    #region Strategy Instances

    private static readonly ExternalArithmeticStrategy  _arithmeticStrategy  = new();
    private static readonly ExternalComparisonStrategy  _comparisonStrategy  = new();
    private static readonly ExternalConversionStrategy  _conversionStrategy  = new();
    private static readonly ExternalCloningStrategy     _cloningStrategy     = new();
    private static readonly ExternalFormattingStrategy  _formattingStrategy  = new();

    protected override IArithmeticStrategy  ArithmeticStrategy  => _arithmeticStrategy;
    protected override IComparisonStrategy  ComparisonStrategy  => _comparisonStrategy;
    protected override IConversionStrategy  ConversionStrategy  => _conversionStrategy;
    protected override ICloningStrategy     CloningStrategy     => _cloningStrategy;
    protected override IFormattingStrategy  FormattingStrategy  => _formattingStrategy;

    #endregion

    #region Constructors

    public ExternalVar(IntPtr pointer)
    {
        Pointer = pointer;
    }

    #endregion

    #region Equality

    public override bool Equals(object? obj) =>
        obj is ExternalVar other && Pointer == other.Pointer;

    public override int GetHashCode() => Pointer.GetHashCode();

    public override bool Equals(Var? other) =>
        other is ExternalVar ext && Pointer == ext.Pointer;

    public override string ToString() => _formattingStrategy.ToString(this);

    #endregion
}
