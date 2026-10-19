namespace Snobol4.Common;

//"tan argument not numeric" /* 309 */,
//"tan produced real overflow or argument is out of range" /* 310 */,

public partial class Executive
{
    internal void Tan(List<Var> arguments) => UnaryNumericOperation(arguments, Tan0, 309, 310, 0);

    internal double Tan0(double dOperand) => RealVar.Sse(Math.Tan(RealVar.Sse(dOperand)));   // an underflow gives 0., as SPITBOL's f_* routines            
}