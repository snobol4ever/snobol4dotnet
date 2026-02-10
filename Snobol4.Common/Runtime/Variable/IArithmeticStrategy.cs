namespace Snobol4.Common;

public interface IArithmeticStrategy
{
                Var Add(Var self, Var other, Executive executive);

                Var Subtract(Var self, Var other, Executive executive);

                Var Multiply(Var self, Var other, Executive executive);

                Var Divide(Var self, Var other, Executive executive);

                Var Power(Var self, Var other, Executive executive);

                Var Negate(Var self, Executive executive);
}