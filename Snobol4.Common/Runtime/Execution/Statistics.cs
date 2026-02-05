using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace Snobol4.Common;

public partial class Executive
{
    public bool ProfileStatements = false;

    // ReSharper disable once UnusedMember.Global
    public void SaveStatus(bool bSaveStatus)
    {
        Failure = bSaveStatus;
    }

    /// <summary>
    /// Load and execute the assembly (DLL) from the compilation of a Snobol4 program.
    ///  https://stackoverflow.com/questions/14479074/c-sharp-reflection-load-assembly-and-invoke-a-method-if-it-exists
    ///  https://github.com/munibrbutt/articles-code/blob/main/Dynamically%20loading%20and%20running%20CSharp%20code/ConsoleAppReadCode/Program.cs
    /// </summary>
    internal void Execute(Assembly dll, AssemblyLoadContext _, string fullClassName)
    {
        _timerExecute.Restart();
        dynamic? instance = dll.CreateInstance(fullClassName);
        if (instance == null)
            throw new ApplicationException("internal void Execute(Assembly dll, AssemblyLoadContext _, string fullClassName)");
        instance.Run(this);
        _timerExecute.Stop();
        PrintExecutionStatistics();
        DisplayVariableValues();
        CloseAllStreams();
    }

    private void PrintExecutionStatistics()
    {
        if (!Parent.ShowExecutionStatistics)
            return;

        var memoryUsed = Process.GetCurrentProcess().WorkingSet64;
        var memInfo = GC.GetGCMemoryInfo();
        var memoryLeft = memInfo.TotalAvailableMemoryBytes;
        Console.Error.WriteLine("");
        Console.Error.WriteLine("");
        Console.Error.WriteLine("");
        if (((IntegerVar)IdentifierTable["&code"]).Data == 0)
            Console.Error.WriteLine(@"normal end");
        Console.Error.WriteLine(@$"in file              {((StringVar)IdentifierTable["&file"]).Data}");
        Console.Error.WriteLine(@$"in line              {((IntegerVar)IdentifierTable["&line"]).Data}");
        Console.Error.WriteLine(@$"in statement         {((IntegerVar)IdentifierTable["&stno"]).Data}");
        //Console.Error.WriteLine(@$"stmts executed       {((IntegerVar)IdentifierTable["&stcount"]).Data}");
        Console.Error.WriteLine(@$"stmts executed       {Amp_StatementCount}");
        Console.Error.WriteLine(@$"execution time sec   {_timerExecute.Elapsed}");
        Console.Error.WriteLine(@$"regenerations        {GC.CollectionCount(memInfo.Generation)}");
        Console.Error.WriteLine(@$"memory used (bytes)  {memoryUsed}");
        Console.Error.WriteLine(@$"memory left (bytes)  {memoryLeft}");
        Console.Error.WriteLine("");
    }

    private void CloseAllStreams()
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
    private readonly Stopwatch _timer;
    private readonly string _statement = "";
    private readonly bool _enable = false;

    public Profiler(string statement, bool enable)
    {
        _enable = enable;
        if (!_enable)
        {
            return;
        }

        _statement = statement;
        _timer = Stopwatch.StartNew();
    }

    public static Profiler? Start(string statement, bool enable)
    {
        if (enable)
        {
            return new Profiler(statement, enable);
        }

        return null;
    }

    public void Dispose()
    {
        if (!_enable)
        {
            return;
        }

        _timer.Stop();
        Console.WriteLine($@"{_statement},{_timer.Elapsed.ToString()[6..^1]}");
    }
}
