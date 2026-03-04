using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Snobol4.Common;

namespace Snobol4.Benchmarks2;

/// <summary>
/// Shared helper for running a SNOBOL4 script through the full Builder pipeline
/// and returning the resulting Executive for inspection.
/// </summary>
public static class BenchmarkHelper
{
    /// <summary>
    /// Compile and execute a SNOBOL4 script, returning the Builder (which holds
    /// the Executive and its IdentifierTable for result inspection).
    /// Uses the same path as SetupTests in the test suite.
    /// </summary>
    public static Builder RunScript(string script)
    {
        var commands = new List<string> { "-b" };
        var testFilePath = Path.Combine(Path.GetTempPath(), "bench.sno");
        var args = commands.Concat(new[] { testFilePath }).ToArray();

        Builder builder = new();
        builder.ParseCommandLine(args);
        builder.Code.ReadTestScript(new MemoryStream(Encoding.UTF8.GetBytes(script)));
        builder.BuildMain();
        return builder;
    }
}
