using BenchmarkDotNet.Running;

namespace BenchmarkSuite1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // With no arguments every benchmark of the assembly runs; BenchmarkDotNet's own options narrow it, as in
            //   dotnet run -c Release -- --filter '*PatternMatch*'      (one class)
            //   dotnet run -c Release -- --filter '*' --job short        (every benchmark, fewer iterations)
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
        }
    }
}
