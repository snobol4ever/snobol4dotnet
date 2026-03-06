using System;
using Snobol4.Benchmarks2;

internal class Program
{
    private const int Reps   = 5;
    private const int Warmup = 1;

    static void Main()
    {
        Console.WriteLine($"SNOBOL4.NET Benchmark — {Reps} reps, {Warmup} warmup, Release build");
        Console.WriteLine(new string('-', 74));
        Console.WriteLine($"{"Benchmark",-28} {"Mean",8} {"±",2} {"StdDev",8} {"Alloc/run",12}");
        Console.WriteLine(new string('-', 74));

        Run("Roman_1776",           Scripts.Roman,                "R1",     Reps, Warmup);
        Run("ArithLoop_1000",       Scripts.ArithLoop,            "RESULT", Reps, Warmup);
        Run("StringPattern_200",    Scripts.StringPattern,        "FINAL",  Reps, Warmup);
        Run("Fibonacci_18",         Scripts.Fibonacci,            "RESULT", Reps, Warmup);
        Run("StringManip_500",     Scripts.StringManip,          "RESULT", Reps, Warmup);

        Console.WriteLine();
        Console.WriteLine("--- Bottleneck isolation ---");
        Console.WriteLine();

        Run("VarAccess_2000",        Scripts.VarAccess,            "RESULT", Reps, Warmup);
        Run("OperatorDispatch_100", Scripts.OperatorDispatch,     "RESULT", Reps, Warmup);
        Run("PatternBacktrack_500",  Scripts.PatternBacktrack,     "RESULT", Reps, Warmup);
        Run("TableAccess_500",      Scripts.TableAccess,          "RESULT", Reps, Warmup);
        Run("MixedWorkload_200",     Scripts.MixedWorkload,        "RESULT", Reps, Warmup);

        Console.WriteLine(new string('-', 74));
    }

    static void Run(string name, string script, string resultVar, int reps, int warmup)
    {
        try
        {
            var check = BenchmarkHelper.RunScript(script);
            var result = check.Execute!.IdentifierTable[check.FoldCase(resultVar)].ToString();
            var (mean, stddev, alloc) = BenchmarkHelper.Measure(script, reps, warmup);
            Console.WriteLine(
                $"{name,-28} {mean,7:F1}ms ± {stddev,6:F1}ms {alloc / 1024,9:F0} KB   ({result})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{name,-28} ERROR: {ex.Message}");
        }
    }
}
