

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

    // ── SPITBOL switches: input/output ──────────────────────────────────────

    /// <summary>
    /// Gets or sets whether errors go to stdout instead of stderr. Command line: -e
    /// Allows: spitbol -e ifiles >trace.dat
    /// </summary>
    public bool ErrorsToStdout { get; set; }

    // ── SPITBOL switches: listing format ────────────────────────────────────

    /// <summary>
    /// Gets or sets lines per page for listings. Command line: -gN (default 60)
    /// </summary>
    public int LinesPerPage { get; set; } = 60;

    /// <summary>
    /// Gets or sets page width in characters for listings. Command line: -tN (default 120)
    /// </summary>
    public int PageWidth { get; set; } = 120;

    /// <summary>
    /// Gets or sets whether to produce a listing with wide titles for printer. Command line: -p
    /// </summary>
    public bool PrinterListing { get; set; }

    /// <summary>
    /// Gets or sets whether to produce a listing with form feeds between pages. Command line: -z
    /// </summary>
    public bool FormFeedListing { get; set; }

    // ── SPITBOL switches: memory control ────────────────────────────────────

    /// <summary>
    /// Gets or sets the maximum dynamic heap size in bytes. Command line: -dN (default 64m)
    /// Under .NET the GC manages the heap; this value is recorded and exposed via HOST().
    /// </summary>
    public long HeapMaxBytes { get; set; } = 64 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the heap increment size in bytes. Command line: -iN (default 128k)
    /// Under .NET the GC expands automatically; recorded for compatibility.
    /// </summary>
    public long HeapIncrementBytes { get; set; } = 128 * 1024;

    /// <summary>
    /// Gets or sets the maximum object size in bytes, initialising &amp;MAXLNGTH. Command line: -mN (default 4m)
    /// </summary>
    public long MaxObjectBytes { get; set; } = 4 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum stack size in bytes. Command line: -sN (default 32k)
    /// Used as the thread stack size when creating the execution thread.
    /// </summary>
    public int StackSizeBytes { get; set; } = 32 * 1024;

    // ── SPITBOL switches: save files ────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether to create a save file (.spx) after compilation. Command line: -y
    /// </summary>
    public bool WriteSpx { get; set; }

    // ── SPITBOL switches: I/O channel pre-association ───────────────────────

    /// <summary>
    /// Gets or sets files pre-associated with I/O channel numbers. Command line: -N=file
    /// Key = channel number N; Value = filename (with optional [options]).
    /// </summary>
    public Dictionary<int, string> ChannelFiles { get; set; } = new();
}

