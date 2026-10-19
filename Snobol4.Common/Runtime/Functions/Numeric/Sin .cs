namespace Snobol4.Common;

//"sin argument not numeric" /* 308 */,
//"sin argument is out of range" /* 323 */,

public partial class Executive
{
    internal void Sin(List<Var> arguments) => UnaryNumericOperation(arguments, Sin0, 308, 323, 0);

    internal double Sin0(double dOperand) => RealVar.Sse(Math.Sin(RealVar.Sse(dOperand)));   // an underflow gives 0., as SPITBOL's f_* routines
}