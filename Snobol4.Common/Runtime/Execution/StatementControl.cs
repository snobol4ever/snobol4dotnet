namespace Snobol4.Common;

public partial class Executive
{
    public int ExecuteLoop(int i)
    {
        while (i >= 0)
        {
            if (Parent.TraceStatements)
                Console.Error.WriteLine(@$"{i} {SourceCode[i]}");

            i = Statements[i](this);

            //if (((IntegerVar)IdentifierTable["&stlimit"]).Data <= 0)
            //    continue;
            if (Amp_StatementLimit <= 0)
                continue;

            //if (((IntegerVar)IdentifierTable["&stcount"]).Data <= ((IntegerVar)IdentifierTable["&stlimit"]).Data) 
            //    continue;
            if (Amp_StatementCount <= Amp_StatementLimit)
                continue;

            LogRuntimeException(244);
            Failure = true;
            break;
        }
        return i;
    }

    /// <summary>
    /// Set a breakpoint here to step into compiled Snobol4 program
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static void BreakPoint()
    {
    }
}