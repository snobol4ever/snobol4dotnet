namespace Snobol4.Common;

using System.Management;

//"erroneous argument for host" /* 254 */,
//"error during execution of host" /* 255 */,

// HOST(n, ...), as SPITBOL answers it:
//   HOST()        the host identification string
//   HOST(0)       the -u string, or else the words after the program's files, joined by blanks
//   HOST(1, cmd)  runs cmd in the shell and answers its wait status (exit status times 256)
//   HOST(2, i)    word i of the command line (0 is the program's own path); fails past the end
//   HOST(3)       the index of the first word after the program's files
//   HOST(4, name) the environment variable name; fails when it is not set
//   anything else is error 254

public partial class Executive
{
    internal void Host(List<Var> arguments)
    {
        if (arguments[0] is not StringVar { Data: "" })
        {
            if (!arguments[0].Convert(VarType.INTEGER, out _, out var selectorObj, this))
            {
                LogRuntimeException(254);
                return;
            }

            switch ((long)selectorObj)
            {
                case 0:
                    SystemStack.Push(new StringVar(Parent.BuildOptions.HostParameter));
                    return;

                case 1:
                    HostShell(arguments[1]);
                    return;

                case 2:
                    HostWord(arguments[1]);
                    return;

                case 3:
                    SystemStack.Push(new IntegerVar(1 + Parent.FirstFileWord + Parent.FilesRead));
                    return;

                case 4:
                    HostEnvironment(arguments[1]);
                    return;

                default:
                    LogRuntimeException(254);
                    return;
            }
        }

        if (arguments[0] is IntegerVar { Data: 0 })
        {
            SystemStack.Push(new StringVar(Parent.BuildOptions.HostParameter));
            return;
        }

        if (arguments[0] is StringVar { Data: "" })
        {
            var cpuName = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            if (OperatingSystem.IsWindows())
            {
                var moc = new ManagementObjectSearcher("select * from Win32_Processor").Get();
                foreach (var obj in moc)
                {
                    try
                    {
                        cpuName = obj["Name"].ToString();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            SystemStack.Push(new StringVar(@$"{cpuName}:{Environment.OSVersion}:SNOBOL4.net:Version 0.1#"));
            return;
        }

        SystemStack.Push(StringVar.Null());
    }

    private void HostWord(Var indexArgument)
    {
        if (!indexArgument.Convert(VarType.INTEGER, out _, out var indexObj, this))
        {
            LogRuntimeException(254);
            return;
        }

        var index = (long)indexObj;
        var words = Parent.CommandWords;
        if (index < 0 || index > words.Length)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new StringVar(index == 0 ? Environment.GetCommandLineArgs()[0] : words[index - 1]));
    }

    private void HostEnvironment(Var nameArgument)
    {
        if (!nameArgument.Convert(VarType.STRING, out _, out var nameObj, this))
        {
            LogRuntimeException(254);
            return;
        }

        var value = Environment.GetEnvironmentVariable((string)nameObj);
        if (value is null)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new StringVar(value));
    }

    private void HostShell(Var commandArgument)
    {
        if (!commandArgument.Convert(VarType.STRING, out _, out var commandObj, this))
        {
            LogRuntimeException(254);
            return;
        }

        try
        {
            Console.Out.Flush();
            var start = new System.Diagnostics.ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
                UseShellExecute = false
            };
            start.ArgumentList.Add(OperatingSystem.IsWindows() ? "/C" : "-c");
            start.ArgumentList.Add((string)commandObj);
            using var process = System.Diagnostics.Process.Start(start);
            process!.WaitForExit();
            SystemStack.Push(new IntegerVar(OperatingSystem.IsWindows() ? process.ExitCode : (long)process.ExitCode * 256));
        }
        catch (Exception)
        {
            LogRuntimeException(255);
        }
    }
}
