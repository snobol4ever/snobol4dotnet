// benchmarks/Program.cs
//
// SNOBOL4.NET Benchmark Runner — loads .sno programs from snobol4corpus submodule.
//
// Programs live in:  corpus/benchmarks/*.sno
//
// Run with:
//   cd snobol4dotnet
//   export PATH=$PATH:/usr/local/dotnet
//   dotnet run --project benchmarks/Benchmarks.csproj -c Release

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Snobol4.Common;

// ── Locate corpus ─────────────────────────────────────────────────────────────

static string FindCorpus()
{
    // Walk up from the executable until we find corpus/benchmarks/
    string dir = AppContext.BaseDirectory;
    for (int i = 0; i < 8; i++)
    {
        string candidate = Path.Combine(dir, "corpus", "benchmarks");
        if (Directory.Exists(candidate)) return candidate;
        string parent = Directory.GetParent(dir)?.FullName ?? dir;
        if (parent == dir) break;
        dir = parent;
    }
    throw new DirectoryNotFoundException(
        "Cannot find corpus/benchmarks/ — ensure the snobol4corpus submodule is initialised.\n" +
        "Run: git submodule update --init --recursive");
}

string benchDir = FindCorpus();

// ── Benchmark table ───────────────────────────────────────────────────────────

var benchmarks = new List<BenchEntry>
{
    // Core workloads — mirror of BenchmarkSuite2
    new("Roman_1776",            "roman.sno",                "R1",     5, 1),
    new("ArithLoop_1000",        "arith_loop.sno",           "N",      5, 1),
    new("StringPattern_200",     "string_pattern.sno",       "RESULT", 5, 1),
    new("Fibonacci_18",          "fibonacci.sno",            "RESULT", 5, 1),
    new("StringManip_500",       "string_manip.sno",         "RESULT", 5, 1),
    new("FuncCallOverhead_3000", "func_call_overhead.sno",   "RESULT", 5, 1),
    new("StringConcat_2000",     "string_concat.sno",        "RESULT", 5, 1),

    // Bottleneck isolation
    new("VarAccess_2000",        "var_access.sno",           "RESULT", 5, 1),
    new("OperatorDispatch",      "op_dispatch.sno",          "RESULT", 5, 1),
    new("PatternBacktrack_500",  "pattern_bt.sno",           "RESULT", 5, 1),
    new("TableAccess_500",       "table_access.sno",         "RESULT", 5, 1),
    new("MixedWorkload_200",     "mixed_workload.sno",       "RESULT", 5, 1),

    // EVAL / indirect dispatch
    new("EvalFixed_200",         "eval_fixed.sno",           "RESULT", 5, 1),
    new("EvalDynamic_200",       "eval_dynamic.sno",         "RESULT", 5, 1),
    new("IndirectDispatch_500",  "indirect_dispatch.sno",    "RESULT", 5, 1),
};

// ── Runner ────────────────────────────────────────────────────────────────────

static Builder RunScript(string src)
{
    Builder builder = new();
    builder.ParseCommandLine(new[] { "-b", Path.Combine(Path.GetTempPath(), "bench.sno") });
    builder.Code.ReadTestScript(new MemoryStream(Encoding.UTF8.GetBytes(src)));
    builder.BuildMain();
    return builder;
}

static (double mean, double stddev, long allocBytes) Measure(string src, int reps, int warmup)
{
    for (int i = 0; i < warmup; i++) RunScript(src);

    var times = new List<long>(reps);
    long totalAlloc = 0;

    for (int i = 0; i < reps; i++)
    {
        long before = GC.GetTotalAllocatedBytes(precise: false);
        var sw = Stopwatch.StartNew();
        RunScript(src);
        sw.Stop();
        long after = GC.GetTotalAllocatedBytes(precise: false);
        times.Add(sw.ElapsedMilliseconds);
        totalAlloc += after - before;
    }

    double mean   = times.Average();
    double stddev = Math.Sqrt(times.Average(t => (t - mean) * (t - mean)));
    return (mean, stddev, totalAlloc / reps);
}

// ── Header ────────────────────────────────────────────────────────────────────

Console.WriteLine($"SNOBOL4.NET Benchmark Runner");
Console.WriteLine($"Programs: {benchDir}");
Console.WriteLine(new string('-', 76));
Console.WriteLine($"{"Benchmark",-30} {"Mean",8} {"±",2} {"StdDev",8} {"Alloc/run",12}  Result");
Console.WriteLine(new string('-', 76));

// ── Run ───────────────────────────────────────────────────────────────────────

foreach (var entry in benchmarks)
{
    string path = Path.Combine(benchDir, entry.File);
    if (!File.Exists(path))
    {
        Console.WriteLine($"{entry.Name,-30} SKIP  ({entry.File} not found)");
        continue;
    }

    string src = File.ReadAllText(path);

    try
    {
        // Correctness check — grab result value
        var check = RunScript(src);
        string resultVal = "?";
        string key = check.FoldCase(entry.ResultVar);
        if (check.Execute?.IdentifierTable.TryGetValue(key, out var v) == true)
            resultVal = v?.ToString() ?? "null";

        var (mean, stddev, alloc) = Measure(src, entry.Reps, entry.Warmup);
        Console.WriteLine(
            $"{entry.Name,-30} {mean,7:F1}ms ± {stddev,6:F1}ms {alloc / 1024,9:F0} KB   {resultVal}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{entry.Name,-30} ERROR: {ex.Message}");
    }
}

Console.WriteLine(new string('-', 76));
Console.WriteLine("Done.");

// ── Types (C# requires them after the top-level statements) ──────────────────

record BenchEntry(string Name, string File, string ResultVar, int Reps = 5, int Warmup = 1);
