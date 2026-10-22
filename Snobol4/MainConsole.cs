using Snobol4.Common;

namespace Snobol4;

internal class MainConsole
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;


        if (args.Length == 0)
        {
            Builder.DisplayManual();
            return Executive.AmpCode;
        }

        Builder builder = new();

        // If the last argument is a DLL, or a save file EXIT(-3) wrote, run it
        if (args.Length > 0 && Path.GetExtension(args[^1]) is var extension
            && (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".spx", StringComparison.OrdinalIgnoreCase)))
        {
            builder.ParseCommandLine(args);
            builder.RunDll(Path.GetFullPath(args[^1]));
            return Executive.AmpCode;
        }

        // Otherwise, run the compiler
        builder.ParseCommandLine(args);
        builder.DisplaySignOnBanner();
        builder.Code.ReadAllFiles();
        builder.BuildMain();
        return builder.ExitStatus ?? Executive.AmpCode;
    }
}

