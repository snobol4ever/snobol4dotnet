

namespace Snobol4.Common;

public class BuilderOptions
{
    public bool SuppressSignOnMessage = false; // -b
    public bool ShowCompilerStatistics = false; // -c
    public bool WriteCSharpCode = false; // -cs
    public bool CaseFolding = true; // -F and -f
    public bool SuppressListingHeader = false; // -h
    public bool StopOnRuntimeError = false; // -k
    public bool ShowListing = false; // -l
    public bool SuppressExecution = false; // -n
    public bool InputAfterEndStatement = false; // -r
    public string HostParameter = ""; // -u
    public bool GenerateDebugSymbols = true; // -v
    public bool WriteDll = false; // -w
    public bool ShowExecutionStatistics = false; // -x
    public bool TraceStatements = false; // True if tracing is on
    public bool ListSource = false; // LIST/NOLIST
    internal bool ErrorOnUnhandledFail = false; // FAIL/NOFAIL
    public string ListFileName = ""; // Name of list file;
}

