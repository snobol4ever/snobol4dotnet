namespace Snobol4.Common;

//"sqrt argument not numeric" /* 313 */,
//"sqrt argument negative" /* 314 */,

public partial class Executive
{
    internal void Sqrt(List<Var> arguments) => UnaryNumericOperation(arguments, Sqrt0, 313, 314, 314);

    internal double Sqrt0(double dOperand) => Math.Sqrt(dOperand);
}