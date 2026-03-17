using Snobol4.Common;

namespace AreaFunction;

public class Area : IExternalLibrary
{
    private Executive? _executive;

    public void Init(Executive executive)
    {
        _executive = executive;

        var name1 = executive.Parent.FoldCase("AreaOfCircle");
        var entry1 = new FunctionTableEntry(executive, name1, AreaOfCircle, 1, false);
        executive.FunctionTable[name1] = entry1;

        var name2 = executive.Parent.FoldCase("AreaOfSquare");
        var entry2 = new FunctionTableEntry(executive, name2, AreaOfSquare, 1, false);
        executive.FunctionTable[name2] = entry2;
    }

    // IExternalLibrary.Unload() default no-op is sufficient here

    public void AreaOfCircle(List<Var> arguments)
    {
        if (!arguments[0].Convert(Executive.VarType.REAL, out var _, out var radiusValue, _executive!))
            throw new InvalidCastException("Cannot convert argument for area of a circle to a number");

        var area = Math.PI * (double)radiusValue * (double)radiusValue;
        _executive!.SystemStack.Push(new RealVar(area));
    }

    public void AreaOfSquare(List<Var> arguments)
    {
        if (!arguments[0].Convert(Executive.VarType.REAL, out var _, out var sideValue, _executive!))
            throw new InvalidCastException("Cannot convert argument for area of a square to a number");

        var area = (double)sideValue * (double)sideValue;
        _executive!.SystemStack.Push(new RealVar(area));
    }
}
