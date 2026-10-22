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
        // Run() populates SourceCode, SourceFiles, LabelTable, Statements etc.
        // In threaded mode, ExecuteLoop() will detect Thread != null and
        // call ThreadedExecuteLoop instead of the C# statement dispatch.
        instance.Run(this);
        _timerExecute.Stop();
    }

    internal void StartTimer()
    {
        ExecutionBegan = true;
        _timerExecute.Start();
    }

    // The program ran: -x prints nothing for one that did not start (a compile error, -n, an include that could not be opened)
    internal bool ExecutionBegan;

    internal void PrintExecutionStatistics()
    {
        _timerExecute.Stop();

        if (!Parent.BuildOptions.ShowExecutionStatistics || ExitEndsRun)
            return;

        // -x: the end-of-run statistics on the listing stream, as sbl lays them out: three empty lines, "normal end", the run summary
        // and an empty line. A run that ended on a fatal error has printed its summary in the report.
        if (ExecutionBegan && !FatalReportPrinted)
        {
            var listing = Parent.ListingWriter;
            string[] block = ["", "", "", "normal end", .. RunSummary(), ""];
            foreach (var text in block)
                listing.WriteLine(text);
            listing.Flush();
        }


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
        _timer!.Stop();

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
