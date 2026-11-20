using BenchmarkDotNet.Attributes;
using Snobol4.Common;
using Microsoft.VSDiagnostics;

namespace Snobol4.Benchmarks;
[CPUUsageDiagnoser]
public class PatternMatchBenchmark
{
    private Executive _executive = null !;
    private Scanner _scanner = null !;
    private Pattern _simplePattern = null !;
    private Pattern _complexPattern = null !;
    private Pattern _arbPattern = null !;
    private string _shortSubject = null !;
    private string _longSubject = null !;
    [GlobalSetup]
    public void Setup()
    {
        // Initialize Executive (required for pattern matching)
        _executive = new Executive();
        _scanner = new Scanner(_executive);
        // Simple literal pattern: "test"
        _simplePattern = new LiteralPattern("test");
        // Complex pattern with concatenation: "pro" + ANY("aeiou") + "ram"
        var anyVowel = new AnyPattern("aeiou");
        var pro = new LiteralPattern("pro");
        var ram = new LiteralPattern("ram");
        _complexPattern = new ConcatenatePattern(pro, new ConcatenatePattern(anyVowel, ram));
        // ARB pattern (more complex matching)
        var arbLeft = new LiteralPattern("p");
        var arbRight = new LiteralPattern("er");
        var arb = ArbPattern.Structure();
        _arbPattern = new ConcatenatePattern(arbLeft, new ConcatenatePattern(arb, arbRight));
        // Test subjects
        _shortSubject = "test";
        _longSubject = "This is a test string for pattern matching with test in the middle and at the end test";
    }

    [Benchmark]
    public void SimplePatternMatch_ShortString_Anchored()
    {
        _scanner.PatternMatch(_shortSubject, _simplePattern, 0, anchor: true);
    }

    [Benchmark]
    public void SimplePatternMatch_ShortString_Unanchored()
    {
        _scanner.PatternMatch(_shortSubject, _simplePattern, 0, anchor: false);
    }

    [Benchmark]
    public void SimplePatternMatch_LongString_Unanchored()
    {
        _scanner.PatternMatch(_longSubject, _simplePattern, 0, anchor: false);
    }

    [Benchmark]
    public void ComplexPatternMatch_Unanchored()
    {
        _scanner.PatternMatch("programmer", _complexPattern, 0, anchor: false);
    }

    [Benchmark]
    public void ArbPatternMatch_Unanchored()
    {
        _scanner.PatternMatch("programmer", _arbPattern, 0, anchor: false);
    }
}