# SNOBOL4.NET — Master Plan
## Branch: `feature/threaded-execution`
## Repo: `https://github.com/jcooper0/Snobol4.Net.git`

---

## Quick Start (New Chat)

```bash
git clone https://github.com/jcooper0/Snobol4.Net.git /home/claude/Snobol4.Net
cd /home/claude/Snobol4.Net
git checkout feature/threaded-execution
export PATH=$PATH:/root/.dotnet
dotnet build -c Release
```

---

## Background

SNOBOL4.NET is a .NET implementation of the SNOBOL4 language, designed to be compatible with
MACRO SPITBOL. It compiles SNOBOL4 source to instructions and executes them via a threaded
execution engine (`ThreadedExecuteLoop`).

**Current status:** The threaded execution refactor (Phases 1-5) is complete and committed.
Benchmarks show **15.9x / 14.9x speedup** over the legacy Roslyn-based interpreter on the
main benchmark suite. Two benchmarks (`FunctionCallOverhead`, `StringConcat`) are still
excluded pending the bug fix below.

---

## STEP 1 — Fix the Self-Referential UDF Hang (DO THIS FIRST)

### The Bug

Any program using a self-referential user-defined function call `R = INC(R)` (same variable
as both argument and assignment target) followed by a conditional goto hangs infinitely.

**Minimal reproduction** (`/tmp/funccall_test.sno`):
```
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
- `snobol4.net` hangs

### Root Cause

**File:** `Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs`
**Line 21:**
```csharp
var savedFailure = ErrorJump > 0;   // BUG: always false
```

`ErrorJump` is cleared to 0 on line 23 (`ErrorJump = 0`), so `ErrorJump > 0` is always
false at the point of this save. On return from any UDF call, `Failure` is unconditionally
restored to `false`, clobbering any failure state set before the call (e.g., by `LT(N,5)`).
The conditional `:F(DONE)` branch never fires; the loop never exits.

### The Fix

Change line 21 from:
```csharp
var savedFailure = ErrorJump > 0;
```
To:
```csharp
var savedFailure = Failure;
```

That is the entire fix.

### Verify

```bash
cd /home/claude/Snobol4.Net
dotnet build -c Release

cat > /tmp/funccall_test.sno << 'EOF'
DEFINE('INC(N)')                    :(INC_END)
INC INC = N + 1                        :(RETURN)
INC_END
    R = 0
    N = 0
LOOP    N = LT(N, 5) N + 1             :F(DONE)
    R = INC(R)                         :(LOOP)
DONE    OUTPUT = R
end
EOF

timeout 10 dotnet run --project Snobol4.Interpreter -c Release -- /tmp/funccall_test.sno
# Expected output: 5
```

### Re-enable Excluded Benchmarks

After the fix, edit `BenchmarkSuite2/Program.cs` — uncomment `FunctionCallOverhead` and
`StringConcat`. Re-run benchmarks and update `BENCHMARKS.md` with new results.

```bash
dotnet run --project BenchmarkSuite2 -c Release
```

### Commit

```bash
git add Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs BenchmarkSuite2/ BENCHMARKS.md
git commit -m "Fix savedFailure bug in ThreadedExecuteLoop: save Failure not ErrorJump

The UDF recursive call path saved 'ErrorJump > 0' as the caller's Failure
state. Since ErrorJump is cleared on the next line, this was always false.
On return from any UDF, Failure was unconditionally reset to false, breaking
any conditional goto that relied on Failure state set before the call.

Caused self-referential UDF calls (R = INC(R) + conditional goto) to loop
infinitely. Fix: save Failure directly."

git push
```

---

## STEP 2 — Complete Regression Testing

The test suite was being run group-by-group when the previous session ended.
**Two groups hang and must be skipped:**
- `Pattern.Bal` — hangs (known issue in pattern matching)
- `Function.InputOutput` — hangs on Linux (uses hardcoded Windows file paths)

### Status as of Last Session

| Test Group | Status | Notes |
|---|---|---|
| ThreadedCompilerTests | PASS | |
| ThreadedExecutionTests | PASS | |
| SlotResolutionTests | PASS | |
| TestGoto (non-direct) | PASS 8/8 | |
| TestGoto (_DIRECT) | FAIL 8/8 | Use CODE() — separate issue, see Step 3c |
| Numeric | PASS 95/95 | |
| Pattern (most groups) | PASS | |
| Pattern.Bal | HANGS | Skip |
| Pattern.Pos TEST_Pos_009 | FAIL 1 | Deferred expression pos(*A) — see Step 3b |
| FunctionControl.Define | PASS 8/8 | |
| FunctionControl.Apply | PASS 5/5 | |
| FunctionControl.Opsyn | FAIL 2 | DLL path + custom operator — see Step 3d/3e |
| Function.InputOutput | HANGS | Skip |
| Gimpel | PASS | |
| ArraysTables | PASS | |
| StringComparison | PASS | |
| StringSynthesis | PASS | |

### Groups Not Yet Run (complete these)

```bash
# Template - replace GROUPNAME with each group below
dotnet test TestSnobol4/TestSnobol4.csproj --no-build \
  --filter "FullyQualifiedName~GROUPNAME" \
  --logger "console;verbosity=minimal" 2>&1 | tail -5
