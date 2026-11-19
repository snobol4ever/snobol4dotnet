using BenchmarkDotNet.Attributes;
using Snobol4.Common;
#if VSDIAGNOSTICS
using Microsoft.VSDiagnostics;
#endif

namespace Snobol4.Benchmarks;

/// <summary>
/// The scripts folder of BenchmarkSuite2: the bottleneck programs at full scale, each printing its answer. Each program is
/// scripts/NAME.sno and its ref scripts/NAME.ref, cut from sbl -bf twice alike on 2026-11-19; both are embedded in this assembly.
/// Each benchmark's setup checks only its own program, since the largest take seconds a run.
/// </summary>
#if VSDIAGNOSTICS
[CPUUsageDiagnoser]
#endif
[MemoryDiagnoser]
public class ScaledBottleneckBenchmark
{
    private string _script = null!;

    private void Load(string name)
    {
        _script = BenchmarkHelper.Resource(name + ".sno");
        BenchmarkHelper.CheckOutput(name, _script, BenchmarkHelper.Resource(name + ".ref"));
    }

    [GlobalSetup(Target = nameof(FuncCall_2000000))]
    public void SetupFuncCall() => Load("func_call");

    [GlobalSetup(Target = nameof(MixedWorkload_500))]
    public void SetupMixedWorkload() => Load("mixed_workload");

    [GlobalSetup(Target = nameof(OperatorDispatch_100000))]
    public void SetupOperatorDispatch() => Load("op_dispatch");

    [GlobalSetup(Target = nameof(PatternBacktrack_200000))]
    public void SetupPatternBacktrack() => Load("pattern_bt");

    [GlobalSetup(Target = nameof(StringConcat_2000))]
    public void SetupStringConcat() => Load("string_concat");

    [GlobalSetup(Target = nameof(TableAccess_200x1000))]
    public void SetupTableAccess() => Load("table_access");

    [GlobalSetup(Target = nameof(VarAccess_1000000))]
    public void SetupVarAccess() => Load("var_access");

    [Benchmark]
    public Builder FuncCall_2000000() => BenchmarkHelper.RunQuiet(_script);

    [Benchmark]
    public Builder MixedWorkload_500() => BenchmarkHelper.RunQuiet(_script);

    [Benchmark]
    public Builder OperatorDispatch_100000() => BenchmarkHelper.RunQuiet(_script);

    [Benchmark]
    public Builder PatternBacktrack_200000() => BenchmarkHelper.RunQuiet(_script);

    [Benchmark]
    public Builder StringConcat_2000() => BenchmarkHelper.RunQuiet(_script);

    [Benchmark]
    public Builder TableAccess_200x1000() => BenchmarkHelper.RunQuiet(_script);

    [Benchmark]
    public Builder VarAccess_1000000() => BenchmarkHelper.RunQuiet(_script);
}
