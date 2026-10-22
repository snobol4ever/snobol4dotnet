namespace Snobol4.Common;

public class SourceLine
{
    // Whether statement has already been compiled
    internal bool Compiled { get; set; }

    // Whether -LIST had its line listed when it was read (the page of an error in it does not list it again)
    internal bool Listed { get; init; }

    // The statement did not compile; it keeps its label, and reaching it is error 7 (sbl -k)
    internal bool CompileError { get; set; }

    // The statement stands on the last card of a source that has no END: SPITBOL reads on to see whether a card continues it, finds the
    // end of the source, and reports no error that it finds only once it has read the whole statement
    internal bool EndsTheSource { get; set; }

    // The number of an error found as the line was read (a continuation with nothing to continue), reported where the lexer reaches
    // the line so that it follows the line's listing
    internal int PendingError { get; set; }
    // Case folding as it stood when the line was read: a -CASE control acts from its own line on
    internal bool CaseFolding { get; }

    // Handler of -FAIL/-NOFAIL on ***current*** line
    internal bool ErrorOnUnhandledFail { get; init; }

    // Index of Include file
    internal int IncludeDepth { get; init; }

    // Index of this line in containing file
    internal int LineCountFile { get; init; }

    // Index of this line among semicolon delimited lines
    internal int LineCountSubLine { get; init; }

    // Index of this line in all files
    internal int LineCountTotal { get; init; }

    // Index of this line in this listing
    internal int LineCountList { get; init; }

    // Count of blank lines
    internal int BlankLineCount { get; init; }

    // Count of comments and directives
    internal int CommentContinuationDirectiveCount { get; init; }

    // The physical lines this statement was read from, the number SPITBOL gives the first statement on them, and where this
    // semicolon-delimited statement begins in the joined text of those lines (the report of a statement in error names the
    // physical line and column it was found at)
    internal List<string> PhysicalLines { get; init; }
    internal int FirstStatementNumber { get; init; }
    internal int SubLineOffset { get; init; }

    // Path to file
    internal string PathName { get; init; }

    // Unprocessed source code
    internal string Text { get; set; }

    // The text as SPITBOL's scanner reads it: with the semicolon that ends the statement, if one does
    internal string SyntaxText { get; init; }

    // Label of this line
    internal string Label { get; set; } = string.Empty;

    // Lexical analysis of body
    internal List<Token> LexBody { get; set; } = [];

    // Lexical analysis of failure goto
    internal List<Token> LexFailureGoto { get; set; } = [];

    // Lexical analysis of success goto
    internal List<Token> LexSuccessGoto { get; set; } = [];

    // Lexical analysis of unconditional goto
    internal List<Token> LexUnconditionalGoto { get; set; } = [];

    // Parse of body in RPN
    internal List<Token> ParseBody { get; set; }

    // Parse of failure goto in RPN
    internal List<Token> ParseFailureGoto { get; set; }

    // Parse of success goto in RPN
    internal List<Token> ParseSuccessGoto { get; set; }

    // Parse of unconditional goto in RPN
    internal List<Token> ParseUnconditionalGoto { get; set; }

    // Whether the success, failure and unconditional goto is written with nothing in its parens or brackets (the lists above are empty
    // for these and for a goto not written)
    internal bool EmptySuccessGoto { get; set; }
    internal bool EmptyFailureGoto { get; set; }
    internal bool EmptyUnconditionalGoto { get; set; }

    // True if first goto is direct (Angle brackets)
    internal bool DirectGotoFirst { get; set; }

    // True if second goto is direct (Angle brackets)
    internal bool DirectGotoSecond { get; set; }

    // True if first goto is success
    internal bool SuccessFirst { get; set; }

    // True if expression is deferred to runtime (Has ' *(' prepended and ')' appended)
    internal bool DeferredExpression { get; set; }

    internal SourceLine(string pathName, int includeDepth, string text, SourceCode code, bool errorOnUnhandledFail)
    {
        ErrorOnUnhandledFail = errorOnUnhandledFail;
        IncludeDepth = includeDepth;
        Listed = code.ListingOn;
        LineCountTotal = code.LineCountTotal;
        LineCountFile = code.LineCountFile;
        BlankLineCount = code.BlankLineCount;
        CommentContinuationDirectiveCount = code.CommentContinuationDirectiveCount;
        LineCountSubLine = code.SubLineCount;
        LineCountList = code.LineCountTotal;
        PhysicalLines = code.CurrentPhysicalLines;
        FirstStatementNumber = code.CurrentFirstStatementNumber;
        SubLineOffset = code.CurrentSubLineOffset;
        PathName = pathName;
        CaseFolding = code.CaseFoldingNow;
        Text = text.TrimEnd();
        SyntaxText = Text + code.CurrentSubLineTail;

        // Mutable collections initialized during parsing
        ParseBody = [];
        ParseFailureGoto = [];
        ParseSuccessGoto = [];
        ParseUnconditionalGoto = [];
    }
}