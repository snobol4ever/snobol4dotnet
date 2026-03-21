namespace Snobol4.Common;

// Command Line Options
// - Options are case-sensitive
// - Options must precede file names
// - Options cannot be concatenated (e.g. -lcx) [DIFFERS FROM ORIGINAL SPITBOL]
// - Parameters for command line options must follow the option without an
//   intervening space (e.g. -o=filename or -o:filename) [DIFFERS FROM ORIGINAL SPITBOL]
// - If the parameter has spaces, it must be enclosed in double quotes (e.g. -o="string data")
// - Numeric arguments accept k/m suffixes: 8k=8192, 8m=8388608
//
// -a   equivalent to -l -c -x
// -b   SuppressSignOnMessage
// -c   ShowCompilerStatistics
// -cs  WriteCSharpCode [NEW. DIFFERS FROM ORIGINAL SPITBOL]
// -dN  HeapMaxBytes (default 64m) — recorded; .NET GC manages heap
// -e   ErrorsToStdout — errors/trace go to stdout instead of stderr
// -f   CaseFolding OFF
// -F   CaseFolding ON
// -gN  LinesPerPage for listings (default 60)
// -h   SuppressListingHeader
// -iN  HeapIncrementBytes (default 128k) — recorded; .NET GC expands automatically
// -j   [UNASSIGNED]
// -k   StopOnRuntimeError
// -l   ShowListing
// -mN  MaxObjectBytes → initialises &MAXLNGTH (default 4m)
// -n   SuppressExecution
// -o   Redirect listing/errors to file
// -p   PrinterListing — wide titles for printer
// -q   [UNASSIGNED]
// -r   InputAfterEndStatement
// -sN  StackSizeBytes (default 32k)
// -tN  PageWidth for listings (default 120)
// -u   Host Parameter string (HOST(0))
// -v   Generate Debug Symbols
// -w   WriteDll — create load module after compilation
// -x   ShowExecutionStatistics
// -y   WriteSpx — create save file (.spx) after compilation
// -z   FormFeedListing — form feeds between pages
// -?   Display manual
// -N=file  ChannelFiles — associate I/O channel N with file

public partial class Builder
{
    public void ParseCommandLine(string[] commandLine)
    {
        var commandMode = true;
        var count = 0;
        var hostParameterIsNext = false;

        // Default value of HOST(0) is the concatenationof all command line arguments.
        BuildOptions.HostParameter = string.Join(" ", commandLine);

        foreach (var arg in commandLine)
        {
            if (hostParameterIsNext)
            {
                BuildOptions.HostParameter = arg;
                hostParameterIsNext = false;
                continue;
            }

            if (arg == "-u")
            {
                hostParameterIsNext = true;
                continue;
            }

            if (commandMode && arg[0] == '-' && arg.Length > 1)
            {
                ArgumentSwitch(arg);
                continue;
            }

            commandMode = false;
            FilesToCompile.Add(arg);
        }

        if (FilesToCompile.Count > 0)
        {
            count += FilesToCompile.Count;
            Arguments = commandLine[count..];
            return;
        }

        DisplayManual();
    }

