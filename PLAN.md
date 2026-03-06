# Bug Fix Plan: Self-Referential UDF Call Hang

## Status
This plan was written at the end of a previous chat session. Pick it up fresh in a new chat by
reading this file and then resuming work. No further context is needed.

## Repository
`/home/claude/Snobol4.Net`  
Branch: `feature/threaded-execution`

---

## The Bug

**Symptom**: Any SNOBOL4 program that uses a self-referential user-defined function call of the
form `R = INC(R)` (same variable as argument and assignment target) followed by any conditional
goto causes an infinite loop. The program never terminates.

**Confirmed minimal reproduction** (`/tmp/funccall_test.sno`):
```snobol
DEFINE('INC(N)')                    :(INC_END)
INC INC = N + 1                        :(RETURN)
INC_END
    R = 0
    N = 0
LOOP    N = LT(N, 5) N + 1             :F(DONE)
    R = INC(R)                         :(LOOP)
DONE    OUTPUT = R
end
```
- `csnobol4` outputs `5` (correct)
- `snobol4.net` hangs (timeout after 5s)

**Observed behavior in trace**:
After the first iteration, the trace shows `N=2  R=0` repeating infinitely.
- N increments to 2 then freezes (never reaches 3)
- R stays 0 (INC is not actually incrementing it)
- The loop never terminates

---

## Root Cause: Confirmed

**File**: `Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs`  
**Line 21**:
```csharp
var savedFailure = ErrorJump > 0;   // ← BUG
```
**Should be**:
```csharp
var savedFailure = Failure;          // ← FIX
```

### Why This Causes the Hang

`ThreadedExecuteLoop` is called recursively whenever a user-defined function is invoked (the call
chain is: `CallFunc` opcode → `Function()` → `ExecuteLoop()` → `ThreadedExecuteLoop()`).

At entry, the method saves the caller's state:
```csharp
var savedFailure   = ErrorJump > 0;  // BUG: always false because ErrorJump is cleared on line 23
var savedErrorJump = ErrorJump;
ErrorJump = 0;                       // line 23 — this is why ErrorJump is always 0 here
```

Because `ErrorJump` is always 0 at the point of the save (it gets cleared on the very next line),
`savedFailure` is **always `false`**, regardless of the actual `Failure` state.

At exit (line 222):
```csharp
Failure = savedFailure;   // always restores false — clobbers whatever Failure was after the UDF
```

For the self-referential case `R = INC(R)`:
1. The `LT(N, 5)` comparison sets `Failure = true` when N reaches 5 (triggering the `:F(DONE)` branch)
2. But `R = INC(R)` is evaluated next — inside the UDF call, `ThreadedExecuteLoop` recursively saves
   and restores `Failure`
3. On return, `Failure` is unconditionally set to `false` (because `savedFailure` is always false)
4. The `:F(DONE)` branch never fires; the loop never exits
5. The self-referential assignment also means the argument read and the result write share the same
   slot, compounding the state corruption

---

## The Fix

### Step 1: Fix `savedFailure` in `ThreadedExecuteLoop.cs`

**File**: `Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs`

Change line 21 from:
```csharp
var savedFailure    = ErrorJump > 0;
```
To:
```csharp
var savedFailure    = Failure;
```

That's the entire fix. The rest of the save/restore block is correct.

### Step 2: Verify the Fix

Run the minimal repro:
```bash
cd /home/claude/Snobol4.Net
dotnet build -c Release
echo "DEFINE('INC(N)')                    :(INC_END)
INC INC = N + 1                        :(RETURN)
INC_END
    R = 0
    N = 0
LOOP    N = LT(N, 5) N + 1             :F(DONE)
    R = INC(R)                         :(LOOP)
DONE    OUTPUT = R
end" > /tmp/funccall_test.sno
dotnet run --project Snobol4.Interpreter -c Release -- /tmp/funccall_test.sno
```
Expected output: `5`

Also run the StringConcat benchmark which was excluded for the same hang reason:
```bash
dotnet run --project BenchmarkSuite2 -c Release
```
Both `FunctionCallOverhead` and `StringConcat` benchmarks should now complete without hanging.

### Step 3: Re-enable Excluded Benchmarks

After fixing, edit `BenchmarkSuite2/Program.cs` (or wherever `FunctionCallOverhead` and
`StringConcat` are commented out) and uncomment them. Re-run the full benchmark suite and
update `BENCHMARKS.md` with the new results.

### Step 4: Run Full Regression Test Suite

```bash
cd /home/claude/Snobol4.Net
dotnet test
```
All tests should pass. Pay particular attention to any UDF-related tests.

### Step 5: Commit

```bash
git add Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs
git commit -m "Fix savedFailure bug in ThreadedExecuteLoop: save Failure not ErrorJump

The UDF recursive call path saved 'ErrorJump > 0' as the caller's Failure
state. Since ErrorJump is cleared on the next line, this was always false.
On return from any UDF, Failure was unconditionally reset to false, breaking
any conditional goto that relied on Failure state set before the call.

This caused self-referential UDF calls (R = INC(R) followed by :F or :S
branches) to loop infinitely. Fix: save Failure directly."
```

---

## Additional Context

### Files Examined
- `Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs` — contains the bug (line 21)
- `Snobol4.Common/Runtime/Execution/StatementControl.cs` — `ExecuteLoop()` wraps `ThreadedExecuteLoop()`; `RunExpressionThread()` for expression sub-programs
- `Snobol4.Common/Runtime/Execution/Function.cs` — `Function()` dispatches to `functionEntry.Handler()`
- `Snobol4.Common/Runtime/Functions/FunctionControl/Define.cs` — `ExecuteProgramDefinedFunction()` saves/restores param/local variable slots correctly; calls `ExecuteLoop()` on line 175

### Why Only Self-Referential Calls Hang
Non-self-referential calls (e.g. `X = INC(R)`) also have the `savedFailure` bug, but because the
assignment target X doesn't share a slot with the argument, the result ends up in the right place.
The loop still completes (incorrectly in some edge cases involving Failure state), but doesn't hang
visibly in simple tests. Self-referential `R = INC(R)` causes the argument to be read, the slot
cleared, the result written to the wrong state — combined with Failure clobbering, the program
counter jumps back to LOOP indefinitely.

### Previous Benchmarking Work
The benchmark suite is in good shape with headline results of **15.9x / 14.9x** speedup over
the legacy interpreter. `FunctionCallOverhead` and `StringConcat` were excluded pending this fix.
See `BENCHMARKS.md` for full details.

### Isolation Tests That Confirmed the Pattern
These all hang with `snobol4.net`, pass with `csnobol4`:
- `R = INC(R)` then any conditional goto ← minimal trigger
- Any self-referential UDF assignment followed by `:S` or `:F` branch

These all work correctly (not self-referential or no conditional after call):
- `X = INC(R)` (different target)
- `R = INC(42)` (constant argument)
- `R = NOP(R)` (NOP returns arg unchanged — actually works, possibly because Failure state coincidentally correct)
- Single `INC(41)` call with no subsequent conditional
