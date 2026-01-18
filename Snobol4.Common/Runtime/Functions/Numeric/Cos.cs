namespace Snobol4.Common;

//"cos argument not numeric" /* 303 */,
//"cos argument is out of range" /* 322 */,

public partial class Executive
{
    internal void Cos(List<Var> arguments) => UnaryNumericOperation(arguments, Cos0, 303, 322, 0);

    internal double Cos0(double dOperand) => Math.Cos(dOperand);
}