namespace Snobol4.Common;

public class UserFunctionTableEntry
{
    internal string FunctionName;
    internal string Prototype;
    internal List<string> Parameters;
    internal List<string> Locals;
    internal string EntryLabel;

    internal UserFunctionTableEntry(string functionName, string prototype, List<string> parameters, List<string> locals, string entryLabel)
    {
        FunctionName = functionName;
        Prototype = prototype;
        Parameters = parameters;
        Locals = locals;
        EntryLabel = entryLabel;
    }
}