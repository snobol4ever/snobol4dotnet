using Snobol4.Common;

namespace ObjectLifecycle;

/// <summary>
/// Step 7 fixture library: exposes ARRAY, TABLE, and POINT lifecycle operations
/// as SNOBOL4 external functions via IExternalLibrary.
///
/// ARRAY group  — MakeArray, ArraySet, ArrayGet, ArraySum, ArrayClear
/// TABLE group  — MakeTable, TablePut, TableGet, TableKeys, TableWipe
/// POINT group  — MakePoint, PointX, PointY, PointMove, PointReset
///
/// All operations use the public Executive object-lifecycle API (ExecutiveObjectApi.cs).
/// POINT is backed by a 2-slot ArrayVar (slot 1 = x, slot 2 = y).
/// </summary>
public class ObjectLifecycleFunctions : IExternalLibrary
{
    private Executive? _exec;

    public void Init(Executive executive)
    {
        _exec = executive;
        Register("MakeArray",  1, MakeArray);
        Register("ArraySet",   3, ArraySet);
        Register("ArrayGet",   2, ArrayGet);
        Register("ArraySum",   1, ArraySum);
        Register("ArrayClear", 1, ArrayClear);
        Register("MakeTable",  0, MakeTable);
        Register("TablePut",   3, TablePut);
        Register("TableGet",   2, TableGet);
        Register("TableKeys",  1, TableKeys);
        Register("TableWipe",  1, TableWipe);
        Register("MakePoint",  2, MakePoint);
        Register("PointX",     1, PointX);
        Register("PointY",     1, PointY);
        Register("PointMove",  3, PointMove);
        Register("PointReset", 1, PointReset);
    }

    private void Register(string name, int arity, FunctionTableEntry.FunctionHandler fn)
    {
        var key = _exec!.Parent.FoldCase(name);
        _exec.FunctionTable[key] = new FunctionTableEntry(_exec, key, fn, arity, false);
    }

    // ── ARRAY ─────────────────────────────────────────────────────────────

