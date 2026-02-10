using System.Diagnostics;
using System.Globalization;

namespace Snobol4.Common;

public partial class Executive
{
    public bool Failure;
    public Builder Parent;
    public Dictionary<string, int> Labels;
    public IdentifierTable IdentifierTable;
    public Dictionary<string,FunctionTableEntry> FunctionTable;
    public List<long> SourceLineNumbers;
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

    internal Stack<bool> FailureStack;
    internal string ReturnType;
    internal Stack<NameListEntry> AlphaStack;     // Stack for conditional variable assignment
    internal Stack<NameListEntry> BetaStack;      // Stack for conditional variable assignment
    internal List<ArrayVar> IndexedArrays;
    internal List<TableVar> IndexedTables;

    private readonly Stopwatch _timerExecute; // Timer for statistics

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
        Labels = [];
        StreamOutputs = [];
        ReturnType = "";
        SourceCode = [];
        SourceFiles = [];
        SourceLineNumbers = [];
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

        FunctionTable = new Dictionary<string, FunctionTableEntry>
        {
            { "binary-", new FunctionTableEntry("binary-", Subtract, 2,  true) },
            { "binary#", new FunctionTableEntry("binary#", Undefined, 2,  false) },
            { "binary$", new FunctionTableEntry("binary$", CreateImmediateVariableAssociationPattern, 2,  true) },
            { "binary%", new FunctionTableEntry("binary%", Undefined, 2,  false) },
            { "binary&", new FunctionTableEntry("binary&", Undefined, 2,  false) },
            { "binary*", new FunctionTableEntry("binary*", Multiply, 2,  true) },
            { "binary.", new FunctionTableEntry("binary.", CreateConditionalVariableAssociationPattern, 2,  true) },
            { "binary/", new FunctionTableEntry("binary/", Divide, 2,  true) },
            { "binary?", new FunctionTableEntry("binary?", PatternMatch, 2,  true)},
            { "binary@", new FunctionTableEntry("binary@", Undefined, 2,  false) },
            { "binary^", new FunctionTableEntry("binary^", Power, 2,  true) },
            { "binary|", new FunctionTableEntry("binary|", CreateAlternatePattern,2,  true)},
            { "binary~", new FunctionTableEntry("binary~", Undefined, 2,  false) },
            { "binary+", new FunctionTableEntry("binary+", Add, 2,  true) },

            { "unary-", new FunctionTableEntry("unary-", UnaryMinus, 1,  true) },
            { "unary!", new FunctionTableEntry("unary!", Undefined, 1,  false) },
            { "unary#", new FunctionTableEntry("unary#", Undefined, 1,  false) },
            { "unary$", new FunctionTableEntry("unary$", Indirection, 1,  true) },
            { "unary%", new FunctionTableEntry("unary%", Undefined, 1,  false) },
            { "unary&", new FunctionTableEntry("unary&", Ampersand, 1,  true) },
            { "unary.", new FunctionTableEntry("unary.", Name, 1,  true) },
            { "unary/", new FunctionTableEntry("unary/", Undefined, 1,  false) },
            { "unary?", new FunctionTableEntry("unary?", Interrogation, 0,  true) },
            { "unary@", new FunctionTableEntry("unary@", CreateAtPattern, 1,  true) },
            { "unary|", new FunctionTableEntry("unary|", Undefined, 1,  false) },
            { "unary~", new FunctionTableEntry("unary~", Negation, 0,  true) },
            { "unary+", new FunctionTableEntry("unary+", UnaryPlus, 1,  true) },
            { "unary=", new FunctionTableEntry("unary=", Undefined, 1,  false) },

            { "any", new FunctionTableEntry("any", CreateAnyPattern, 1,true)},
            { "apply", new FunctionTableEntry("apply", Apply, -1,true)},
            { "arbno", new FunctionTableEntry("arbno", CreateArbNoPattern, 1,true)},
            { "array", new FunctionTableEntry("array", CreateArray, 2,  true)},
            { "arg", new FunctionTableEntry("arg", Arg, 2,true)},
            { "atan", new FunctionTableEntry("atan", Atan, 1,  true) },
            { "backspace", new FunctionTableEntry("backspace", BackspaceFile, 1,  true) },
            { "break", new FunctionTableEntry("break", CreateBreakPattern, 1,true)},
            { "breakx", new FunctionTableEntry("breakx", CreateBreakXPattern, 1,true)},
            { "char", new FunctionTableEntry("char", Char, 1,  true) },
            { "chop", new FunctionTableEntry("chop", Chop, 1,  true) },
            { "clear", new FunctionTableEntry("clear", ReinitializeVariables, 1,  true)},
            { "code", new FunctionTableEntry("code", CreateCode, 1,  true)},
            { "copy", new FunctionTableEntry("copy", Copy, 1,  true)},
            { "collect", new FunctionTableEntry("collect", GarbageCollect, 1,  true)},
            { "concat", new FunctionTableEntry("concat", CreateConcatenatePattern,2,  true)},
            { "convert", new FunctionTableEntry("convert", Convert, 2,true)},
            { "cos", new FunctionTableEntry("cos", Cos, 1,  true) },
            { "data", new FunctionTableEntry("data", ProgramDefinedData, 1,  true)},
            { "date", new FunctionTableEntry("date", Date, 0,  true)},
            { "datatype", new FunctionTableEntry("datatype", DataType, 1,true)},
            { "define", new FunctionTableEntry("define", CreateProgramDefinedFunction, 2,  true)},
            { "detach", new FunctionTableEntry("detach", Detach, 1,  true)},
            { "differ", new FunctionTableEntry("differ", Differ, 2,  true)},
            { "dump", new FunctionTableEntry("dump", DisplayVariableValues, 2,  true)},
            { "dupl", new FunctionTableEntry("dupl", Duplicate, 2,  true)},
            { "eject", new FunctionTableEntry("eject", EjectFile, 1,  true)},
            { "endfile", new FunctionTableEntry("endfile", EndFile, 1,  true)},
            { "eq", new FunctionTableEntry("eq", Eq, 2,  true) },
            { "eval", new FunctionTableEntry("eval", Eval, 1,true)},
            { "exit", new FunctionTableEntry("exit", Exit, 1,  true) },
            { "exp", new FunctionTableEntry("exp", Exp, 1,  true) },
            { "fence", new FunctionTableEntry("fence",CreateFenceFunction, 1,true)},
            { "field", new FunctionTableEntry("field", Field, 2,true)},
            { "ge", new FunctionTableEntry("ge", Ge, 2,  true) },
            { "gt", new FunctionTableEntry("gt", Gt, 2,  true) },
            { "host", new FunctionTableEntry("host", Host, 1,  true) },
            { "ident", new FunctionTableEntry("ident", Ident, 2,  true) },
            { "input", new FunctionTableEntry("input", InputFileOpen, 5,  true)},
            { "integer", new FunctionTableEntry("integer", Integer, 1,  true)},
            { "item", new FunctionTableEntry("item", Item, -1,  true)},
            { "le", new FunctionTableEntry("le", Le, 2,  true) },
            { "len", new FunctionTableEntry("len", CreateLenPattern, 1,true)},
            { "ln", new FunctionTableEntry("ln", Ln, 1,  true) },
            { "load", new FunctionTableEntry("load", LoadExternalFunction, 2,  true) },
            { "local", new FunctionTableEntry("local", Local, 2,true)},
            { "lpad", new FunctionTableEntry("lpad", PadLeft, 3,true)},
            { "lt", new FunctionTableEntry("lt", Lt, 2,  true) },
            { "ne", new FunctionTableEntry("ne", Ne, 2,  true) },
            { "leq", new FunctionTableEntry("leq", LexicalEqual, 2,  true) },
            { "lge", new FunctionTableEntry("lge", LexicalGreaterThanOrEqual, 2,  true) },
            { "lgt", new FunctionTableEntry("lgt", LexicalGreaterThan, 2,  true) },
            { "lle", new FunctionTableEntry("lge", LexicalLessThanOrEqual, 2,  true) },
            { "llt", new FunctionTableEntry("llt", LexicalLessThan, 2,  true) },
            { "lne", new FunctionTableEntry("lne", LexicalNotEqual, 2,  true) },
            { "notany", new FunctionTableEntry("notany", CreateNotAnyPattern, 1,true)},
            { "opsyn", new FunctionTableEntry("opsyn", OperatorSynonym, 3,  true) },
            { "output", new FunctionTableEntry("output", OutputFileOpen, 5,  true) },
            { "prototype", new FunctionTableEntry("prototype", Prototype, 1,true)},
            { "pos", new FunctionTableEntry("pos", CreatePosPattern, 1,true)},
            { "remdr", new FunctionTableEntry("remdr", Remainder, 2,  true) },
            { "replace", new FunctionTableEntry("replace", Replace, 3,  true) },
            { "reverse", new FunctionTableEntry("reverse", Reverse, 1,  true)},
            { "rewind", new FunctionTableEntry("rewind", Rewind, 1,  true)},
            { "rpos", new FunctionTableEntry("rpos", CreateRPosPattern, 1,true)},
            { "rsort", new FunctionTableEntry("rsort", ReverseSort, 2,true)},
            { "rpad", new FunctionTableEntry("rpad", PadRight, 3,true)},
            { "rtab", new FunctionTableEntry("rtab", CreateRTabPattern, 1,true)},
            { "set", new FunctionTableEntry("set", Set, 3, true)},
            { "setexit", new FunctionTableEntry("setexit", SetExit, 1, true)},
            { "sin", new FunctionTableEntry("sin", Sin, 1,  true) },
            { "size", new FunctionTableEntry("size", Size, 1,true)},
            { "sort", new FunctionTableEntry("sort", Sort, 2,true)},
            { "span", new FunctionTableEntry("span", CreateSpanPattern, 1,true)},
            { "sqrt", new FunctionTableEntry("sqrt", Sqrt, 1,  true) },
            { "stoptr", new FunctionTableEntry("stoptr", StopTrace, 2,  true) },
            { "substr", new FunctionTableEntry("substr", Substring, 3,true)},
            { "tab", new FunctionTableEntry("tab", CreateTabPattern, 1,true)},
            { "table", new FunctionTableEntry("table", CreateTable, 3,  true)},
            { "tan", new FunctionTableEntry("tan", Tan, 1,true)},
            { "time", new FunctionTableEntry("time", Time, 0, true)},
            { "trace", new FunctionTableEntry("trace", Trace, 4, true)},
            { "trim", new FunctionTableEntry("trim", Trim, 1, true)},
            { "unload", new FunctionTableEntry("unload", UnloadExternalFunction, 1,  true) },

            { "ANY", new FunctionTableEntry("any", CreateAnyPattern, 1,true)},
            { "APPLY", new FunctionTableEntry("apply", Apply, -1,true)},
            { "ARBNO", new FunctionTableEntry("arbno", CreateArbNoPattern, 1,true)},
            { "ARRAY", new FunctionTableEntry("array", CreateArray, 2,  true)},
            { "ARG", new FunctionTableEntry("arg", Arg, 2,true)},
            { "ATAN", new FunctionTableEntry("atan", Atan, 1,  true) },
            { "BACKSPACE", new FunctionTableEntry("backspace", BackspaceFile, 1,  true) },
            { "BREAK", new FunctionTableEntry("break", CreateBreakPattern, 1,true)},
            { "BREAKX", new FunctionTableEntry("breakx", CreateBreakXPattern, 1,true)},
            { "CHAR", new FunctionTableEntry("char", Char, 1,  true) },
            { "CHOP", new FunctionTableEntry("chop", Chop, 1,  true) },
            { "CLEAR", new FunctionTableEntry("clear", ReinitializeVariables, 1,  true)},
            { "CODE", new FunctionTableEntry("code", CreateCode, 1,  true)},
            { "COPY", new FunctionTableEntry("copy", Copy, 1,  true)},
            { "COLLECT", new FunctionTableEntry("collect", GarbageCollect, 1,  true)},
            { "CONCAT", new FunctionTableEntry("concat", CreateConcatenatePattern,2,  true)},
            { "CONVERT", new FunctionTableEntry("convert", Convert, 2,true)},
            { "COS", new FunctionTableEntry("cos", Cos, 1,  true) },
            { "DATA", new FunctionTableEntry("data", ProgramDefinedData, 1,  true)},
            { "DATE", new FunctionTableEntry("date", Date, 0,  true)},
            { "DATATYPE", new FunctionTableEntry("datatype", DataType, 1,true)},
            { "DEFINE", new FunctionTableEntry("define", CreateProgramDefinedFunction, 2,  true)},
            { "DETACH", new FunctionTableEntry("detach", Detach, 1,  true)},
            { "DIFFER", new FunctionTableEntry("differ", Differ, 2,  true)},
            { "DUMP", new FunctionTableEntry("dump", DisplayVariableValues, 2,  true)},
            { "DUPL", new FunctionTableEntry("dupl", Duplicate, 2,  true)},
            { "EJECT", new FunctionTableEntry("eject", EjectFile, 1,  true)},
            { "ENDFILE", new FunctionTableEntry("endfile", EndFile, 1,  true)},
            { "EQ", new FunctionTableEntry("eq", Eq, 2,  true) },
            { "EVAL", new FunctionTableEntry("eval", Eval, 1,true)},
            { "EXIT", new FunctionTableEntry("exit", Exit, 1,  true) },
            { "EXP", new FunctionTableEntry("exp", Exp, 1,  true) },
            { "FENCE", new FunctionTableEntry("fence",CreateFenceFunction, 1,true)},
            { "FIELD", new FunctionTableEntry("field", Field, 2,true)},
            { "GE", new FunctionTableEntry("ge", Ge, 2,  true) },
            { "GT", new FunctionTableEntry("gt", Gt, 2,  true) },
            { "HOST", new FunctionTableEntry("host", Host, 1,  true) },
            { "IDENT", new FunctionTableEntry("ident", Ident, 2,  true) },
            { "INPUT", new FunctionTableEntry("input", InputFileOpen, 5,  true)},
            { "INTEGER", new FunctionTableEntry("integer", Integer, 1,  true)},
            { "ITEM", new FunctionTableEntry("item", Item, -1,  true)},
            { "LE", new FunctionTableEntry("le", Le, 2,  true) },
            { "LEN", new FunctionTableEntry("len", CreateLenPattern, 1,true)},
            { "LN", new FunctionTableEntry("ln", Ln, 1,  true) },
            { "LOAD", new FunctionTableEntry("load", LoadExternalFunction, 2,  true) },
            { "LOCAL", new FunctionTableEntry("local", Local, 2,true)},
            { "LPAD", new FunctionTableEntry("lpad", PadLeft, 3,true)},
            { "LT", new FunctionTableEntry("lt", Lt, 2,  true) },
            { "NE", new FunctionTableEntry("ne", Ne, 2,  true) },
            { "LEQ", new FunctionTableEntry("leq", LexicalEqual, 2,  true) },
            { "LGE", new FunctionTableEntry("lge", LexicalGreaterThanOrEqual, 2,  true) },
            { "LGT", new FunctionTableEntry("lgt", LexicalGreaterThan, 2,  true) },
            { "LLE", new FunctionTableEntry("lge", LexicalLessThanOrEqual, 2,  true) },
            { "LLT", new FunctionTableEntry("llt", LexicalLessThan, 2,  true) },
            { "LNE", new FunctionTableEntry("lne", LexicalNotEqual, 2,  true) },
            { "NOTANY", new FunctionTableEntry("notany", CreateNotAnyPattern, 1,true)},
            { "OPSYN", new FunctionTableEntry("opsyn", OperatorSynonym, 3,  true) },
            { "OUTPUT", new FunctionTableEntry("output", OutputFileOpen, 5,  true) },
            { "PROTOTYPE", new FunctionTableEntry("prototype", Prototype, 1,true)},
            { "POS", new FunctionTableEntry("pos", CreatePosPattern, 1,true)},
            { "REMDR", new FunctionTableEntry("remdr", Remainder, 2,  true) },
            { "REPLACE", new FunctionTableEntry("replace", Replace, 3,  true) },
            { "REVERSE", new FunctionTableEntry("reverse", Reverse, 1,  true)},
            { "REWIND", new FunctionTableEntry("rewind", Rewind, 1,  true)},
            { "RPOS", new FunctionTableEntry("rpos", CreateRPosPattern, 1,true)},
            { "RSORT", new FunctionTableEntry("rsort", ReverseSort, 2,true)},
            { "RPAD", new FunctionTableEntry("rpad", PadRight, 3,true)},
            { "RTAB", new FunctionTableEntry("rtab", CreateRTabPattern, 1,true)},
            { "SET", new FunctionTableEntry("set", Set, 3, true)},
            { "SETEXIT", new FunctionTableEntry("setexit", SetExit, 1, true)},
            { "SIN", new FunctionTableEntry("sin", Sin, 1,  true) },
            { "SIZE", new FunctionTableEntry("size", Size, 1,true)},
            { "SORT", new FunctionTableEntry("sort", Sort, 2,true)},
            { "SPAN", new FunctionTableEntry("span", CreateSpanPattern, 1,true)},
            { "SQRT", new FunctionTableEntry("sqrt", Sqrt, 1,  true) },
            { "STOPTR", new FunctionTableEntry("stoptr", StopTrace, 2,  true) },
            { "SUBSTR", new FunctionTableEntry("substr", Substring, 3,true)},
            { "TAB", new FunctionTableEntry("tab", CreateTabPattern, 1,true)},
            { "TABLE", new FunctionTableEntry("table", CreateTable, 3,  true)},
            { "TAN", new FunctionTableEntry("tan", Tan, 1,true)},
            { "TIME", new FunctionTableEntry("time", Time, 0, true)},
            { "TRACE", new FunctionTableEntry("trace", Trace, 4, true)},
            { "TRIM", new FunctionTableEntry("trim", Trim, 1, true)},
            { "UNLOAD", new FunctionTableEntry("unload", UnloadExternalFunction, 1,  true) }
        };

