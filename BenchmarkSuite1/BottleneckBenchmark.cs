using BenchmarkDotNet.Attributes;
using Snobol4.Common;
#if VSDIAGNOSTICS
using Microsoft.VSDiagnostics;
#endif

namespace Snobol4.Benchmarks;

/// <summary>
/// The bottleneck isolation programs of BenchmarkSuite2, each under the name it had there.
/// Each answer was read from sbl -bf on 2026-11-19 (the program run with its result variable written to OUTPUT).
/// </summary>
#if VSDIAGNOSTICS
[CPUUsageDiagnoser]
#endif
[MemoryDiagnoser]
public class BottleneckBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        BenchmarkHelper.CheckVariable(nameof(VarAccess_2000), Scripts.VarAccess, "RESULT", "12012");
        BenchmarkHelper.CheckVariable(nameof(OperatorDispatch_100), Scripts.OperatorDispatch, "RESULT", "165116");
        BenchmarkHelper.CheckVariable(nameof(PatternBacktrack_500), Scripts.PatternBacktrack, "RESULT", "500");
        BenchmarkHelper.CheckVariable(nameof(TableAccess_500), Scripts.TableAccess, "RESULT", "250500");
        BenchmarkHelper.CheckVariable(nameof(MixedWorkload_200), Scripts.MixedWorkload, "RESULT", "550");
    }

    [Benchmark]
    public Builder VarAccess_2000() => BenchmarkHelper.RunQuiet(Scripts.VarAccess);

    [Benchmark]
    public Builder OperatorDispatch_100() => BenchmarkHelper.RunQuiet(Scripts.OperatorDispatch);

    [Benchmark]
    public Builder PatternBacktrack_500() => BenchmarkHelper.RunQuiet(Scripts.PatternBacktrack);

    [Benchmark]
    public Builder TableAccess_500() => BenchmarkHelper.RunQuiet(Scripts.TableAccess);

    [Benchmark]
    public Builder MixedWorkload_200() => BenchmarkHelper.RunQuiet(Scripts.MixedWorkload);
}
