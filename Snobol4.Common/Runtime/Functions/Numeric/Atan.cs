namespace Snobol4.Common;

//"atan argument not numeric" /* 301 */,

public partial class Executive
{
    internal void Atan(List<Var> arguments) => UnaryNumericOperation(arguments, Atan0, 301, 0, 0);

    internal double Atan0(double dOperand) => Math.Atan(dOperand);            
}