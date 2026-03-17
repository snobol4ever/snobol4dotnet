using Snobol4.Common;

namespace MathFunction;

/// <summary>
/// Example C# external library demonstrating all argument/return type combinations:
/// integer in/out, real in/out, string in/out, 3-argument functions, and
/// predicate functions that signal success/failure instead of returning a value.
/// </summary>
public class MathFunctions : IExternalLibrary
{
    private Executive? _exec;

    public void Init(Executive executive)
    {
        _exec = executive;
        Register("Add",        2, Add);
        Register("Multiply",   2, Multiply);
        Register("Reverse",    1, Reverse);
        Register("Clamp",      3, Clamp);
        Register("IsPositive", 1, IsPositive);
    }

    // IExternalLibrary.Unload() — default no-op is sufficient

    private void Register(string name, int arity, FunctionTableEntry.FunctionHandler fn)
    {
        var key = _exec!.Parent.FoldCase(name);
        _exec.FunctionTable[key] = new FunctionTableEntry(_exec, key, fn, arity, false);
    }

    /// <summary>Add(a, b) — integer + integer → IntegerVar</summary>
    private void Add(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.INTEGER, out _, out var a, _exec!) ||
            !args[1].Convert(Executive.VarType.INTEGER, out _, out var b, _exec!))
            throw new InvalidCastException("Add: arguments must be numeric");

        _exec!.SystemStack.Push(new IntegerVar((long)a + (long)b));
    }

    /// <summary>Multiply(a, b) — real × real → RealVar</summary>
    private void Multiply(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.REAL, out _, out var a, _exec!) ||
            !args[1].Convert(Executive.VarType.REAL, out _, out var b, _exec!))
            throw new InvalidCastException("Multiply: arguments must be numeric");

        _exec!.SystemStack.Push(new RealVar((double)a * (double)b));
    }

    /// <summary>Reverse(s) — string → StringVar (reversed)</summary>
    private void Reverse(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.STRING, out _, out var s, _exec!))
            throw new InvalidCastException("Reverse: argument must be a string");

        var chars = ((string)s).ToCharArray();
        Array.Reverse(chars);
        _exec!.SystemStack.Push(new StringVar(new string(chars)));
    }

    /// <summary>Clamp(value, lo, hi) — 3 real args → RealVar clamped to [lo,hi]</summary>
    private void Clamp(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.REAL, out _, out var v, _exec!) ||
            !args[1].Convert(Executive.VarType.REAL, out _, out var lo, _exec!) ||
            !args[2].Convert(Executive.VarType.REAL, out _, out var hi, _exec!))
            throw new InvalidCastException("Clamp: arguments must be numeric");

        _exec!.SystemStack.Push(new RealVar(Math.Clamp((double)v, (double)lo, (double)hi)));
    }

    /// <summary>
    /// IsPositive(n) — predicate: succeeds if n > 0, fails otherwise.
    /// Demonstrates a function that signals :S/:F without pushing a return value.
    /// </summary>
    private void IsPositive(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.REAL, out _, out var n, _exec!))
            throw new InvalidCastException("IsPositive: argument must be numeric");

        if ((double)n > 0)
            _exec!.PredicateSuccess();
        else
            _exec!.NonExceptionFailure();
    }
}
