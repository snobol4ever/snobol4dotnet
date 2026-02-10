namespace Snobol4.Common;

internal class ConditionalVariableAssociation1 : NullPattern
{
    #region Members

                internal Var Assignee;

                internal Executive Exec;

    #endregion

    #region Construction

                                        internal ConditionalVariableAssociation1(Var assignee, Executive exec)
    {
        Assignee = assignee;
        Exec = exec;
    }

    #endregion

    #region Methods

                    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociation1(Assignee, Exec);
    }

                                                                                    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".1", scan.Exec);
        
        Exec.AlphaStack.Push(new NameListEntry(Assignee, scan.CursorPosition, -1, scan));
        return MatchResult.Success(scan);
    }

    #endregion
}

internal class ConditionalVariableAssociationBackup1 : NullPattern
{
                internal Var Assignee;

                internal Executive Exec;

                    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociationBackup1(Assignee, Exec);
    }

                        internal ConditionalVariableAssociationBackup1(Var assignee, Executive exec)
    {
        Assignee = assignee;
        Exec = exec;
    }

                                                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".2", scan.Exec);
        Exec.AlphaStack.Pop();
        return MatchResult.Failure(scan);
    }
}

internal class ConditionalVariableAssociation2 : NullPattern
{
                internal Var Assignee;

                internal Executive Exec;

                    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociation2(Assignee, Exec);
    }

                                        internal ConditionalVariableAssociation2(Var assignee, Executive exec)
    {
        Assignee = assignee;
        Exec = exec;
    }

                                                                                                                internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".3", scan.Exec);
   
        if (Exec.AlphaStack.Count == 0)
        {
            return MatchResult.Failure(scan);
        }
        var nameEntry = Exec.AlphaStack.Pop();
        nameEntry.PostCursor = scan.CursorPosition;
        Exec.BetaStack.Push(nameEntry);
        return MatchResult.Success(scan);
    }
}

internal class ConditionalVariableAssociationBackup2 : NullPattern
{
                internal Executive Exec;

                internal Var Assignee;

                    internal override Pattern Clone()
    {
        return new ConditionalVariableAssociationBackup2(Assignee, Exec);
    }

                        internal ConditionalVariableAssociationBackup2(Var assignee, Executive exec)
    {
        Exec = exec;
        Assignee = assignee;
    }

                                                                                    internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4(".4", scan.Exec);

        if (Exec.BetaStack.Count == 0)
        {
            return MatchResult.Failure(scan);
        }
        var nameListEntry = Exec.BetaStack.Pop();
        nameListEntry.PostCursor = 0;
        Exec.AlphaStack.Push(nameListEntry);
        return MatchResult.Failure(scan);
    }
}

internal class NameListEntry
{
                internal Var Assignee;

                    internal int PreCursor;

                    internal int PostCursor;

                    internal Scanner Scan;

                                                                        internal NameListEntry(Var assignee, int pre, int post, Scanner scan)
    {
        Assignee = assignee;
        PreCursor = pre;
        PostCursor = post;
        Scan = scan;
    }
}