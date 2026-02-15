namespace Snobol4.Common;

//"detach argument is not appropriate name" /* 87 */,

public partial class Executive
{
    internal void Detach(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(87);
            return;
        }

        var symbol =(string)value;

        if (symbol == "")
        {
            LogRuntimeException(87);
            return;
        }

        var inputChannel = IdentifierTable[symbol].InputChannel;

        if (inputChannel != "")
            foreach (var identifier in IdentifierTable.Where(identifier => identifier.Value.InputChannel == inputChannel))
                identifier.Value.InputChannel = "";

        var outputChannel = IdentifierTable[symbol].OutputChannel;

        if (outputChannel != "")
            foreach (var identifier in IdentifierTable.Where(identifier => identifier.Value.OutputChannel == outputChannel))
                identifier.Value.InputChannel = "";

        PredicateSuccess();
    }
}