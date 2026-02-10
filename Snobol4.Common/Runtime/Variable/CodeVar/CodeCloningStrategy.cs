namespace Snobol4.Common;

public sealed class CodeCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var codeSelf = (CodeVar)self;

        return new CodeVar(codeSelf.StatementNumber, codeSelf.Data)
        {
            Symbol = codeSelf.Symbol,
            InputChannel = codeSelf.InputChannel,
            OutputChannel = codeSelf.OutputChannel
        };
    }
}