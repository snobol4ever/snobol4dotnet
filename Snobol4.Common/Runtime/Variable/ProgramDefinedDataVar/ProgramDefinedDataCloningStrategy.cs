namespace Snobol4.Common;

public sealed class ProgramDefinedDataCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;

        // Shallow clone - fields reference the same variables
        return new ProgramDefinedDataVar(dataSelf)
        {
            Symbol = dataSelf.Symbol,
            InputChannel = dataSelf.InputChannel,
            OutputChannel = dataSelf.OutputChannel
        };
    }
}