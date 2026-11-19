using BenchmarkDotNet.Attributes;
using Snobol4.Common;
#if VSDIAGNOSTICS
using Microsoft.VSDiagnostics;
#endif

namespace Snobol4.Benchmarks;

/// <summary>
/// The core workloads of BenchmarkSuite2: whole programs through the full Builder pipeline, each under the name it had there.
/// Each answer was read from sbl -bf on 2026-11-19 (the program run with its result variable written to OUTPUT).
/// </summary>
#if VSDIAGNOSTICS
[CPUUsageDiagnoser]
#endif
[MemoryDiagnoser]
public class WorkloadBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        BenchmarkHelper.CheckVariable(nameof(Roman_1776), Scripts.Roman, "R1", "MDCCLXXVI");
        BenchmarkHelper.CheckVariable(nameof(ArithLoop_1000), Scripts.ArithLoop, "RESULT", "1000");
        BenchmarkHelper.CheckVariable(nameof(StringPattern_200), Scripts.StringPattern, "FINAL", "alphabetagammadeltaepsilonzetaetathetaiotakappa");
        BenchmarkHelper.CheckVariable(nameof(Fibonacci_18), Scripts.Fibonacci, "RESULT", "2584");
        BenchmarkHelper.CheckVariable(nameof(StringManip_500), Scripts.StringManip, "RESULT", "43");
        BenchmarkHelper.CheckVariable(nameof(FuncCallOverhead_3000), Scripts.FunctionCallOverhead, "RESULT", "300");
        BenchmarkHelper.CheckVariable(nameof(StringConcat_500), Scripts.StringConcat, "RESULT", "100");
    }

    [Benchmark]
    public Builder Roman_1776() => BenchmarkHelper.RunQuiet(Scripts.Roman);

    [Benchmark]
    public Builder ArithLoop_1000() => BenchmarkHelper.RunQuiet(Scripts.ArithLoop);

    [Benchmark]
    public Builder StringPattern_200() => BenchmarkHelper.RunQuiet(Scripts.StringPattern);

    [Benchmark]
    public Builder Fibonacci_18() => BenchmarkHelper.RunQuiet(Scripts.Fibonacci);

    [Benchmark]
    public Builder StringManip_500() => BenchmarkHelper.RunQuiet(Scripts.StringManip);

    // the name is BenchmarkSuite2's; the program makes 300 calls
    [Benchmark]
    public Builder FuncCallOverhead_3000() => BenchmarkHelper.RunQuiet(Scripts.FunctionCallOverhead);

    // the name is BenchmarkSuite2's; the program appends 100 characters
    [Benchmark]
    public Builder StringConcat_500() => BenchmarkHelper.RunQuiet(Scripts.StringConcat);
}
