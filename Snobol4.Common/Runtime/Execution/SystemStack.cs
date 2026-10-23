namespace Snobol4.Common;

// The operand stack of statement evaluation. Its own array of struct slots, not a Stack<Var>: storing a Var in a slot is a plain field
// store, where a Var[] store pays the covariance check every reference-array store does, and Peek(n) reads the slot n below the top,
// where it was LINQ's ElementAt, which enumerated a boxed enumerator from the top on every operator (measured 2026-10-22 with perf on a
// one-statement loop: the two were about 14 percent of its time). It enumerates top first, as Stack<Var> did, and an empty Pop or
// Peek throws InvalidOperationException as Stack<Var> does.
public sealed class SystemStack : IEnumerable<Var>
{
    private struct Slot
    {
        public Var Value;
    }

    private Slot[] _slots = new Slot[64];
    private int _count;

    public int Count => _count;

    public void Push(Var v)
    {
        if (_count == _slots.Length)
            Array.Resize(ref _slots, _count * 2);
        _slots[_count++].Value = v;
    }

    public Var Pop()
    {
        if (_count == 0)
            throw new InvalidOperationException("Stack empty.");
        ref var slot = ref _slots[--_count];
        var v = slot.Value;
        slot.Value = null!;
        return v;
    }

    // The value count places below the top (0 is the top)
    public Var Peek(int count = 0)
    {
        if ((uint)count >= (uint)_count)
            throw count == 0 && _count == 0 ? new InvalidOperationException("Stack empty.") : new ArgumentOutOfRangeException(nameof(count));
        return _slots[_count - 1 - count].Value;
    }

    public void Clear()
    {
        Array.Clear(_slots, 0, _count);
        _count = 0;
    }

    public IEnumerator<Var> GetEnumerator()
    {
        for (var i = _count - 1; i >= 0; i--)
            yield return _slots[i].Value;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ExtractArguments(int count, List<Var> arguments, Executive x, int start = 0)
    {
        // Pop args off stack (they arrive in reverse order) then reverse to
        // restore call order.  Add() is O(1) amortised; Insert(0,...) was O(n²).
        int insertStart = arguments.Count;
        for (var i = 0; i < count; ++i)
            arguments.Add(Pop());
        arguments.Reverse(insertStart, count);

        var succeeded = true;
        foreach (var arg in arguments)
            succeeded &= arg.Succeeded;
        if (succeeded)
            return false;

        x.PushFailedValue();
        return true;
    }

    /// <summary>
    /// Array-based variant for the hot path — avoids List allocation.
    /// Fills <paramref name="buf"/> in argument order (index 0 = first arg).
    /// Returns true if any argument failed (Succeeded == false), pushing a
    /// null StringVar sentinel and signalling the caller to abort.
    /// </summary>
    public bool ExtractArgumentsToArray(int count, Var[] buf)
    {
        for (var i = count - 1; i >= 0; --i)
            buf[i] = Pop();

        for (var i = 0; i < count; ++i)
            if (!buf[i].Succeeded)
            {
                Push(new StringVar(false) { Succeeded = false });
                return true;
            }

        return false;
    }
}
