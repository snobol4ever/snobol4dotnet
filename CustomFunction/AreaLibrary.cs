using Snobol4.Common;
using System.Diagnostics;

namespace AreaFunction;
public class Area
{
    private Executive? _executive;

    public void Init(Executive x)
    {
        _executive = x;

        if (_executive == null)
            throw new ArgumentNullException(nameof(x));

        var name1 = x.Parent.FoldCase("Init", "AreaOfCircle");
        var entry1 = new FunctionTableEntry(_executive, name1, AreaOfCircle, 1, false);
        _executive.FunctionTable[name1] = entry1;

        var name2 = x.Parent.FoldCase("Init", "AreaOfSquare");
        var entry2 = new FunctionTableEntry(_executive, name2, AreaOfSquare, 1, false);
        _executive.FunctionTable[name2] = entry2;
    }

    public void AreaOfCircle(List<Var> arguments)
    {
        Debug.Assert(_executive != null);

        if (!arguments[0].Convert(Executive.VarType.REAL, out var _, out var radiusValue, _executive))
            throw new InvalidCastException("Cannot convert argument for area of a circle to a number");

        Debug.Assert(radiusValue != null, nameof(radiusValue) + " != null");
        var area = Math.PI * (double)radiusValue * (double)radiusValue;
        _executive?.SystemStack.Push(new RealVar(area));
    }

    public void AreaOfSquare(List<Var> arguments)
    {
        Debug.Assert(_executive != null);

        if (!arguments[0].Convert(Executive.VarType.REAL, out var _, out var sideValue, _executive))
            throw new InvalidCastException("Cannot convert argument for area of a square to a number");

        Debug.Assert(sideValue != null, nameof(sideValue) + " != null");
        var area = (double)sideValue * (double)sideValue;
        _executive?.SystemStack.Push(new RealVar(area));
    }
}
