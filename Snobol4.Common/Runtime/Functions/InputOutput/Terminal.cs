namespace Snobol4.Common;

public partial class Executive
{
                    // ReSharper disable once UnusedMember.Global
    internal Var TerminalIn(Var? v, string _)
    {
        var s = Console.ReadLine();

        if (string.IsNullOrEmpty(s)) 
            return StringVar.Null();
        
        if (AmpTrim != 0)
            s = s.TrimEnd();

        return new StringVar(s, "terminal", true);

    }

                    // ReSharper disable once UnusedMember.Global
    internal Var TerminalOut(Var v, string symbol)
    {
        var s = v.ToString() ?? "";
        Console.Error.WriteLine(s);
        return new StringVar(s);
    }
}