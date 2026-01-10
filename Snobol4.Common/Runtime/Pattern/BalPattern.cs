namespace Snobol4.Common;

internal class BalPattern : Pattern
{
    #region Members

    internal NullPattern GBal0;
    internal GBal1Pattern GBal1;
    internal GBalPattern GBal;

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor forms three patterns. 
    /// </summary>
    internal BalPattern()
    {
        GBal0 = new NullPattern();
        GBal1 = new GBal1Pattern();
        GBal = new GBalPattern();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Structure of composite pattern
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// First pattern is a NULL pattern augmented to store the cursor position for
    /// failure of BAL.
    /// </summary>
    internal class GBal1Pattern : TerminalPattern
    {
        internal override MatchResult Scan(int node, Scanner scan)
        {
            return MatchResult.Success(scan);
        }

        internal override Pattern Clone()
        {
            return new GBal1Pattern();
        }
    }

    /// <summary>
    /// GBal pattern matches
    ///      (1) the shortest possible string without any parentheses,
    ///      (2) any string that starts with a left parenthesis and
    ///          ends with a right parenthesis, where parentheses are matched.
    ///      (3) any combination of 1 and 2.
    /// </summary>
    internal class GBalPattern : TerminalPattern
    {
        internal override Pattern Clone()
        {
            return new GBalPattern();
        }

        internal override MatchResult Scan(int node, Scanner scan)
        {
            // Fail if there are no more characters to scan or
            // the next character is a closing parenthesis
            if (scan.CursorPosition == scan.Subject.Length || scan.Subject[scan.CursorPosition] == ')')
                return MatchResult.Failure(scan);

            // If the next character in the subject is not a parenthesis, then the
            // match succeeds. // This pattern is pushed. If a subsequent fails,
            // the Bal pattern can be extended.
            if (scan.Subject[scan.CursorPosition] != '(')
            {
                var mr = MatchResult.Success(scan);
                scan.CursorPosition++;
                scan.SaveAlternate(node);
                return mr;
            }

            // If the next character in the subject is a parenthesis, look for a balanced match
            var parenCount = 1;
            ++scan.CursorPosition;
            while (scan.CursorPosition < scan.Subject.Length && parenCount > 0)
            {
                switch (scan.Subject[scan.CursorPosition])
                {
                    case ')':
                        parenCount--;
                        break;
                    case '(':
                        parenCount++;
                        break;
                }

                ++scan.CursorPosition;
            }

            // Fail if there are no more characters to can and no balance was found.
            if (parenCount > 0)
                return MatchResult.Failure(scan);

            // If a balanced string was match, succeed.
            // This pattern is pushed. If a subsequent fails,
            // the Bal pattern can be extended.
            scan.SaveAlternate(node);
            return MatchResult.Success(scan);
        }
    }

    #endregion
}
