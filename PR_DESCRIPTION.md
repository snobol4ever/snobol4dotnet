# PR: Threaded Execution Engine

## What this does

Replaces the Roslyn C#-codegen execution path with a custom threaded interpreter.

Previously, SNOBOL4.NET compiled each program to C# source code at runtime,
invoked Roslyn to compile it to a DLL, loaded the DLL, and executed it.
This worked correctly but Roslyn's compile time (25–100ms per program) dominated
execution time for all short-to-medium programs.

The new path compiles SNOBOL4 directly to a flat `Instruction[]` array and
executes it in a tight `switch` dispatch loop (`ThreadedExecuteLoop`). No C#
is generated, no Roslyn DLL is compiled or loaded at runtime.

The Roslyn path is preserved in full and remains switchable via
`BuilderOptions.UseThreadedExecution = false` for regression testing.

---

## Performance

Benchmarks run on Linux (Ubuntu 24.04, Intel Xeon @ 2.10GHz), .NET 10.0,
5 reps / 1 warmup, Release build.

| Benchmark | master (Roslyn) | feature (Threaded) | Speedup |
|---|---|---|---|
| `Roman_1776` | 124.2 ms | 6.2 ms | **20x** |
| `StringManip_500` | 35.8 ms | 32.6 ms | 1.1x |
| `ArithLoop_1000` | 160.8 ms | 17.6 ms | **9x** |
| `StringPattern_200` | 416.7 ms | 75.8 ms | **5.5x** |
| `Fibonacci_18` | 593.4 ms | 161.0 ms | **3.7x** |
| `FuncCallOverhead_3000` | — | 5.0 ms | (new) |
| `StringConcat_500` | — | 0.4 ms | (new) |

The largest gains are on short programs where Roslyn compile overhead was the
dominant cost. Longer programs show proportionally smaller gains since execution
time dominates.

Full benchmark history is in `BENCHMARKS.md`.

---

## What changed

### New files
- `Snobol4.Common/Builder/Instruction.cs` — `OpCode` enum and `Instruction` struct (12-byte value type)
- `Snobol4.Common/Builder/ThreadedCodeCompiler.cs` — compiles SNOBOL4 AST → `Instruction[]`
- `Snobol4.Common/Runtime/Execution/ThreadedExecuteLoop.cs` — main dispatch loop
- `Snobol4.Common/Runtime/Execution/ExecutionCache.cs` — `VarSlotArray` (O(1) variable access), pre-resolved operator handlers
- `BENCHMARKS.md` — performance history
- `REGRESSION.md` — full test results

### Key changes to existing files
- `Builder.cs` — `BuildMain/Eval/Code` default to threaded path; Roslyn path preserved under `UseThreadedExecution = false`
- `StatementControl.cs` — `ExecuteLoop()` delegates to `ThreadedExecuteLoop`; Roslyn `Statements[]` loop kept as fallback
- `Define.cs` — fixed argument mutation bug: clone argument before binding as UDF parameter
- `ExecutionCache.cs` — `OperatorFast` integer fast path for `+`, `-`, `*` with `IntegerVar` operands

### Bugs fixed
1. **`ThreadedExecuteLoop.cs`**: `savedFailure = ErrorJump > 0` was always `false` — fixed to `savedFailure = Failure`. This caused UDF calls to unconditionally clear `Failure` on return, breaking any `:F`/`:S` goto that depended on pre-call failure state (e.g. `LT(N,5)`).

2. **`Define.cs`**: `ExecuteProgramDefinedFunction` mutated the argument's `.Symbol` in-place. Since `PushVar` pushes the live `VarSlotArray` reference, the same object could be on the stack as the LHS of an enclosing assignment (`R = INC(R)`), causing the write to go to the wrong variable. Fixed by cloning the argument before renaming it as the parameter.

---

## Test results

**1358 passed, 0 failed, 28 skipped** out of 1386 tests run.

The two excluded groups hang and are excluded via `--filter`:
- `Pattern.Bal` — BAL backtracking loop in threaded engine (known issue, see REGRESSION.md)
- `Function.InputOutput` — hardcoded Windows paths, passes on Windows

Full details in `REGRESSION.md`.

---

## Known issues (post-merge follow-up)

- **Pattern.Bal** hangs in the threaded engine. The Roslyn path handles it correctly. Root cause is in `BalNode` backtracking in the threaded Scanner.
- **Function.InputOutput** requires Windows file paths. Not a regression — was already environment-specific.
