using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace Snobol4.Common;

public partial class Executive
{
    // ReSharper disable once UnusedMember.Global
    public void SaveStatus(bool bSaveStatus)
    {
        Failure = bSaveStatus;
    }

    internal void Execute(Assembly dll, AssemblyLoadContext _, string fullClassName)
    {
        _timerExecute.Restart();
        dynamic? instance = dll.CreateInstance(fullClassName);
        if (instance == null)
            throw new ApplicationException("internal void Execute(Assembly dll, AssemblyLoadContext _, string fullClassName)");
        instance.Run(this);
        _timerExecute.Stop();
    }

    internal void PrintExecutionStatistics()
    {
        if (!Parent.BuildOptions.ShowExecutionStatistics)
            return;

        var memoryUsed = Process.GetCurrentProcess().WorkingSet64;
        var memInfo = GC.GetGCMemoryInfo();
        var memoryLeft = memInfo.TotalAvailableMemoryBytes;
        Console.Error.WriteLine("");
        Console.Error.WriteLine("");
        Console.Error.WriteLine("");
        if (AmpErrorType == 0)
            Console.Error.WriteLine(@"normal end");
        var fileName = Path.GetFileName(SourceFiles[AmpCurrentLineNumber]);
        Console.Error.WriteLine(@$"in file                  {fileName}");
        Console.Error.WriteLine(@$"in line                  {SourceLineNumbers[AmpCurrentLineNumber - 1]}");
        Console.Error.WriteLine(@$"in statement             {AmpCurrentLineNumber}");
        Console.Error.WriteLine(@$"statements executed      {AmpStatementCount}");
        Console.Error.WriteLine(@$"execution time seconds   {_timerExecute.ElapsedTicks / 10000000.0}");
        Console.Error.WriteLine(@$"regenerations            {GC.CollectionCount(memInfo.Generation)}");
        Console.Error.WriteLine(@$"memory used (bytes)      {memoryUsed}");
        Console.Error.WriteLine(@$"memory left (bytes)      {memoryLeft}");
        Console.Error.WriteLine("");


        if (AmpProfile != 1 && AmpProfile != 3 && AmpProfile != 4)
            return;

        foreach (var entry in ProfileCount)
        {
            Console.WriteLine($@"{entry.Key},{entry.Value},{ProfileTotal[entry.Key] / 10000000.0},{ProfileTotal[entry.Key] / (entry.Value * 10000000.0)}");
        }

    }

    internal void CloseAllStreams()
    {
        // Close all writers
        foreach (var streamReader in StreamReadersBySymbol)
            streamReader.Value.Close();

        // Close all streams
        foreach (var readStream in StreamInputs)
            //    if (readStream.Value is FileStream)
            readStream.Value.Close();

        foreach (var writeStream in StreamOutputs)
            //  if (writeStream.Value is FileStream)
            writeStream.Value.Close();
    }
}

public class Profiler : IDisposable
{
    private Executive _exec;
    private readonly Stopwatch? _timer;
    private readonly string _statement;

    public Profiler(string statement, Executive exec)
    {
        _exec = exec;
        _statement = statement;
        _timer = Stopwatch.StartNew();
    }

    public static Profiler? Start1(string statement, Executive exec)
    {
        if (exec.AmpProfile == 1)
        {
            return new Profiler(statement, exec);
        }

        return null;
    }

    public static Profiler? Start3(string statement, Executive exec)
    {
        if (exec.AmpProfile == 3)
        {
            return new Profiler(statement, exec);
        }

        return null;
    }

    public static Profiler? Start4(string statement, Executive exec)
    {
        if (exec.AmpProfile == 4)
        {
            return new Profiler(statement, exec);
        }

        return null;
    }

    public void Dispose()
    {
        _timer.Stop();

        if (!_exec.ProfileTotal.ContainsKey(_statement))
        {
            _exec.ProfileCount[_statement] = 1;
            _exec.ProfileTotal[_statement] = _timer.ElapsedTicks;
            return;
        }
        _exec.ProfileCount[_statement]++;
        _exec.ProfileTotal[_statement] += _timer.ElapsedTicks;
    }
}
