using System.Diagnostics;
using System.Globalization;

namespace Snobol4.Common;

public partial class Executive
{
    public const int GotoNotFound = -10;

    public bool Failure;
    /// <summary>Set by ThreadedExecuteLoop before restoring Failure, so
    /// RunExpressionThread can re-apply the sub-expression's failure result.</summary>
    internal bool LastExpressionFailure;
    public Builder Parent;
    public List<long> SourceLineNumbers;
    public List<long> SourceListingNumbers;
    public List<long> SourceStatementNumbers;
    public List<string> SourceCode;
    public List<string> SourceFiles;
    public SystemStack SystemStack;
    public List<DeferredCode> StarFunctionList;
    public int PreviousStarFunctionCount;
    public delegate int StatementCode(Executive x);
    public delegate void DeferredCode(Executive x);
    public List<StatementCode> Statements;
    internal Stack<string> ProgramDefinedFunctionStack = [];

    // Threaded execution (Phase 3+)
    internal Instruction[]? Thread;
    internal int InstructionPointer;
    internal int ErrorJump;

    /// <summary>
    /// Reusable argument list for OperatorFast — avoids per-call List allocation.
    /// Cleared and refilled before every use. Must not be retained across calls.
    /// </summary>
    internal readonly List<Var> _reusableArgList = new(8);

    // Symbol Tables
    public FunctionTable FunctionTable;
    public IdentifierTable IdentifierTable;
    public LabelTable LabelTable;
    public UserFunctionTable UserFunctionTable;

    internal Stack<bool> FailureStack;
    internal string ReturnType;
    internal Stack<NameListEntry> AlphaStack;     // Stack for conditional variable assignment
    internal Stack<NameListEntry> BetaStack;      // Stack for conditional variable assignment
    internal List<ArrayVar> IndexedArrays;
    internal List<TableVar> IndexedTables;

    internal readonly Stopwatch _timerExecute; // Timer for statistics

    // Map of channel names to streams. A stream is either
    // a StreamReader or a StreamWriter, but not both.
    internal Dictionary<string, Stream> StreamInputs;
    internal Dictionary<string, Stream> StreamOutputs;
    internal Dictionary<string, StreamReader> StreamReadersBySymbol;
    internal Dictionary<string, StreamReader> StreamReadersByChannel;

    internal Dictionary<string, string> TraceTableIdentifierAccess;
    internal Dictionary<string, string> TraceTableIdentifierValue;
    internal Dictionary<string, string> TraceTableIdentifierKeyword;
    internal Dictionary<string, string> TraceTableLabel;
    internal Dictionary<string, string> TraceTableFunctionCall;
    internal Dictionary<string, string> TraceTableFunctionReturn;

    internal Dictionary<string, long> ProfileTotal;
    internal Dictionary<string, long> ProfileCount;

