using BenchmarkDotNet.Running;

namespace Snobol4.Benchmarks2;

/// <summary>
/// BenchmarkSuite2 — end-to-end execution benchmarks for SNOBOL4.NET.
///
/// Unlike BenchmarkSuite1 (which benchmarks the pattern scanner in isolation),
/// these benchmarks run complete SNOBOL4 programs through the full pipeline:
///   SNOBOL4 source → Parser → CodeGenerator → Roslyn → ExecuteLoop
///
/// Three workload profiles:
///   RomanBenchmark           — recursive functions, heavy identifier lookup
///   ArithmeticLoopBenchmark  — pure dispatch overhead, tight counter loop
///   StringPatternLoopBenchmark — realistic string/pattern workload
///
/// Purpose: establish a baseline before the threaded-execution refactor
/// (feature/threaded-execution) so we can measure the improvement precisely.
///
/// Run with: dotnet run -c Release
/// Results are written to BenchmarkDotNet.Artifacts/
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run(typeof(Program).Assembly, null, args);
    }
}
