namespace Snobol4.Common;

public class SourceLine
{
    #region Members

    internal bool Compiled = false;                   // Whether statement has already been compiled
    internal bool ErrorOnUnhandledFail;               // Handler of -FAIL/-NOFAIL on ***current*** line
    internal int IncludeDepth;                        // Index of Include file
    internal int LineCountFile;                       // Index of this line in containing file
    internal int LineCountSubLine;                    // Index of this line among semicolon delimited lines
    internal int LineCountTotal;                      // Index of this line in all files
    internal string PathName;                         // Path to file
    internal List<Token> LexBody = [];                // Lexical analysis of body
    internal List<Token> LexFailureGoto = [];         // Lexical analysis of failure goto
    internal List<Token> LexSuccessGoto = [];         // Lexical analysis of success goto
    internal List<Token> LexUnconditionalGoto = [];   // Lexical analysis of unconditional goto
    internal bool DirectGotoFirst;                    // True if first goto is direct (Angle brackets)
    internal bool DirectGotoSecond;                   // True if second goto is direct (Angle brackets)
    internal bool SuccessFirst;                       // true if first goto is success
    internal List<Token> ParseBody;                   // Parse of body in RPN
    internal List<Token> ParseFailureGoto;            // Parse of failure goto in RPN
    internal List<Token> ParseSuccessGoto;            // Parse of success goto in RPN
    internal List<Token> ParseUnconditionalGoto;      // Parse of unconditional got in RPN
    internal string Label = "";                       // Label of this line
    internal string Text;                             // Unprocessed source code

    #endregion

    #region Constructor

                internal SourceLine(string pathName, int includeDepth, string text, SourceCode code, bool errorOnUnhandledFail)
    {
        ErrorOnUnhandledFail = errorOnUnhandledFail;
        IncludeDepth = includeDepth;
        LineCountTotal = code.LineCountTotal;
        LineCountFile = code.LineCountFile;
        LineCountSubLine = code.SubLineCount;
        ParseBody = [];
        ParseFailureGoto = [];
        ParseSuccessGoto = [];
        ParseUnconditionalGoto = [];
        ParseBody = [];
        ParseFailureGoto = [];
        ParseSuccessGoto = [];
        ParseUnconditionalGoto = [];
        PathName = pathName;
        Text = text.TrimEnd();
        DirectGotoFirst = false;
        DirectGotoSecond = false;
    }

    #endregion
}