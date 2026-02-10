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

        Console.WriteLine(Environment.CurrentDirectory);

        Builder builder = new();

        // If the last argument is a DLL, run it
        if (args.Length > 0 && string.Equals(Path.GetExtension(args[^1]), ".dll", StringComparison.OrdinalIgnoreCase))
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
        return Executive.AmpCode;
    }
}

