using BenchmarkDotNet.Attributes;
using Snobol4.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VSDiagnostics;

namespace Snobol4.Benchmarks;
[CPUUsageDiagnoser]
[DotNetObjectAllocDiagnoser]
public class PatternMatchBenchmark
{
    [Benchmark]
    public void SimplePatternMatch()
    {
        var builder = CreateAndBuildScript("-b -i", @"
            &anchor = 0
            subject = 'test'
            pattern = 'test'
            result = subject ? pattern
        end");
    }

    [Benchmark]
    public void ComplexPatternMatch()
    {
        var builder = CreateAndBuildScript("-b -i", @"
            &anchor = 0
            subject = 'programmer'
            pattern = 'pro' any('aeiou') 'ram'
            result = subject ? pattern
        end");
    }

    [Benchmark]
    public void ArbPatternMatch()
    {
        var builder = CreateAndBuildScript("-b -i", @"
            &anchor = 0
            subject = 'programmer'
            pattern = 'p' arb 'er'
            result = subject ? pattern
        end");
    }

    [Benchmark]
    public void LongStringPatternMatch()
    {
        var builder = CreateAndBuildScript("-b -i", @"
            &anchor = 0
            subject = 'This is a test string for pattern matching with test in the middle and at the end test'
            pattern = 'test'
            result = subject ? pattern
        end");
    }

    private static Builder CreateAndBuildScript(string directives, string script)
    {
        var commands = new List<string>(
            directives.Split(" ", StringSplitOptions.RemoveEmptyEntries));

        var testDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BenchmarkSuite1");
        var testFilePath = Path.Combine(testDirectory, "Test.sno");
        Directory.CreateDirectory(testDirectory);

        List<string> files = new List<string> { testFilePath };
        var args = commands.Concat(files).ToArray();

        Builder builder = new();
        builder.ParseCommandLine(args);
        builder.Code.ReadTestScript(new MemoryStream(Encoding.UTF8.GetBytes(script)));
        builder.BuildMain();
        return builder;
    }
}