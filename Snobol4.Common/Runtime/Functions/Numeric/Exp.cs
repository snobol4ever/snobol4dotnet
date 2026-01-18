namespace Snobol4.Common;

//"exp argument not numeric" /* 304 */,
//"exp produced real overflow" /* 305 */,

public partial class Executive
{
    internal void Exp(List<Var> arguments) => UnaryNumericOperation(arguments, Exp0, 304, 305, 0);

    internal double Exp0(double dOperand) => Math.Exp(dOperand);
}