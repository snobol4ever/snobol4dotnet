namespace Snobol4.Common;

//"eject argument is not a suitable name" /* 92 */,
//"eject file does not exist" /* 93 */,
//"eject file does not permit page eject" /* 94 */,
//"eject caused non-recoverable output error" /* 95 */,

public partial class Executive
{
    internal void EjectFile(List<Var> arguments)
    {
        // EJECT() ejects standard output: a form feed is written to it. EJECT(n) ejects the file open for output on channel n: a form
        // feed and a line end are written to it. Any other channel is a file that does not exist (93); an argument that is no string
        // or number is 92.
        if (arguments[0] is StringVar { Data: "" })
        {
            Console.Out.Write('\f');
            PredicateSuccess();
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var channel, this))
        {
            LogRuntimeException(92);
            return;
        }

        if (!StreamOutputs.TryGetValue((string)channel, out var stream))
        {
            LogRuntimeException(93);
            return;
        }

        stream.Write(System.Text.Encoding.UTF8.GetBytes("\f" + Environment.NewLine));
        PredicateSuccess();
    }

}
