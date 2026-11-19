using BenchmarkDotNet.Attributes;
using Snobol4.Common;
#if VSDIAGNOSTICS
using Microsoft.VSDiagnostics;
#endif

namespace Snobol4.Benchmarks;

/// <summary>
/// The CODE() and EVAL() runtime-compile programs of BenchmarkSuite2, each under the name it had there, and CodeAndGoto, which
/// BenchmarkSuite2 defined and never ran. Each answer was read from sbl -bf on 2026-11-19.
/// Two of them end in an error in SPITBOL as here, so each times its compile and its run up to that error, not the loop its
/// comment describes: CodeAndGoto_100 ends in error 239 at :($C) (C holds CODE, not a label name), and IndirectDispatch_500 in
/// error 022 at $FN(X), which SPITBOL reads as $(FN(X)), a call of the undefined function FN. The indirect call the second meant
/// to time is the corpus kernel indirect_dispatch, through APPLY (CorpusBenchmark).
/// </summary>
#if VSDIAGNOSTICS
[CPUUsageDiagnoser]
#endif
[MemoryDiagnoser]
public class RuntimeCompileBenchmark
{
    [GlobalSetup]
    public void Setup()
    {
        BenchmarkHelper.CheckVariable(nameof(CodeFixed_200), Scripts.CodeFixedString, "RESULT", "200");
        BenchmarkHelper.CheckVariable(nameof(CodeDynamic_200), Scripts.CodeDynamicString, "RESULT", "200");
        BenchmarkHelper.CheckError(nameof(CodeAndGoto_100), Scripts.CodeAndGoto, 239);
        BenchmarkHelper.CheckVariable(nameof(EvalFixed_200), Scripts.EvalSimpleExpr, "RESULT", "11");
        BenchmarkHelper.CheckVariable(nameof(EvalDynamic_200), Scripts.EvalDynamicExpr, "RESULT", "400");
        BenchmarkHelper.CheckError(nameof(IndirectDispatch_500), Scripts.EvalVsIndirect, 22);
    }

    [Benchmark]
    public Builder CodeFixed_200() => BenchmarkHelper.RunQuiet(Scripts.CodeFixedString);

    [Benchmark]
    public Builder CodeDynamic_200() => BenchmarkHelper.RunQuiet(Scripts.CodeDynamicString);

    [Benchmark]
    public Builder CodeAndGoto_100() => BenchmarkHelper.RunQuiet(Scripts.CodeAndGoto);

    [Benchmark]
    public Builder EvalFixed_200() => BenchmarkHelper.RunQuiet(Scripts.EvalSimpleExpr);

    [Benchmark]
    public Builder EvalDynamic_200() => BenchmarkHelper.RunQuiet(Scripts.EvalDynamicExpr);

    [Benchmark]
    public Builder IndirectDispatch_500() => BenchmarkHelper.RunQuiet(Scripts.EvalVsIndirect);
}
