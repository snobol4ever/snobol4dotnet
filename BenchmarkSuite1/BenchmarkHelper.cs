using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Snobol4.Common;

namespace Snobol4.Benchmarks;

/// <summary>
/// Runs a SNOBOL4 program through the full Builder pipeline (lex, parse, resolve, emit, thread, execute) and returns the
/// resulting Builder, which holds the Executive and its IdentifierTable. A benchmark times RunQuiet; its [GlobalSetup] first
/// checks the program's answer with RunCaptured, so a benchmark whose program stops being right fails instead of timing it.
/// </summary>
public static class BenchmarkHelper
{
    public static Builder RunScript(string script, params string[] switches)
    {
        Builder builder = new();
        builder.ParseCommandLine(new[] { "-b" }.Concat(switches).Append(Path.Combine(Path.GetTempPath(), "bench.sno")).ToArray());
        builder.Code.ReadTestScript(new MemoryStream(Encoding.UTF8.GetBytes(script)));
        builder.BuildMain();
        return builder;
    }

    /// <summary>The run a benchmark times: what the program writes to OUTPUT, or an error report, goes nowhere.</summary>
    public static Builder RunQuiet(string script, params string[] switches)
    {
        TextWriter output = Console.Out, error = Console.Error;
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        try
        {
            return RunScript(script, switches);
        }
        finally
        {
            Console.SetOut(output);
            Console.SetError(error);
        }
    }

    /// <summary>The run a check reads: the Builder and everything the program wrote to OUTPUT.</summary>
    public static (Builder builder, string output) RunCaptured(string script, params string[] switches)
    {
        TextWriter output = Console.Out, error = Console.Error;
        var captured = new StringWriter();
        Console.SetOut(captured);
        Console.SetError(TextWriter.Null);
        try
        {
            var builder = RunScript(script, switches);
            return (builder, captured.ToString());
        }
        finally
        {
            Console.SetOut(output);
            Console.SetError(error);
        }
    }

    /// <summary>Throws unless the program leaves <paramref name="variable"/> holding <paramref name="expected"/>, sbl -bf's answer.</summary>
    public static void CheckVariable(string name, string script, string variable, string expected)
    {
        var builder = RunCaptured(script).builder;
        var key = builder.FoldCase(variable);
        string actual = builder.Execute != null && builder.Execute.IdentifierTable.TryGetValue(key, out var v) ? v?.ToString() ?? "" : "<unset>";
        if (actual != expected)
            throw new InvalidOperationException($"{name}: {variable} is '{actual}', sbl -bf answers '{expected}'");
    }

    /// <summary>Throws unless the program ends in the error <paramref name="errorNumber"/> that sbl -bf ends it in.</summary>
    public static void CheckError(string name, string script, int errorNumber)
    {
        var builder = RunCaptured(script).builder;
        if (!builder.ErrorCodeHistory.Contains(errorNumber))
            throw new InvalidOperationException($"{name}: ends without error {errorNumber}, which sbl -bf ends it in "
                                                + $"(errors seen: {string.Join(",", builder.ErrorCodeHistory)})");
    }

    /// <summary>Throws unless what the program writes to OUTPUT is <paramref name="expected"/>, its ref cut from sbl -bf.</summary>
    public static void CheckOutput(string name, string script, string expected, params string[] switches)
    {
        var output = RunCaptured(script, switches).output.Replace("\r\n", "\n");
        if (output != expected.Replace("\r\n", "\n"))
            throw new InvalidOperationException($"{name}: OUTPUT differs from its ref (sbl -bf)\n--- ref\n{expected}\n--- this run\n{output}");
    }

    /// <summary>A program or ref this assembly carries (scripts/NAME.sno, scripts/NAME.ref), by its file name.</summary>
    public static string Resource(string fileName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("scripts." + fileName)
                           ?? throw new FileNotFoundException($"scripts/{fileName} is not embedded in {Assembly.GetExecutingAssembly().GetName().Name}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
