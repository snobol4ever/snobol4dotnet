#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Snobol4.Common;
#if VSDIAGNOSTICS
using Microsoft.VSDiagnostics;
#endif

namespace Snobol4.Benchmarks;

/// <summary>
/// Every SNOBOL4 kernel of snobol4ever/corpus, benchmarks/snobol4/*.sno (the programs the old benchmarks/ runner read, all of
/// them rather than its list of fifteen), one case a kernel. A kernel runs at the sizes its NAME.heap and NAME.stack declare, and
/// its setup checks what it writes to OUTPUT against NAME.ref, cut from sbl -bf. The folder is the one CORPUS names, else the first
/// corpus/benchmarks/snobol4 found walking up from this assembly or from the working directory.
/// </summary>
#if VSDIAGNOSTICS
[CPUUsageDiagnoser]
#endif
[MemoryDiagnoser]
public class CorpusBenchmark
{
    private const string NotFound = "corpus-not-found";
    private string _script = null!;
    private string[] _switches = null!;

    [ParamsSource(nameof(Kernels))]
    public string Kernel { get; set; } = null!;

    public static IEnumerable<string> Kernels()
    {
        var dir = FindCorpus();
        if (dir == null)
            return new[] { NotFound };
        return Directory.GetFiles(dir, "*.sno").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.Ordinal)!;
    }

    [GlobalSetup]
    public void Setup()
    {
        var dir = FindCorpus() ?? throw new DirectoryNotFoundException(
            "no corpus/benchmarks/snobol4 above this assembly or the working directory; set CORPUS to the folder that holds the kernels");
        var path = Path.Combine(dir, Kernel + ".sno");
        _script = File.ReadAllText(path);
        _switches = new[] { "-d" + Size(dir, "heap") + "k", "-s" + Size(dir, "stack") + "k" };
        BenchmarkHelper.CheckOutput(Kernel, _script, File.ReadAllText(Path.Combine(dir, Kernel + ".ref")), _switches);
    }

    [Benchmark]
    public Builder CorpusKernel() => BenchmarkHelper.RunQuiet(_script, _switches);

    // NAME.heap and NAME.stack hold one line, the name, a tab and the size in KB
    private string Size(string dir, string kind)
    {
        var line = File.ReadAllText(Path.Combine(dir, Kernel + "." + kind)).Trim();
        return line.Split('\t').Last();
    }

    private static string? FindCorpus()
    {
        var named = Environment.GetEnvironmentVariable("CORPUS");
        if (!string.IsNullOrEmpty(named))
            return Directory.Exists(named) ? named : null;
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "corpus", "benchmarks", "snobol4");
                if (Directory.Exists(candidate))
                    return candidate;
            }
        }
        return null;
    }
}