    private void MakeArray(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.INTEGER, out _, out var nObj, _exec!))
            throw new InvalidCastException("MakeArray: argument must be an integer");
        var arr = _exec!.CreateArray((long)nObj)
            ?? throw new ArgumentException("MakeArray: invalid size");
        _exec.SystemStack.Push(arr);
        _exec.Failure = false;
    }

    private void ArraySet(List<Var> args)
    {
        var arr = args[0] as ArrayVar ?? throw new InvalidCastException("ArraySet: arg0 must be ArrayVar");
        if (!args[1].Convert(Executive.VarType.INTEGER, out _, out var idxObj, _exec!))
            throw new InvalidCastException("ArraySet: index must be integer");
        _exec!.ArraySet(arr, (long)idxObj, args[2]);
        _exec.SystemStack.Push(arr);
        _exec.Failure = false;
    }

    private void ArrayGet(List<Var> args)
    {
        var arr = args[0] as ArrayVar ?? throw new InvalidCastException("ArrayGet: arg0 must be ArrayVar");
        if (!args[1].Convert(Executive.VarType.INTEGER, out _, out var idxObj, _exec!))
            throw new InvalidCastException("ArrayGet: index must be integer");
        _exec!.SystemStack.Push(_exec.ArrayGet(arr, (long)idxObj));
        _exec.Failure = false;
    }

    private void ArraySum(List<Var> args)
    {
        var arr = args[0] as ArrayVar ?? throw new InvalidCastException("ArraySum: arg0 must be ArrayVar");
        long total = 0;
        foreach (var v in _exec!.ArrayData(arr))
            if (v.Convert(Executive.VarType.INTEGER, out _, out var iv, _exec))
                total += (long)iv;
        _exec.SystemStack.Push(new IntegerVar(total));
        _exec.Failure = false;
    }

    private void ArrayClear(List<Var> args)
    {
        var arr = args[0] as ArrayVar ?? throw new InvalidCastException("ArrayClear: arg0 must be ArrayVar");
        _exec!.ArrayFillEmpty(arr);
        _exec.SystemStack.Push(arr);
        _exec.Failure = false;
    }

    // ── TABLE ─────────────────────────────────────────────────────────────

    private void MakeTable(List<Var> args)
    {
        _exec!.SystemStack.Push(_exec.CreateTable());
        _exec.Failure = false;
    }

    private void TablePut(List<Var> args)
    {
        var tbl = args[0] as TableVar ?? throw new InvalidCastException("TablePut: arg0 must be TableVar");
        _exec!.TablePut(tbl, args[1], args[2]);
        _exec.SystemStack.Push(tbl);
        _exec.Failure = false;
    }

    private void TableGet(List<Var> args)
    {
        var tbl = args[0] as TableVar ?? throw new InvalidCastException("TableGet: arg0 must be TableVar");
        _exec!.SystemStack.Push(_exec.TableGet(tbl, args[1]));
        _exec.Failure = false;
    }

    private void TableKeys(List<Var> args)
    {
        var tbl = args[0] as TableVar ?? throw new InvalidCastException("TableKeys: arg0 must be TableVar");
        var keys = _exec!.TableKeys(tbl).ToList();
        if (keys.Count == 0)
        {
            _exec.SystemStack.Push(new IntegerVar(0));
            _exec.Failure = false;
            return;
        }
        var arr = _exec.CreateArray(keys.Count)!;
        for (var i = 0; i < keys.Count; i++)
            _exec.ArraySet(arr, i + 1, new StringVar(keys[i].ToString() ?? ""));
        _exec.SystemStack.Push(arr);
        _exec.Failure = false;
    }

    private void TableWipe(List<Var> args)
    {
        var tbl = args[0] as TableVar ?? throw new InvalidCastException("TableWipe: arg0 must be TableVar");
        _exec!.TableWipe(tbl);
        _exec.SystemStack.Push(tbl);
        _exec.Failure = false;
    }

    // ── POINT (2-slot ArrayVar: slot1=x, slot2=y) ─────────────────────────

    private ArrayVar NewPoint(long x, long y)
    {
        var pt = _exec!.CreateArray(2)!;
        _exec.ArraySet(pt, 1, new IntegerVar(x));
        _exec.ArraySet(pt, 2, new IntegerVar(y));
        return pt;
    }

    private long GetCoord(ArrayVar pt, long slot)
    {
        var v = _exec!.ArrayGet(pt, slot);
        if (v is IntegerVar iv) return iv.Data;
        return v.Convert(Executive.VarType.INTEGER, out _, out var o, _exec) ? (long)o : 0;
    }

    private void MakePoint(List<Var> args)
    {
        if (!args[0].Convert(Executive.VarType.INTEGER, out _, out var xo, _exec!) ||
            !args[1].Convert(Executive.VarType.INTEGER, out _, out var yo, _exec!))
            throw new InvalidCastException("MakePoint: arguments must be integers");
        _exec!.SystemStack.Push(NewPoint((long)xo, (long)yo));
        _exec.Failure = false;
    }

    private void PointX(List<Var> args)
    {
        var pt = args[0] as ArrayVar ?? throw new InvalidCastException("PointX: arg0 must be ArrayVar");
        _exec!.SystemStack.Push(_exec.ArrayGet(pt, 1));
        _exec.Failure = false;
    }

    private void PointY(List<Var> args)
    {
        var pt = args[0] as ArrayVar ?? throw new InvalidCastException("PointY: arg0 must be ArrayVar");
        _exec!.SystemStack.Push(_exec.ArrayGet(pt, 2));
        _exec.Failure = false;
    }

    private void PointMove(List<Var> args)
    {
        var pt = args[0] as ArrayVar ?? throw new InvalidCastException("PointMove: arg0 must be ArrayVar");
        if (!args[1].Convert(Executive.VarType.INTEGER, out _, out var dxo, _exec!) ||
            !args[2].Convert(Executive.VarType.INTEGER, out _, out var dyo, _exec!))
            throw new InvalidCastException("PointMove: dx/dy must be integers");
        _exec!.SystemStack.Push(NewPoint(GetCoord(pt, 1) + (long)dxo, GetCoord(pt, 2) + (long)dyo));
        _exec.Failure = false;
    }

    private void PointReset(List<Var> args)
    {
        _exec!.SystemStack.Push(NewPoint(0, 0));
        _exec.Failure = false;
    }
}
