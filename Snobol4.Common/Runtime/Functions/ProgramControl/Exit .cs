using System.Diagnostics;

namespace Snobol4.Common;

//"exit first argument is not suitable integer or string" /* 104 */,
//"exit action not available in this implementation" /* 105 */,
//"exit action caused irrecoverable error" /* 106 */,

public partial class Executive
{
    // EXIT(s) ends the run and has the operating system perform the command s, as SPITBOL's does: through the shell (SHELL, else
    // /bin/sh; cmd.exe on Windows), the run's exit status the command's. A first argument that is a number, or a string that reads as
    // one, is an action: a negative one writes a save file (a.spx without a name, the second argument) and ends the run, -4 alone goes
    // on, as sbl does; a real is cut toward zero first, and zero and above (a load module, EXIT(3), included) are error 105. Here the
    // file holds the program, as -w writes it, and not its state: running it starts the program over. A second argument that is no
    // string (an array, a table, a pattern, an expression, code, a data object) is error 288, whatever the first is.
    internal void Exit(List<Var> arguments)
    {
        // the second argument is looked at first, whatever the first is
        if (!arguments[1].Convert(VarType.STRING, out _, out var name, this))
        {
            LogRuntimeException(288);
            return;
        }

        if (ExitAction(arguments[0], out var action))
        {
            if (action >= 0)
            {
                LogRuntimeException(105);
                return;
            }

            // the name is cut at a NUL, as the operating system's call takes it
            var cut = ((string)name).Split('\0')[0];
            var file = cut != "" ? cut : "a.spx";
            if (!Parent.WriteProgramImage(Path.GetFullPath(file), warn: false))
            {
                LogRuntimeException(106);
                return;
            }

            if (action != -4)
            {
                RunLeftAtOnce = true;
                throw new ProgramEndException();
            }
            SystemStack.Push(StringVar.Null());
            return;
        }

        switch (arguments[0])
        {
            case StringVar { Data.Length: > 0 } command:
                RunCommandAndEnd(command.Data);
                return;

            case StringVar:
                LogRuntimeException(105);
                return;

            default:
                LogRuntimeException(104);
                return;
        }
    }

    // The first argument as an action number: an integer, a real cut toward zero, or a string that reads as either
    private bool ExitAction(Var first, out long action)
    {
        action = 0;
        switch (first)
        {
            case IntegerVar integer:
                action = integer.Data;
                return true;
            case RealVar real:
                action = double.IsFinite(real.Data) ? (long)Math.Clamp(Math.Truncate(real.Data), long.MinValue, long.MaxValue) : 0;
                return true;
            case StringVar { Data.Length: > 0 }:
                if (first.Convert(VarType.INTEGER, out _, out var integerValue, this))
                {
                    action = (long)integerValue;
                    return true;
                }

                if (first.Convert(VarType.REAL, out _, out var realValue, this))
                {
                    action = double.IsFinite((double)realValue) ? (long)Math.Clamp(Math.Truncate((double)realValue), long.MinValue, long.MaxValue) : 0;
                    return true;
                }

                return false;
            default:
                return false;
        }
    }

    private void RunCommandAndEnd(string command)
    {
        var windows = OperatingSystem.IsWindows();
        var shell = windows ? "cmd.exe" : Environment.GetEnvironmentVariable("SHELL") is { Length: > 0 } login ? login : "/bin/sh";
        var startInfo = new ProcessStartInfo { FileName = shell, UseShellExecute = false };
        startInfo.ArgumentList.Add(windows ? "/C" : "-c");
        startInfo.ArgumentList.Add(command);
        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                NonExceptionFailure();
                return;
            }

            process.WaitForExit();
            Parent.ExitStatus = process.ExitCode;
        }
        catch (Exception)
        {
            // the system cannot run the command: EXIT fails, as SPITBOL's does
            NonExceptionFailure();
            return;
        }

        RunLeftAtOnce = true;
        throw new ProgramEndException();
    }

    // Set when the run is left at once: by an EXIT that ends it (a save with -3 or the like, or a command) or by a stack overflow. SPITBOL
    // then prints no end-of-run dump and none of the statistics of -x
    internal bool RunLeftAtOnce;
}
