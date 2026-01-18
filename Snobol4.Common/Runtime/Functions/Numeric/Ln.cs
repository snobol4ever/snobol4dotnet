namespace Snobol4.Common;

//"ln argument not numeric" /* 306 */,
//"ln produced real overflow" /* 307 */,
//"ln argument negative" /* 315 */,

public partial class Executive
{
    internal void Ln(List<Var> arguments) => UnaryNumericOperation(arguments, Ln0, 306, 307, 315);

    internal double Ln0(double dOperand) => Math.Log(dOperand);
}