    private void ArgumentSwitch(string command)
    {
        // Numeric channel association: -N=file or -N:file where N is a positive integer
        // e.g. -23=infile.dat[–r10]  →  ChannelFiles[23] = "infile.dat[–r10]"
        if (command.Length >= 3)
        {
            var sep = command.IndexOfAny(['=', ':'], 2);
            if (sep >= 2 && int.TryParse(command[1..sep], out var channel) && channel >= 0)
            {
                BuildOptions.ChannelFiles[channel] = command[(sep + 1)..];
                return;
            }
        }

        // Three-char prefix check for -cs before two-char dispatch
        var prefix3 = command.Length >= 3 ? command[..3] : "";
        if (prefix3 == "-cs") { BuildOptions.WriteCSharpCode = true; return; }

        var prefix2 = command.Length >= 2 ? command[..2] : command;

        switch (prefix2)
        {
            case "-a":
                BuildOptions.ShowCompilerStatistics  = true;
                BuildOptions.ShowExecutionStatistics = true;
                BuildOptions.ShowListing             = true;
                break;

            case "-b":
                BuildOptions.SuppressSignOnMessage = true;
                break;

            case "-c":
                BuildOptions.ShowCompilerStatistics = true;
                break;

            case "-d":
                if (TryParseNumericArg(command, 2, out var heapMax))
                    BuildOptions.HeapMaxBytes = heapMax;
                else
                    ReportBadArg(command);
                break;

            case "-e":
                BuildOptions.ErrorsToStdout = true;
                break;

            case "-F":
                BuildOptions.CaseFolding = true;
                break;

            case "-f":
                BuildOptions.CaseFolding = false;
                break;

            case "-g":
                if (TryParseNumericArg(command, 2, out var lpp))
                    BuildOptions.LinesPerPage = (int)lpp;
                else
                    ReportBadArg(command);
                break;

            case "-h":
                BuildOptions.SuppressListingHeader = true;
                break;

            case "-i":
                if (TryParseNumericArg(command, 2, out var heapInc))
                    BuildOptions.HeapIncrementBytes = heapInc;
                else
                    ReportBadArg(command);
                break;

            case "-k":
                BuildOptions.StopOnRuntimeError = true;
                break;

            case "-l":
                BuildOptions.ShowListing = true;
                break;

            case "-m":
                if (TryParseNumericArg(command, 2, out var maxObj))
                    BuildOptions.MaxObjectBytes = maxObj;
                else
                    ReportBadArg(command);
                break;

            case "-n":
                BuildOptions.SuppressExecution = true;
                break;

            case "-o":
                var oArg = ExtractStringArg(command, 2);
                if (oArg is not null)
                {
                    BuildOptions.ListFileName = oArg;
                    if (Path.GetExtension(BuildOptions.ListFileName) == "")
                        BuildOptions.ListFileName += ".lst";
                }
                else
                {
                    ReportBadArg(command);
                }
                break;

            case "-p":
                BuildOptions.PrinterListing = true;
                BuildOptions.ShowListing    = true;
                break;

            case "-r":
                BuildOptions.InputAfterEndStatement = true;
                break;

            case "-s":
                if (TryParseNumericArg(command, 2, out var stack))
                    BuildOptions.StackSizeBytes = (int)Math.Min(stack, int.MaxValue);
                else
                    ReportBadArg(command);
                break;

            case "-t":
                if (TryParseNumericArg(command, 2, out var pw))
                    BuildOptions.PageWidth = (int)pw;
                else
                    ReportBadArg(command);
                break;

            case "-v":
                BuildOptions.GenerateDebugSymbols = true;
                break;

            case "-w":
                BuildOptions.WriteDll = true;
                break;

            case "-x":
                BuildOptions.ShowExecutionStatistics = true;
                break;

            case "-y":
                BuildOptions.WriteSpx = true;
                break;

            case "-z":
                BuildOptions.FormFeedListing = true;
                BuildOptions.ShowListing     = true;
                break;

            case "-?":
                DisplayManual();
                break;

            default:
                ReportBadArg(command);
                break;
        }
    }

    /// <summary>
    /// Parses a numeric argument embedded in a switch string starting at <paramref name="start"/>.
    /// Accepts optional '=' or ':' separator. Accepts 'k' (×1024) and 'm' (×1048576) suffixes.
    /// Examples: "-m4m" → 4194304, "-s32k" → 32768, "-g=60" → 60, "-t:120" → 120.
    /// </summary>
    private static bool TryParseNumericArg(string command, int start, out long result)
    {
        result = 0;
        if (start >= command.Length) return false;

        var s = command[start..];
        if (s.Length > 0 && s[0] is '=' or ':') s = s[1..];
        if (s.Length == 0) return false;

        long multiplier = 1;
        var lower = char.ToLowerInvariant(s[^1]);
        if      (lower == 'k') { multiplier = 1024;           s = s[..^1]; }
        else if (lower == 'm') { multiplier = 1024 * 1024;    s = s[..^1]; }

        if (!long.TryParse(s, out var n)) return false;
        result = n * multiplier;
        return true;
    }

    /// <summary>
    /// Extracts a string argument from a switch string starting at <paramref name="start"/>,
    /// accepting optional '=' or ':' separator.
    /// </summary>
    private static string? ExtractStringArg(string command, int start)
    {
        if (start >= command.Length) return null;
        var s = command[start..];
        if (s.Length > 0 && s[0] is '=' or ':') s = s[1..];
        return s.Length > 0 ? s : null;
    }

    private static void ReportBadArg(string command) =>
        Console.Error.WriteLine($"Invalid option or argument: {command}");

    public static void DisplayManual()
    {
        Console.Error.WriteLine("""

                          usage: snobol4 [options] files[.sno, .sbl, .spt]
                          source files are concatenated

                           Compilation/execution:
                           -a equal to -l -c -x                    -b suppress signon message
                           -c compiler statistics                  -cs write c# source file
                           -e errors to stdout (redirectable)      -f don't fold names to upper
                           -F fold names to upper case             -k run despite compile errors
                           -l show listing                         -n suppress execution
                           -r INPUT reads after END statement      -x execution statistics

                           Input/output:
                           -o=file[.lst] listing/error output      -u "host parameter string"
                           -N=file  associate channel N with file

                           Listing format:
                           -gN lines per page (default 60)         -h suppress listing header
                           -p  wide titles for printer             -tN page width (default 120)
                           -z  form feeds between pages

                           Memory:
                           -dN max heap bytes (default 64m)        -iN heap increment (default 128k)
                           -mN max object size → &MAXLNGTH (4m)    -sN stack size (default 32k)

                           Save files:
                           -w  write load module (.dll)            -y  write save file (.spx)

                           Misc:
                           -v  generate debug symbols              -? display this help

                           Numeric arguments accept k/m suffixes: -s32k  -m4m  -d64m

                          option defaults: -F -m4m -s32k -i128k -d64m -g60 -t120

                          """);
    }
}