namespace Snobol4.Common;

public class SourceLine
{
    // Whether statement has already been compiled
    internal bool Compiled { get; set; }
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

    // Path to file
    internal string PathName { get; init; }

    // Unprocessed source code
    internal string Text { get; set; }

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
        LineCountTotal = code.LineCountTotal;
        LineCountFile = code.LineCountFile;
        BlankLineCount = code.BlankLineCount;
        CommentContinuationDirectiveCount = code.CommentContinuationDirectiveCount;
        LineCountSubLine = code.SubLineCount;
        LineCountList = code.LineCountTotal;
        PathName = pathName;
        CaseFolding = code.CaseFoldingNow;
        Text = text.TrimEnd();

        // Mutable collections initialized during parsing
        ParseBody = [];
        ParseFailureGoto = [];
        ParseSuccessGoto = [];
        ParseUnconditionalGoto = [];
    }
}