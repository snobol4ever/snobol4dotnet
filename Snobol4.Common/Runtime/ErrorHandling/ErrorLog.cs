namespace Snobol4.Common;

public partial class Executive
{

    public StringVar LogRuntimeException(int code, Exception? e = null)
    {
        ((IntegerVar)IdentifierTable["&errtype"]).Data = code;
        Failure = true;
        var nullStringVar = new StringVar(false);
        SystemStack.Push(nullStringVar);
        var ce = new CompilerException(code);
        Parent.ErrorCodeHistory.Add(code);
        Parent.ColumnHistory.Add(0);
        var fi = new FileInfo(((StringVar)IdentifierTable["&file"]).Data);
        ce.Message = $"{Environment.NewLine}{fi.Name}({((IntegerVar)IdentifierTable["&line"]).Data + 1}) : error {code} -- {CompilerException.ErrorMessage[code]}{Environment.NewLine}{SourceCode[(int)((IntegerVar)IdentifierTable["&stno"]).Data - 1].Split('\n')[1]}";
        ((StringVar)IdentifierTable["&errtext"]).Data = ce.Message[2..];

        if (e != null)
        {
            ce.Message += $"{Environment.NewLine}{e.Message}";
            Console.Error.WriteLine(ce.Message);
            ((StringVar)IdentifierTable["&errtext"]).Data = e.Message[2..];
        }

        Parent.MessageHistory.Add(ce.Message);
        var errorLimit = ((IntegerVar)IdentifierTable["&errlimit"]).Data;
        Console.Error.WriteLine($@"{ce.Message}");
        ((StringVar)IdentifierTable["&errtext"]).Data = ce.Message;

        if(!Parent.CodeMode && (errorLimit<1 || Parent.StopOnRuntimeError))
        {
            ((IntegerVar)IdentifierTable["&code"]).Data = ce.Code;
            throw ce;
        }

        ((IntegerVar)IdentifierTable["&errlimit"]).Data = errorLimit - 1;
        return nullStringVar;
    }

    public StringVar NonExceptionFailure()
    {
        Failure = true;
        var nullVar = new StringVar(false);
        SystemStack.Push(nullVar);
        return nullVar;
    }

    public StringVar PredicateSuccess()
    {
        Failure = false;
        var nullVar = new StringVar(true);
        SystemStack.Push(nullVar);
        return nullVar;
    }
}