        var alphabet = Enumerable.Range(0, 255).Select(i => (char)i).ToArray();
        AmpAlphabet = "";
        AmpLowerCaseLetters = "";
        AmpUpperCaseLetters = "";
        foreach (var letter in alphabet)
        {
            // Use the culture-specific method for case conversion
            AmpAlphabet += letter;
            if (!char.IsLower(letter)) continue;
            AmpLowerCaseLetters += letter;
            AmpUpperCaseLetters += char.ToUpper(letter, CultureInfo.CurrentCulture);
        }

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

            {"ABORT", new PatternVar(new AbortPattern(), "abort", true, true)},
            {"ARB", new PatternVar(ArbPattern.Structure(), "arb", true , true)},
            {"BAL", new PatternVar(BalPattern.Structure(), "bal", true, true)},
            {"FAIL", new PatternVar(new FailPattern(), "fail", true , true)},
            {"FENCE", new PatternVar(new AlternatePattern(new NullPattern(), new AbortPattern()), "fence", true , true)},
            {"INPUT", new StringVar("","input")},
            {"NULL", new PatternVar(new NullPattern(),"null", true , true)},
            {"OUTPUT", new StringVar("","output")},
            {"REM", new PatternVar(new RemPattern(), "rem", true, true)},
            {"SUCCEED", new PatternVar(new SucceedPattern(), "succeed", true , true)},
            {"TERMINAL", new StringVar("", "terminal")}
        };

        KeywordTable = new()
        {
            // Protected keywords

            { "&ABORT", HandleAbort },
            { "&abort", HandleAbort },
            { "&ALPHABET", HandleAlphabet },
            { "&alphabet", HandleAlphabet },
            { "&ARB", HandleArb },
            { "&arb", HandleArb },
            { "&BAL", HandleBal },
            { "&bal", HandleBal },
            { "&FAIL", HandleFail },
            { "&fail", HandleFail },
            { "&FENCE", HandleFence },
            { "&fence", HandleFence },
            { "&FILE", HandleFile },
            { "&file", HandleFile },
            { "&FNCLEVEL", HandleFncLevel },
            { "&fnclevel", HandleFncLevel },
            { "&LASTFILE", HandleLastFile },
            { "&lastfile", HandleLastFile },
            { "&LASTLINE", HandleLastLine },
            { "&lastline", HandleLastLine },
            { "&LASTNO", HandleLastNo },
            { "&lastno", HandleLastNo },
            { "&LCASE", HandleLCase },
            { "&lcase", HandleLCase },
            { "&LINE", HandleLine },
            { "&line", HandleLine },
            { "&REM", HandleRem },
            { "&rem", HandleRem },
            { "&RTNTYPE", HandleRtnType },
            { "&rtntype", HandleRtnType },
            { "&STCOUNT", HandleStCount },
            { "&stcount", HandleStCount },
            { "&STNO", HandleStNo },
            { "&stno", HandleStNo },
            { "&SUCCEED", HandleSucceed },
            { "&succeed", HandleSucceed },
            { "&UCASE", HandleUCase },
            { "&ucase", HandleUCase },

            // Unprotected keywords

            { "&ABEND", HandleAbend },
            { "&abend", HandleAbend },
            { "&ANCHOR", HandleAnchor },
            { "&anchor", HandleAnchor },
            { "&CASE", HandleCase },
            { "&case", HandleCase },
            { "&CODE", HandleCode },
            { "&code", HandleCode },
            { "&COMPARE", HandleCompare },
            { "&compare", HandleCompare },
            { "&DUMP", HandleDump },
            { "&dump", HandleDump },
            { "&ERRLIMIT", HandleErrLimit },
            { "&errlimit", HandleErrLimit },
            { "&ERRTEXT", HandleErrText },
            { "&errtext", HandleErrText },
            { "&ERRTYPE", HandleErrType },
            { "&errtype", HandleErrType },
            { "&FTRACE", HandleFTrace },
            { "&ftrace", HandleFTrace },
            { "&FULLSCAN", HandleFullScan },
            { "&fullscan", HandleFullScan },
            { "&INPUT", HandleInput },
            { "&input", HandleInput },
            { "&PROFILE", HandleProfile },
            { "&profile", HandleProfile },
            { "&STLIMIT", HandleStatementLimit },
            { "&stlimit", HandleStatementLimit },
            { "&TRIM", HandleTrim },
            { "&trim", HandleTrim },
            { "&TRACE", HandleTrace },
            { "&trace", HandleTrace },
        };

        IdentifierTable["output"].OutputChannel = "+console-output";
        IdentifierTable["terminal"].OutputChannel = "+terminal-output";
        IdentifierTable["input"].InputChannel = "+console-input";
        IdentifierTable["terminal"].InputChannel = "+terminal-input";
        IdentifierTable["OUTPUT"].OutputChannel = "+console-output";
        IdentifierTable["TERMINAL"].OutputChannel = "+terminal-output";
        IdentifierTable["INPUT"].InputChannel = "+console-input";
        IdentifierTable["TERMINAL"].InputChannel = "+terminal-input";
    }
}