```

Groups to run:
- `FunctionControl.Arg`
- `FunctionControl.Local`
- `FunctionControl.Unload`
- `Function.Memory`
- `Function.Miscellaneous`
- `Function.ProgramDefinedDataType`
- `Compiilation` (note: typo in folder name — two i's)
- `TestLexer`
- `TestParser`
- `TestPredicate`
- `TestSourceReader`
- `TestCommandLine`

After all groups are checked, document results in `REGRESSION.md`.

---

## STEP 3 — Fix Remaining Known Failures

### 3a. Pattern.Bal Hang
`BAL` matches balanced parentheses. Something in threaded execution causes it to loop.
Investigate `ThreadedExecuteLoop` or the `Scanner` backtracking logic when processing a
`BalNode` pattern. Compare execution trace against the non-threaded path.

### 3b. Deferred Expressions in Patterns — pos(*A)
`*A` in a pattern means "evaluate A at match time" (star function / deferred expression).
The `PushExpr` opcode and `StarFunctionList` mechanism has a failure in TEST_Pos_009.
The deferred expression sub-thread is likely being compiled or executed with wrong context.
Check `ThreadedCompiler.cs` for how `*` prefix patterns are compiled and
`ThreadedExecuteLoop.cs` for how `PushExpr` / `RunExpressionThread` is invoked.

### 3c. TestGoto _DIRECT — CODE() Feature
The 8 `_DIRECT` goto tests compile and execute SNOBOL4 code at runtime via `CODE()`.
The `CODE()` function generates a new `Instruction[]` thread (`CompileTarget.CODE`).
The most likely issue: the dynamically-compiled thread is not being executed with the
correct `Thread` field set, or `ThreadedExecuteLoop` is called on it with a wrong start
index. Compare how `ExecuteProgramDefinedFunction` in `Define.cs` sets up threaded
execution vs how `CODE()` in `Snobol4.Common/Runtime/Functions/FunctionControl/Code.cs`
does it.

### 3d. OPSYN Custom Operator — TEST_Opsyn_007
`OPSYN('!', 'any', 1)` should alias `!` to the `ANY` function. The `!` token is likely
not handled in the threaded compiler's operator tokenization. Check `ThreadedCompiler.cs`
for the list of recognized unary operator tokens.

### 3e. DLL Loading Tests — TEST_Opsyn_001
Uses hardcoded path `C:\Users\...\AreaLibrary.dll`. The DLL doesn't exist in the repo.
Add `[Ignore("Requires local build of AreaLibrary.dll")]` to this test on Linux, or
restructure the test to build the DLL from source as part of test setup.

---

## STEP 4 — Performance: Next Optimization Ideas

The threaded execution refactor achieved 15.9x speedup. These are the next opportunities:

### 4a. Eliminate Argument List Allocation per Function Call (High Impact, Medium Effort)
Every function call does `List<Var> arguments = []` and fills it from the stack. For
built-in functions with fixed arity, this is a heap allocation on every call.
Replace with a pooled `Var[]` argument buffer that gets reused across calls.
UDFs are the hot path (FunctionCallOverhead benchmark).

**Files:** `Function.cs`, `Operator.cs`, `FunctionTableEntry.cs`

### 4b. Integer Fast Path for Arithmetic (High Impact, Medium Effort)
`LT`, `GT`, `EQ`, and arithmetic operators go through full `Operator()` -> `FunctionTable`
-> handler dispatch even when both operands are `IntegerVar`. Add a fast path in
`ThreadedExecuteLoop` that, for the common numeric opcodes, checks if both stack operands
are `IntegerVar` and performs the operation inline without full dispatch overhead.
The SPITBOL approach was to specialize on the most common type combinations.

**Files:** `ThreadedExecuteLoop.cs`, new `ArithmeticFast.cs`

### 4c. Remove Roslyn / Static Compilation Path (Medium Impact, Low Risk)
The Roslyn C# text generation path (`CodeGenerator.cs`, `RoslynCompiler.cs`) is now dead
code — replaced by the threaded compiler. Removing it cleans up the codebase and
eliminates the Roslyn NuGet dependency (reduces startup time and binary size).

**Before removing:** Verify `CODE()` and `EVAL()` do not still use the Roslyn path.
If they do, port them to use the threaded compiler for dynamic code blocks first.

**Files to delete:** `Snobol4.Common/Builder/CodeGenerator.cs`,
`Snobol4.Common/Builder/RoslynCompiler.cs`
**Also:** Remove Roslyn NuGet packages from `Snobol4.Common.csproj`.

### 4d. Clone Elimination for Numeric Constants (Low Effort, Moderate Impact)
`IntegerVar` and `RealVar` from the constant pool are currently cloned on every push to
avoid aliasing. Since integers and reals are immutable-in-practice in SNOBOL4, the clone
can be skipped for constants. This was explored in Phase 9 but reverted due to test
failures — re-investigate the specific failure before trying again.

### 4e. Peephole: Combine Numeric Compare + Conditional Goto
The pattern `N = LT(N, 5) N + 1 :F(DONE)` is extremely common. A peephole optimizer in
the threaded compiler could detect this pattern and emit a combined
`CompareAndBranchFail` opcode, removing 4-6 individual opcode dispatches per loop
iteration.

### 4f. Long Term: Direct IL Compilation (High Impact, High Effort)
Rather than a stack-machine interpreter, compile SNOBOL4 directly to .NET IL using
`System.Reflection.Emit`. The main obstacles are SNOBOL4's dynamic features:
- `OPSYN` can rename any operator at runtime
- `GOTO($var)` jumps to a runtime string label
- `EVAL`/`CODE` compile new code into the live environment

A realistic middle path: replace string keys in `FunctionTable` and `IdentifierTable`
with integer slot indices at compile time (already done for variable slots). Then inline
the most common fixed-arity operations (arithmetic, string ops) as direct method calls
rather than table-dispatched handlers.

---

## STEP 5 — Before Merging to Master

1. All regression tests pass or known failures are documented with issues filed
2. `BENCHMARKS.md` updated with post-fix numbers (including FunctionCallOverhead, StringConcat)
3. This `PLAN.md` removed or archived (do not merge working notes to master)
4. PR description written: what the threaded execution refactor is, before/after benchmark
   numbers, known remaining issues

---

## Architecture Reference

### Key Files

| File | Purpose |
|---|---|
| `Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs` | Main execution loop — hot path |
| `Snobol4.Common/Runtime/Execution/StatementControl.cs` | `ExecuteLoop()` wrapper, `RunExpressionThread()` |
| `Snobol4.Common/Runtime/Execution/Function.cs` | UDF dispatch |
| `Snobol4.Common/Runtime/Functions/FunctionControl/Define.cs` | UDF handler — saves/restores param slots |
| `Snobol4.Common/Builder/ThreadedCompiler.cs` | Compiles SNOBOL4 AST to Instruction[] |
| `Snobol4.Common/Builder/OpCode.cs` | All opcode definitions |
| `BenchmarkSuite2/Benchmarks.cs` | Benchmark programs (SNOBOL4 source as strings) |
| `BenchmarkSuite2/Program.cs` | Benchmark runner harness |
| `BENCHMARKS.md` | Recorded benchmark results |

### Execution Flow

```
SNOBOL4 source
  -> Lexer -> Parser -> AST
  -> ThreadedCompiler -> Instruction[] (the "thread")
  -> ThreadedExecuteLoop (hot loop over Instruction[])
      -> OpCode.CallFunc -> Function() -> ExecuteProgramDefinedFunction()
          -> ThreadedExecuteLoop (recursive, saves/restores state on C# stack)
```

### The savedFailure Bug (for reference)

```csharp
// ThreadedExecuteLoop entry — BEFORE fix:
var savedFailure = ErrorJump > 0;   // always false (ErrorJump cleared next line)
var savedErrorJump = ErrorJump;
ErrorJump = 0;

// ThreadedExecuteLoop entry — AFTER fix:
var savedFailure = Failure;          // correct: saves actual Failure state
var savedErrorJump = ErrorJump;
ErrorJump = 0;

// Exit (unchanged, now correct with fix):
Failure = savedFailure;
```

### Benchmark Results (current — FunctionCallOverhead/StringConcat excluded)

| Benchmark | snobol4.net | csnobol4 | Speedup |
|---|---|---|---|
| ArithLoop | ~5ms | ~80ms | ~15.9x |
| StringPattern | ~12ms | ~178ms | ~14.9x |
| Fibonacci | ~180ms | — | — |
| StringManip | ~45ms | — | — |

Update these after Step 1 re-enables the excluded benchmarks.
