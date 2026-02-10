    using System.Diagnostics;

    namespace Snobol4.Common;

[DebuggerDisplay("{DebugPattern()}")]
internal class LiteralPattern : TerminalPattern
{
    #region Members

                internal string Literal;

    #endregion

    #region Construction

                        internal LiteralPattern(string literal)
    {
        Literal = literal ?? throw new ArgumentNullException(nameof(literal));
    }

    #endregion

    #region Methods

                    internal override Pattern Clone()
    {
        return new LiteralPattern(Literal);
    }

                                                        internal override MatchResult Scan(int node, Scanner scan)
    {
        using var profile1 = Profiler.Start4("Literal", scan.Exec);

        var remainingLength = scan.Subject.Length - scan.CursorPosition;

        // Early exit if not enough characters remain
        if (remainingLength < Literal.Length)
            return MatchResult.Failure(scan);

        // Use ReadOnlySpan to avoid substring allocation
        var subjectSpan = scan.Subject.AsSpan(scan.CursorPosition, Literal.Length);

        // Use SequenceEqual for optimized comparison
        if (!subjectSpan.SequenceEqual(Literal.AsSpan()))
            return MatchResult.Failure(scan);

        // Advance cursor past the matched literal
        scan.CursorPosition += Literal.Length;
        return MatchResult.Success(scan);
    }

    #endregion

    #region Debugging

                                    public override string DebugPattern() => "literal";

    #endregion
}