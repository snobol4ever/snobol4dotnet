namespace Snobol4.Common;

public partial class Executive
{
    public int ExecuteLoop(int i)
    {
        if (LabelTable[Parent.FoldCase(Parent.EntryLabel)] != GotoNotFound)
            i = LabelTable[Parent.FoldCase(Parent.EntryLabel)];

        while (i >= 0)
        {
            using var profiler1 = Profiler.Start1($"Statement{AmpCurrentLineNumber:000000}", this);

            if (Parent.TraceStatements)
                Console.Error.WriteLine(@$"{i} {SourceCode[i]}");

            if (AmpStatementLimit >= 0)
                AmpStatementCount++;

            i = Statements[i](this);

            if (AmpStatementLimit <= 0)
                continue;

            if (AmpStatementCount < AmpStatementLimit)
                continue;

            LogRuntimeException(244);
            Failure = true;
            break;
        }
        return i;
    }

    // ReSharper disable once UnusedMember.Global
    public static void BreakPoint()
    {
    }
}