using System.Diagnostics;

namespace Snobol4.Common;

internal class BalPattern : Pattern
{
    #region Members

    internal NullPattern GBal0;
    internal GBal1Pattern GBal1;
    internal GBalPattern GBal;

    #endregion

    #region Constructors

                internal BalPattern()
    {
        GBal0 = new NullPattern();
        GBal1 = new GBal1Pattern();
        GBal = new GBalPattern();
    }

    #endregion

    #region Methods

                    public static Pattern Structure()
    {
        var bal = new BalPattern();
        return new ConcatenatePattern(bal.GBal0, new ConcatenatePattern(bal.GBal1, bal.GBal));
    }

                    internal override Pattern Clone()
    {
        return new BalPattern();
    }

    #endregion

    #region Embedded Classes

                    [DebuggerDisplay("{DebugPattern()}")]
    internal class GBal1Pattern : TerminalPattern
    {
        internal override MatchResult Scan(int node, Scanner scan)
        {
            using var profile1 = Profiler.Start4("GBal1", scan.Exec);
            return MatchResult.Success(scan);
        }

        internal override Pattern Clone()
        {
            return new GBal1Pattern();
        }

        public override string DebugPattern() => "gbal1";
        
    }

                                [DebuggerDisplay("{DebugPattern()}")]
    internal class GBalPattern : TerminalPattern
    {
        internal override Pattern Clone()
        {
            return new GBalPattern();
        }

                                                                                                                internal override MatchResult Scan(int node, Scanner scan)
        {
            using var profile1 = Profiler.Start4("GBal", scan.Exec);

            // Fail if there are no more characters to scan or
            // the next character is a closing parenthesis
            if (scan.CursorPosition == scan.Subject.Length ||
                scan.Subject[scan.CursorPosition] == ')')
                return MatchResult.Failure(scan);

            // If the next character in the subject is not a parenthesis, then the
            // match succeeds. This pattern is pushed. If a subsequent fails,
            // the Bal pattern can be extended.
            if (scan.Subject[scan.CursorPosition] != '(')
            {
                var mr = MatchResult.Success(scan);
                scan.CursorPosition++;
                scan.SaveAlternate(node);
                return mr;
            }

            // If the next character in the subject is a parenthesis, look for a balanced match
            // Use ReadOnlySpan to avoid substring allocation
            var subject = scan.Subject.AsSpan(scan.CursorPosition);
            var parenCount = 1;
            var pos = 1;

            while (pos < subject.Length && parenCount > 0)
            {
                switch (subject[pos])
                {
                    case ')':
                        parenCount--;
                        break;
                    case '(':
                        parenCount++;
                        break;
                }

                pos++;
            }

            // Fail if there are no more characters to scan and no balance was found.
            if (parenCount > 0)
                return MatchResult.Failure(scan);

            // If a balanced string was matched, succeed.
            // This pattern is pushed. If a subsequent fails,
            // the Bal pattern can be extended.
            scan.CursorPosition += pos;
            scan.SaveAlternate(node);
            return MatchResult.Success(scan);
        }

        #region Debugging

                                        public override string DebugPattern() => "gbal";

        #endregion
    }

    #endregion

    #region Debugging

                    public override string DebugPattern() => "bal";

    #endregion

}
