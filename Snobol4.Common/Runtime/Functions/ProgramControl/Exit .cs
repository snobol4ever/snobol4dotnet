using System.Diagnostics;

namespace Snobol4.Common;

//"exit first argument is not suitable integer or string" /* 104 */,
//"exit action not available in this implementation" /* 105 */,
//"exit action caused irrecoverable error" /* 106 */,

public partial class Executive
{
    // EXIT(s) ends the run and has the operating system perform the command s, as SPITBOL's does: through the shell (SHELL, else
    // /bin/sh; cmd.exe on Windows), the run's exit status the command's. EXIT(-3, file) and EXIT(-4, file) write a save file (a.spx
    // without a name), -3 ending the run and -4 going on, as sbl does. Here the file holds the program, as -w writes it, and not its
    // state: running it starts the program over. Any other first argument, a load module (EXIT(3)) included, is error 105, as sbl
    // answers it.
    internal void Exit(List<Var> arguments)
    {
        switch (arguments[0])
        {
            case IntegerVar { Data: -3 or -4 } save:
            {
                // the name is cut at a NUL, as the operating system's call takes it
                var named = arguments[1].Convert(VarType.STRING, out _, out var name, this) && ((string)name).Split('\0')[0] != "";
                var file = named ? ((string)name).Split('\0')[0] : "a.spx";
                if (!Parent.WriteProgramImage(Path.GetFullPath(file), warn: false))
                {
                    LogRuntimeException(106);
                    return;
                }

                if (save.Data == -3)
                    throw new ProgramEndException();
                SystemStack.Push(StringVar.Null());
                return;
            }

            case StringVar { Data.Length: > 0 } command:
                RunCommandAndEnd(command.Data);
                return;

            case IntegerVar or RealVar or StringVar:
                LogRuntimeException(105);
                return;

            default:
                LogRuntimeException(104);
                return;
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

        throw new ProgramEndException();
    }
}
