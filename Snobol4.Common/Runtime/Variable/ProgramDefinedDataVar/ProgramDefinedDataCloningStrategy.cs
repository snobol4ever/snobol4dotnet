using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for program-defined data variables
/// Creates a shallow copy of the data structure
/// </summary>
public sealed class ProgramDefinedDataCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var dataSelf = (ProgramDefinedDataVar)self;

        // Shallow clone - fields reference the same variables
        return new ProgramDefinedDataVar(dataSelf.UserDefinedDataName, dataSelf.ProgramDefinedData)
        {
            Symbol = dataSelf.Symbol,
            InputChannel = dataSelf.InputChannel,
            OutputChannel = dataSelf.OutputChannel
        };
    }
}