    public Executive(Builder parent)
    {
        Parent = parent;
        Statements = [];
        AlphaStack = [];
        BetaStack = [];
        IndexedArrays = [];
        IndexedTables = [];
        _timerExecute = new Stopwatch();
        FailureStack = [];
        StreamInputs = [];
        LabelTable = new LabelTable(this);
        UserFunctionTable = new UserFunctionTable(this);
        StreamOutputs = [];
        ReturnType = "";
        SourceCode = [];
        SourceFiles = [];
        SourceLineNumbers = [];
        SourceListingNumbers = [];
        SourceStatementNumbers = [];
        SystemStack = [];
        StarFunctionList = [];
        StreamReadersBySymbol = [];  // Dictionary associating labels to line numbers
        StreamReadersByChannel = [];  // Dictionary associating labels to line numbers;

        AmpReturnType = "";
        AmpOutput = "";

        TraceTableIdentifierAccess = [];
        TraceTableIdentifierKeyword = [];
        TraceTableIdentifierValue = [];
        TraceTableLabel = [];
        TraceTableFunctionCall = [];
        TraceTableFunctionReturn = [];

        ProfileCount = [];
        ProfileTotal = [];

        FunctionTable = new FunctionTable(this)
        {
            { "__-", new FunctionTableEntry(this, "__-", Subtract, 2, true) },
            { "__#", new FunctionTableEntry(this, "__#", Undefined, 2, false) },
            { "__$", new FunctionTableEntry(this, "__$", CreateImmediateVariableAssociationPattern, 2, true) },
            { "__%", new FunctionTableEntry(this, "__%", Undefined, 2, false) },
            { "__&", new FunctionTableEntry(this, "__&", Undefined, 2, false) },
            { "__*", new FunctionTableEntry(this, "__*", Multiply, 2, true) },
            { "__.", new FunctionTableEntry(this, "__.", CreateConditionalVariableAssociationPattern, 2, true) },
            { "__/", new FunctionTableEntry(this, "__/", Divide, 2, true) },
            { "__?", new FunctionTableEntry(this, "__?", PatternMatch, 2, true) },
            { "__@", new FunctionTableEntry(this, "__@", Undefined, 2, false) },
            { "__^", new FunctionTableEntry(this, "__^", Power, 2, true) },
            { "__|", new FunctionTableEntry(this, "__|", CreateAlternatePattern, 2, true) },
            { "__~", new FunctionTableEntry(this, "__~", Undefined, 2, false) },
            { "__+", new FunctionTableEntry(this, "__+", Add, 2, true) },
            { "___", new FunctionTableEntry(this, "___", CreateConcatenatePattern, 2, true) },

            { "_-", new FunctionTableEntry(this, "_-", UnaryMinus, 1, true) },
            { "_!", new FunctionTableEntry(this, "_!", Undefined, 1, false) },
            { "_#", new FunctionTableEntry(this, "_#", Undefined, 1, false) },
            { "_$", new FunctionTableEntry(this, "_$", Indirection, 1, true) },
            { "_%", new FunctionTableEntry(this, "_%", Undefined, 1, false) },
            { "_&", new FunctionTableEntry(this, "_&", Ampersand, 1, true) },
            { "_.", new FunctionTableEntry(this, "_.", Name, 1, true) },
            { "_/", new FunctionTableEntry(this, "_/", Undefined, 1, false) },
            { "_?", new FunctionTableEntry(this, "_?", Interrogation, 0, true) },
            { "_@", new FunctionTableEntry(this, "_@", CreateAtPattern, 1, true) },
            { "_|", new FunctionTableEntry(this, "_|", Undefined, 1, false) },
            { "_~", new FunctionTableEntry(this, "_~", Negation, 0, true) },
            { "_+", new FunctionTableEntry(this, "_+", UnaryPlus, 1, true) },
            { "_=", new FunctionTableEntry(this, "_=", Undefined, 1, false) },

            { "any", new FunctionTableEntry(this, "any", CreateAnyPattern, 1, true)},
            { "apply", new FunctionTableEntry(this, "apply", Apply, -1, true)},
            { "arbno", new FunctionTableEntry(this, "arbno", CreateArbNoPattern, 1, true)},
            { "array", new FunctionTableEntry(this, "array", CreateArray, 2, true)},
            { "arg", new FunctionTableEntry(this, "arg", Arg, 2, true)},
            { "atan", new FunctionTableEntry(this, "atan", Atan, 1, true) },
            { "backspace", new FunctionTableEntry(this, "backspace", BackspaceFile, 1, true) },
            { "break", new FunctionTableEntry(this, "break", CreateBreakPattern, 1, true)},
            { "breakx", new FunctionTableEntry(this, "breakx", CreateBreakXPattern, 1, true)},
            { "char", new FunctionTableEntry(this, "char", Char, 1, true) },
            { "chop", new FunctionTableEntry(this, "chop", Chop, 1, true) },
            { "clear", new FunctionTableEntry(this, "clear", ReinitializeVariables, 1, true)},
            { "code", new FunctionTableEntry(this, "code", CreateCode, 1, true)},
            { "copy", new FunctionTableEntry(this, "copy", Copy, 1, true)},
            { "collect", new FunctionTableEntry(this, "collect", GarbageCollect, 1, true)},
            { "concat", new FunctionTableEntry(this, "concat", CreateConcatenatePattern, 2, true)},
            { "convert", new FunctionTableEntry(this, "convert", Convert, 2, true)},
            { "cos", new FunctionTableEntry(this, "cos", Cos, 1, true) },
            { "data", new FunctionTableEntry(this, "data", ProgramDefinedData, 1, true)},
            { "date", new FunctionTableEntry(this, "date", Date, 0, true)},
            { "datatype", new FunctionTableEntry(this, "datatype", DataType, 1, true)},
            { "define", new FunctionTableEntry(this, "define", CreateProgramDefinedFunction, 2, true)},
            { "detach", new FunctionTableEntry(this, "detach", Detach, 1, true)},
            { "differ", new FunctionTableEntry(this, "differ", Differ, 2, true)},
            { "dump", new FunctionTableEntry(this, "dump", DisplayVariableValues, 2, true)},
            { "dupl", new FunctionTableEntry(this, "dupl", Duplicate, 2, true)},
            { "eject", new FunctionTableEntry(this, "eject", EjectFile, 1, true)},
            { "endfile", new FunctionTableEntry(this, "endfile", EndFile, 1, true)},
            { "eq", new FunctionTableEntry(this, "eq", Eq, 2, true) },
            { "eval", new FunctionTableEntry(this, "eval", Eval, 1, true)},
            { "exit", new FunctionTableEntry(this, "exit", Exit, 1, true) },
            { "exp", new FunctionTableEntry(this, "exp", Exp, 1, true) },
            { "fence", new FunctionTableEntry(this, "fence", CreateFenceFunction, 1, true)},
            { "field", new FunctionTableEntry(this, "field", Field, 2, true)},
            { "ge", new FunctionTableEntry(this, "ge", Ge, 2, true) },
            { "gt", new FunctionTableEntry(this, "gt", Gt, 2, true) },
            { "host", new FunctionTableEntry(this, "host", Host, 1, true) },
            { "ident", new FunctionTableEntry(this, "ident", Ident, 2, true) },
            { "input", new FunctionTableEntry(this, "input", InputFileOpen, 5, true)},
            { "integer", new FunctionTableEntry(this, "integer", Integer, 1, true)},
            { "item", new FunctionTableEntry(this, "item", Item, -1, true)},
            { "le", new FunctionTableEntry(this, "le", Le, 2, true) },
            { "len", new FunctionTableEntry(this, "len", CreateLenPattern, 1, true)},
            { "ln", new FunctionTableEntry(this, "ln", Ln, 1, true) },
            { "load", new FunctionTableEntry(this, "load", LoadExternalFunction, 2, true) },
            { "local", new FunctionTableEntry(this, "local", Local, 2, true)},
            { "lpad", new FunctionTableEntry(this, "lpad", PadLeft, 3, true)},
            { "lt", new FunctionTableEntry(this, "lt", Lt, 2, true) },
            { "ne", new FunctionTableEntry(this, "ne", Ne, 2, true) },
            { "leq", new FunctionTableEntry(this, "leq", LexicalEqual, 2, true) },
            { "lge", new FunctionTableEntry(this, "lge", LexicalGreaterThanOrEqual, 2, true) },
            { "lgt", new FunctionTableEntry(this, "lgt", LexicalGreaterThan, 2, true) },
            { "lle", new FunctionTableEntry(this, "lge", LexicalLessThanOrEqual, 2, true) },
            { "llt", new FunctionTableEntry(this, "llt", LexicalLessThan, 2, true) },
            { "lne", new FunctionTableEntry(this, "lne", LexicalNotEqual, 2, true) },
            { "notany", new FunctionTableEntry(this, "notany", CreateNotAnyPattern, 1, true)},
            { "opsyn", new FunctionTableEntry(this, "opsyn", OperatorSynonym, 3, true) },
            { "output", new FunctionTableEntry(this, "output", OutputFileOpen, 6, true) },
            { "prototype", new FunctionTableEntry(this, "prototype", Prototype, 1, true)},
            { "pos", new FunctionTableEntry(this, "pos", CreatePosPattern, 1, true)},
            { "remdr", new FunctionTableEntry(this, "remdr", Remainder, 2, true) },
            { "replace", new FunctionTableEntry(this, "replace", Replace, 3, true) },
            { "reverse", new FunctionTableEntry(this, "reverse", Reverse, 1, true)},
            { "rewind", new FunctionTableEntry(this, "rewind", Rewind, 1, true)},
            { "rpos", new FunctionTableEntry(this, "rpos", CreateRPosPattern, 1, true)},
            { "rsort", new FunctionTableEntry(this, "rsort", ReverseSort, 2, true)},
            { "rpad", new FunctionTableEntry(this, "rpad", PadRight, 3, true)},
            { "rtab", new FunctionTableEntry(this, "rtab", CreateRTabPattern, 1, true)},
            { "set", new FunctionTableEntry(this, "set", Set, 3, true)},
            { "setexit", new FunctionTableEntry(this, "setexit", SetExit, 1, true)},
            { "sin", new FunctionTableEntry(this, "sin", Sin, 1, true) },
            { "size", new FunctionTableEntry(this, "size", Size, 1, true)},
            { "sort", new FunctionTableEntry(this, "sort", Sort, 2, true)},
            { "span", new FunctionTableEntry(this, "span", CreateSpanPattern, 1, true)},
            { "sqrt", new FunctionTableEntry(this, "sqrt", Sqrt, 1, true) },
            { "stoptr", new FunctionTableEntry(this, "stoptr", StopTrace, 2, true) },
            { "substr", new FunctionTableEntry(this, "substr", Substring, 3, true)},
            { "tab", new FunctionTableEntry(this, "tab", CreateTabPattern, 1, true)},
            { "table", new FunctionTableEntry(this, "table", CreateTable, 3, true)},
            { "tan", new FunctionTableEntry(this, "tan", Tan, 1, true)},
            { "time", new FunctionTableEntry(this, "time", Time, 0, true)},
            { "trace", new FunctionTableEntry(this, "trace", Trace, 4, true)},
            { "trim", new FunctionTableEntry(this, "trim", Trim, 1, true)},
            { "unload", new FunctionTableEntry(this, "unload", UnloadExternalFunction, 1, true) },

            { "ANY", new FunctionTableEntry(this, "any", CreateAnyPattern, 1,true)},
            { "APPLY", new FunctionTableEntry(this, "apply", Apply, -1,true)},
            { "ARBNO", new FunctionTableEntry(this, "arbno", CreateArbNoPattern, 1,true)},
            { "ARRAY", new FunctionTableEntry(this, "array", CreateArray, 2,  true)},
            { "ARG", new FunctionTableEntry(this, "arg", Arg, 2,true)},
            { "ATAN", new FunctionTableEntry(this, "atan", Atan, 1,  true) },
            { "BACKSPACE", new FunctionTableEntry(this, "backspace", BackspaceFile, 1,  true) },
            { "BREAK", new FunctionTableEntry(this, "break", CreateBreakPattern, 1,true)},
            { "BREAKX", new FunctionTableEntry(this, "breakx", CreateBreakXPattern, 1,true)},
            { "CHAR", new FunctionTableEntry(this, "char", Char, 1,  true) },
            { "CHOP", new FunctionTableEntry(this, "chop", Chop, 1,  true) },
            { "CLEAR", new FunctionTableEntry(this, "clear", ReinitializeVariables, 1,  true)},
            { "CODE", new FunctionTableEntry(this, "code", CreateCode, 1,  true)},
            { "COPY", new FunctionTableEntry(this, "copy", Copy, 1,  true)},
            { "COLLECT", new FunctionTableEntry(this, "collect", GarbageCollect, 1,  true)},
            { "CONCAT", new FunctionTableEntry(this, "concat", CreateConcatenatePattern,2,  true)},
            { "CONVERT", new FunctionTableEntry(this, "convert", Convert, 2,true)},
            { "COS", new FunctionTableEntry(this, "cos", Cos, 1,  true) },
            { "DATA", new FunctionTableEntry(this, "data", ProgramDefinedData, 1,  true)},
            { "DATE", new FunctionTableEntry(this, "date", Date, 0,  true)},
            { "DATATYPE", new FunctionTableEntry(this, "datatype", DataType, 1,true)},
            { "DEFINE", new FunctionTableEntry(this, "define", CreateProgramDefinedFunction, 2,  true)},
            { "DETACH", new FunctionTableEntry(this, "detach", Detach, 1,  true)},
            { "DIFFER", new FunctionTableEntry(this, "differ", Differ, 2,  true)},
            { "DUMP", new FunctionTableEntry(this, "dump", DisplayVariableValues, 2,  true)},
            { "DUPL", new FunctionTableEntry(this, "dupl", Duplicate, 2,  true)},
            { "EJECT", new FunctionTableEntry(this, "eject", EjectFile, 1,  true)},
            { "ENDFILE", new FunctionTableEntry(this, "endfile", EndFile, 1,  true)},
            { "EQ", new FunctionTableEntry(this, "eq", Eq, 2,  true) },
            { "EVAL", new FunctionTableEntry(this, "eval", Eval, 1,true)},
            { "EXIT", new FunctionTableEntry(this, "exit", Exit, 1,  true) },
            { "EXP", new FunctionTableEntry(this, "exp", Exp, 1,  true) },
            { "FENCE", new FunctionTableEntry(this, "fence",CreateFenceFunction, 1,true)},
            { "FIELD", new FunctionTableEntry(this, "field", Field, 2,true)},
            { "GE", new FunctionTableEntry(this, "ge", Ge, 2,  true) },
            { "GT", new FunctionTableEntry(this, "gt", Gt, 2,  true) },
            { "HOST", new FunctionTableEntry(this, "host", Host, 1,  true) },
            { "IDENT", new FunctionTableEntry(this, "ident", Ident, 2,  true) },
            { "INPUT", new FunctionTableEntry(this, "input", InputFileOpen, 5,  true)},
            { "INTEGER", new FunctionTableEntry(this, "integer", Integer, 1,  true)},
            { "ITEM", new FunctionTableEntry(this, "item", Item, -1,  true)},
            { "LE", new FunctionTableEntry(this, "le", Le, 2,  true) },
            { "LEN", new FunctionTableEntry(this, "len", CreateLenPattern, 1,true)},
            { "LN", new FunctionTableEntry(this, "ln", Ln, 1,  true) },
            { "LOAD", new FunctionTableEntry(this, "load", LoadExternalFunction, 2,  true) },
            { "LOCAL", new FunctionTableEntry(this, "local", Local, 2,true)},
            { "LPAD", new FunctionTableEntry(this, "lpad", PadLeft, 3,true)},
            { "LT", new FunctionTableEntry(this, "lt", Lt, 2,  true) },
            { "NE", new FunctionTableEntry(this, "ne", Ne, 2,  true) },
            { "LEQ", new FunctionTableEntry(this, "leq", LexicalEqual, 2,  true) },
            { "LGE", new FunctionTableEntry(this, "lge", LexicalGreaterThanOrEqual, 2,  true) },
            { "LGT", new FunctionTableEntry(this, "lgt", LexicalGreaterThan, 2,  true) },
            { "LLE", new FunctionTableEntry(this, "lge", LexicalLessThanOrEqual, 2,  true) },
            { "LLT", new FunctionTableEntry(this, "llt", LexicalLessThan, 2,  true) },
            { "LNE", new FunctionTableEntry(this, "lne", LexicalNotEqual, 2,  true) },
            { "NOTANY", new FunctionTableEntry(this, "notany", CreateNotAnyPattern, 1,true)},
            { "OPSYN", new FunctionTableEntry(this, "opsyn", OperatorSynonym, 3,  true) },
            { "OUTPUT", new FunctionTableEntry(this, "output", OutputFileOpen, 6,  true) },
            { "PROTOTYPE", new FunctionTableEntry(this, "prototype", Prototype, 1,true)},
            { "POS", new FunctionTableEntry(this, "pos", CreatePosPattern, 1,true)},
            { "REMDR", new FunctionTableEntry(this, "remdr", Remainder, 2,  true) },
            { "REPLACE", new FunctionTableEntry(this, "replace", Replace, 3,  true) },
            { "REVERSE", new FunctionTableEntry(this, "reverse", Reverse, 1,  true)},
            { "REWIND", new FunctionTableEntry(this, "rewind", Rewind, 1,  true)},
            { "RPOS", new FunctionTableEntry(this, "rpos", CreateRPosPattern, 1,true)},
            { "RSORT", new FunctionTableEntry(this, "rsort", ReverseSort, 2,true)},
            { "RPAD", new FunctionTableEntry(this, "rpad", PadRight, 3,true)},
            { "RTAB", new FunctionTableEntry(this, "rtab", CreateRTabPattern, 1,true)},
            { "SET", new FunctionTableEntry(this, "set", Set, 3, true)},
            { "SETEXIT", new FunctionTableEntry(this, "setexit", SetExit, 1, true)},
            { "SIN", new FunctionTableEntry(this, "sin", Sin, 1,  true) },
            { "SIZE", new FunctionTableEntry(this, "size", Size, 1,true)},
            { "VALUE", new FunctionTableEntry(this, "value", Value, 1, true)},
            { "SORT", new FunctionTableEntry(this, "sort", Sort, 2,true)},
            { "SPAN", new FunctionTableEntry(this, "span", CreateSpanPattern, 1,true)},
            { "SQRT", new FunctionTableEntry(this, "sqrt", Sqrt, 1,  true) },
            { "STOPTR", new FunctionTableEntry(this, "stoptr", StopTrace, 2,  true) },
            { "SUBSTR", new FunctionTableEntry(this, "substr", Substring, 3,true)},
            { "TAB", new FunctionTableEntry(this, "tab", CreateTabPattern, 1,true)},
            { "TABLE", new FunctionTableEntry(this, "table", CreateTable, 3,  true)},
            { "TAN", new FunctionTableEntry(this, "tan", Tan, 1,true)},
            { "TIME", new FunctionTableEntry(this, "time", Time, 0, true)},
            { "TRACE", new FunctionTableEntry(this, "trace", Trace, 4, true)},
            { "TRIM", new FunctionTableEntry(this, "trim", Trim, 1, true)},
            { "UNLOAD", new FunctionTableEntry(this, "unload", UnloadExternalFunction, 1,  true) }
        };

        var alphabet = Enumerable.Range(0, 256).Select(i => (char)i).ToArray();
        AmpAlphabet = "";
        foreach (var letter in alphabet)
            AmpAlphabet += letter;
        // SPITBOL MINIMAL: &lcase and &ucase are exactly 26 ASCII letters (sbl.min: lcase/ucase dac 26)
        AmpLowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
        AmpUpperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        IdentifierTable = new IdentifierTable(this)
        {
            {"abort", new PatternVar(new AbortPattern(), "abort", true, true)},
            {"arb", new PatternVar(ArbPattern.Structure(), "arb", true , true)},
            {"bal", new PatternVar(BalPattern.Structure(), "bal", true, true)},
            {"fail", new PatternVar(new FailPattern(), "fail", true , true)},
            {"fence", new PatternVar(new AlternatePattern(new NullPattern(), new AbortPattern()), "fence", true , true)},
            {"input", new StringVar("","input")},
            {"null", new PatternVar(new NullPattern(),"null", true , true)},
            {"output", new StringVar("","output")},
            {"rem", new PatternVar(new RemPattern(), "rem", true, true)},
            {"succeed", new PatternVar(new SucceedPattern(), "succeed", true , true)},
            {"terminal", new StringVar("", "terminal")},

            {"ABORT", new PatternVar(new AbortPattern(), "ABORT", true, true)},
            {"ARB", new PatternVar(ArbPattern.Structure(), "ARB", true , true)},
            {"BAL", new PatternVar(BalPattern.Structure(), "BAL", true, true)},
            {"FAIL", new PatternVar(new FailPattern(), "FAIL", true , true)},
            {"FENCE", new PatternVar(new AlternatePattern(new NullPattern(), new AbortPattern()), "FENCE", true , true)},
            {"INPUT", new StringVar("","INPUT")},
            {"NULL", new PatternVar(new NullPattern(),"NULL", true , true)},
            {"OUTPUT", new StringVar("","OUTPUT")},
            {"REM", new PatternVar(new RemPattern(), "REM", true, true)},
            {"SUCCEED", new PatternVar(new SucceedPattern(), "SUCCEED", true , true)},
            {"TERMINAL", new StringVar("", "TERMINAL")}
        };

        KeywordTable = new()
        {
            // Protected keywords

            { "&ABORT", HandleAbort },
            { "&ALPHABET", HandleAlphabet },
            { "&ARB", HandleArb },
            { "&BAL", HandleBal },
            { "&FAIL", HandleFail },
            { "&FENCE", HandleFence },
            { "&FILE", HandleFile },
            { "&FNCLEVEL", HandleFncLevel },
            { "&LASTFILE", HandleLastFile },
            { "&LASTLINE", HandleLastLine },
            { "&LASTNO", HandleLastNo },
            { "&LCASE", HandleLCase },
            { "&LINE", HandleLine },
            { "&REM", HandleRem },
            { "&RTNTYPE", HandleRtnType },
            { "&STCOUNT", HandleStCount },
            { "&STNO", HandleStNo },
            { "&SUCCEED", HandleSucceed },
            { "&UCASE", HandleUCase },
            { "&abort", HandleAbort },
            { "&alphabet", HandleAlphabet },
            { "&arb", HandleArb },
            { "&bal", HandleBal },
            { "&fail", HandleFail },
            { "&fence", HandleFence },
            { "&file", HandleFile },
            { "&fnclevel", HandleFncLevel },
            { "&lastfile", HandleLastFile },
            { "&lastline", HandleLastLine },
            { "&lastno", HandleLastNo },
            { "&lcase", HandleLCase },
            { "&line", HandleLine },
            { "&rem", HandleRem },
            { "&rtntype", HandleRtnType },
            { "&stcount", HandleStCount },
            { "&stno", HandleStNo },
            { "&succeed", HandleSucceed },
            { "&ucase", HandleUCase },

            // Unprotected keywords

            { "&ABEND", HandleAbend },
            { "&ANCHOR", HandleAnchor },
            { "&CASE", HandleCase },
            { "&CODE", HandleCode },
            { "&COMPARE", HandleCompare },
            { "&DUMP", HandleDump },
            { "&ERRLIMIT", HandleErrLimit },
            { "&ERRTEXT", HandleErrText },
            { "&ERRTYPE", HandleErrType },
            { "&FTRACE", HandleFTrace },
            { "&FULLSCAN", HandleFullScan },
            { "&INPUT", HandleInput },
            { "&MAXLNGTH", HandleMaxLength },
            { "&OUTPUT", HandleOutput },
            { "&PROFILE", HandleProfile },
            { "&STLIMIT", HandleStatementLimit },
            { "&TRACE", HandleTrace },
            { "&TRIM", HandleTrim },
            { "&abend", HandleAbend },
            { "&anchor", HandleAnchor },
            { "&case", HandleCase },
            { "&code", HandleCode },
            { "&compare", HandleCompare },
            { "&dump", HandleDump },
            { "&errlimit", HandleErrLimit },
            { "&errtext", HandleErrText },
            { "&errtype", HandleErrType },
            { "&ftrace", HandleFTrace },
            { "&fullscan", HandleFullScan },
            { "&input", HandleInput },
            { "&maxlngth", HandleMaxLength },
            { "&output", HandleOutput },
            { "&profile", HandleProfile },
            { "&stlimit", HandleStatementLimit },
            { "&trace", HandleTrace },
            { "&trim", HandleTrim },
        };

        IdentifierTable["output"].OutputChannel = "+console-output";
        IdentifierTable["terminal"].OutputChannel = "+terminal-output";
        IdentifierTable["input"].InputChannel = "+console-input";
        IdentifierTable["terminal"].InputChannel = "+terminal-input";
        IdentifierTable["OUTPUT"].OutputChannel = "+console-output";
        IdentifierTable["TERMINAL"].OutputChannel = "+terminal-output";
        IdentifierTable["INPUT"].InputChannel = "+console-input";
        IdentifierTable["TERMINAL"].InputChannel = "+terminal-input";

        // xncbp: fire any unfired shutdown callbacks when the process exits.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => FireAllNativeCallbacks();
    }
}