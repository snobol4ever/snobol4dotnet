

namespace Snobol4.Common;

/// <summary>
/// Configuration options for the SNOBOL4 compiler and runtime.
/// </summary>
public class BuilderOptions
{
    /// <summary>
    /// Gets or sets whether to suppress the sign-on message. Command line: -b
    /// </summary>
    public bool SuppressSignOnMessage { get; set; }

    /// <summary>
    /// Gets or sets whether to show compiler statistics. Command line: -c
    /// </summary>
    public bool ShowCompilerStatistics { get; set; }

    /// <summary>
    /// Gets or sets whether to write C# code. Command line: -cs
    /// </summary>
    public bool WriteCSharpCode { get; set; }

    /// <summary>
    /// Gets or sets whether case folding is enabled. Command line: -F and -f
    /// </summary>
    public bool CaseFolding { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to suppress the listing header. Command line: -h
    /// </summary>
    public bool SuppressListingHeader { get; set; }

    /// <summary>
    /// Gets or sets whether to stop on runtime errors. Command line: -k
    /// </summary>
    public bool StopOnRuntimeError { get; set; }

    /// <summary>
    /// Gets or sets whether to show the listing. Command line: -l
    /// </summary>
    public bool ShowListing { get; set; }

    /// <summary>
    /// Gets or sets whether to suppress execution. Command line: -n
    /// </summary>
    public bool SuppressExecution { get; set; }

    /// <summary>
    /// Gets or sets whether to allow input after the END statement. Command line: -r
    /// </summary>
    public bool InputAfterEndStatement { get; set; }

    /// <summary>
    /// Gets or sets the host parameter. Command line: -u
    /// </summary>
    public string HostParameter { get; set; } = "";

    /// <summary>
    /// Gets or sets whether to generate debug symbols. Command line: -v
    /// </summary>
    public bool GenerateDebugSymbols { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to write a DLL. Command line: -w
    /// </summary>
    public bool WriteDll { get; set; }

    /// <summary>
    /// Gets or sets whether to show execution statistics. Command line: -x
    /// </summary>
    public bool ShowExecutionStatistics { get; set; }

    /// <summary>
    /// Gets or sets whether statement tracing is enabled.
    /// </summary>
    public bool TraceStatements { get; set; }

    /// <summary>
    /// Gets or sets whether to list source code. Controlled by LIST/NOLIST directives.
    /// </summary>
    public bool ListSource { get; set; }

    /// <summary>
    /// Gets or sets whether to error on unhandled failures. Controlled by FAIL/NOFAIL directives.
    /// </summary>
    internal bool ErrorOnUnhandledFail { get; set; }

    /// <summary>
    /// Gets or sets the name of the list file.
    /// </summary>
    public string ListFileName { get; set; } = "";

    /// <summary>
    /// When true, the threaded code compiler runs after Parse() and the
    /// <summary>
    /// When true (default), the threaded execution engine is used.
    /// When false, the legacy Roslyn C#-codegen path is used instead.
    ///
    /// NOT exposed as a command-line flag. Set this in test code only,
    /// to run the Roslyn path for regression comparison against the threaded path.
    /// </summary>
    public bool UseThreadedExecution { get; set; } = true;
}

