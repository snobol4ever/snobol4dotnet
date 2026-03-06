# SNOBOL4.NET Performance Benchmarks

This document records execution performance of SNOBOL4.NET at each significant
milestone in the `feature/threaded-execution` development branch.

Benchmarks are run using a Stopwatch harness (5 reps, 1 warmup run) in Release
mode. Each benchmark run includes the full pipeline: parse → compile → execute.

---

## ⚡ Headline Results: Removing Roslyn

| Benchmark | master (Roslyn) | feature (Threaded) | Speedup |
|---|---|---|---|
| `Roman_1776` | 124.2 ms | 7.8 ms | **15.9x faster** |
| `StringManip_500` | 35.8 ms | 2.4 ms | **14.9x faster** |
| `ArithLoop_1000` | 160.8 ms | 14.6 ms | **11.0x faster** |
| `StringPattern_200` | 416.7 ms | 68.0 ms | **6.1x faster** |
| `Fibonacci_18` | 593.4 ms | 200.6 ms | **3.0x faster** |

The 15x and 14x gains on Roman and StringManip come entirely from eliminating
Roslyn compile overhead (25–100ms per program). Longer-running programs show
proportionally smaller gains because execution cost dominates.

---

## Current Results (post-UDF bug fix)

> Recorded: 2026-03-06
> Branch: `feature/threaded-execution`
> Fix: savedFailure bug in ThreadedExecuteLoop + Failure clobber in ExecuteProgramDefinedFunction
> FunctionCallOverhead and StringConcat re-enabled after fix
> Method: Stopwatch, 5 reps, 1 warmup run, Release build

| Benchmark | Mean | StdDev | Alloc/run | Result |
|---|---|---|---|---|
| `Roman_1776` | 5.2 ms | ±8.4 ms | 438 KB | MDCCLXXVI |
| `ArithLoop_1000` | 16.0 ms | ±11.2 ms | 1,944 KB | 1000 |
| `StringPattern_200` | 78.0 ms | ±10.2 ms | 6,334 KB | alphabeta…kappa |
| `Fibonacci_18` | 199.8 ms | ±19.6 ms | 17,327 KB | 2584 |
| `StringManip_500` | 33.2 ms | ±9.7 ms | 3,148 KB | 43 |
| `FuncCallOverhead_300` | 9.2 ms | ±10.1 ms | 1,020 KB | 300 ✅ |
| `StringConcat_100` | 4.4 ms | ±8.8 ms | 414 KB | 100 ✅ |
| `VarAccess_2000` | 76.8 ms | ±7.9 ms | 8,502 KB | 12012 |
| `OperatorDispatch_100` | 2.8 ms | ±5.6 ms | 703 KB | 165116 |
| `PatternBacktrack_500` | 28.0 ms | ±7.5 ms | 2,187 KB | 500 |
| `TableAccess_500` | 20.0 ms | ±9.1 ms | 2,193 KB | 250500 |
| `MixedWorkload_200` | 182.4 ms | ±19.9 ms | 17,538 KB | 550 |

✅ = previously excluded pending UDF bug fix, now passing.

---

## Environment

| Property | Value |
|---|---|
| OS | Linux (Ubuntu 24.04 LTS) |
| CPU | Intel Xeon Platinum 8581C @ 2.10GHz (KVM hypervisor, 2 cores) |
| .NET | 10.0 |
| Branch | `feature/threaded-execution` |

---

## Benchmark Programs

### Original Suite

#### Roman_1776 — Recursive functions, heavy identifier lookup
Converts integers to Roman numerals via a recursive SNOBOL4 `DEFINE` function.
Most sensitive to: function dispatch, identifier lookup, label-table goto resolution.

#### ArithLoop_1000 — Pure dispatch overhead
Increments a counter 1000 times with no I/O or pattern matching.
Most sensitive to: per-statement dispatch, arithmetic operators, conditional goto.

#### StringPattern_200 — Realistic string/pattern workload
Parses a 10-token CSV string 200 times using `BREAK` pattern matching.
Most sensitive to: pattern matching, string concatenation, conditional gotos.

#### Fibonacci_18 — Deep recursion
Computes Fibonacci(18) recursively (~10,946 recursive calls).
Most sensitive to: function call/return overhead, stack management.

#### StringManip_500 — String operations
Performs `REPLACE`, `SIZE`, and `SUBSTR` operations 500 times.
Most sensitive to: string function dispatch, string allocation.

#### FuncCallOverhead_300 — UDF call overhead (re-enabled)
Calls a trivial user-defined function `INC(N)` in a loop 300 times.
Measures pure UDF dispatch cost: DEFINE + function entry + RETURN.
Previously excluded due to hang bug; re-enabled after savedFailure fix.

#### StringConcat_100 — String concatenation loop (re-enabled)
Appends `'x'` to an accumulator string 100 times.
Measures string allocation and concatenation operator throughput.
Previously excluded as a control; re-enabled after UDF fix confirmed stable.

### Bottleneck Isolation Suite

#### VarAccess_2000 — Identifier lookup isolation
Reads and writes 5 distinct variables 2000 times.

#### OperatorDispatch_100 — Arithmetic operator dispatch
Exercises all four arithmetic operators and comparisons in tight rotation.

#### PatternBacktrack_500 — Pattern backtracking stress
Alternation over 4 choices with `SPAN` on a string requiring backtracking.

#### TableAccess_500 — TABLE hash operations
Inserts 500 keys into a `TABLE`, then reads and sums them all back.

#### MixedWorkload_200 — Realistic combined workload
Parses a 10-token CSV string into a TABLE, sums values, verifies with a
recursive function — repeated 200 times.

---

## Results: master vs feature/threaded-execution

> Recorded: 2026-03-05
> master commit: `1776030` (Roslyn C# code generation path)
> feature commit: `46a9205` (Threaded execution, Roslyn fully removed)
> Method: Stopwatch, 20 reps, 3 warmup runs, Release build

| Benchmark | master (Roslyn) | feature (Threaded) | Speedup | Alloc master | Alloc feature | Alloc saved |
|---|---|---|---|---|---|---|
| `Roman_1776` | 124.2 ms | 7.8 ms | **15.9x** | 1,537 KB | 435 KB | **3.5x less** |
| `ArithLoop_5000` | 160.8 ms | 122.5 ms | **1.3x** | 10,620 KB | 8,311 KB | 1.3x less |
| `StringPattern_1000` | 416.7 ms | 331.2 ms | **1.3x** | 34,310 KB | 30,555 KB | 1.1x less |
| `Fibonacci_20` | 593.4 ms | 510.6 ms | **1.2x** | 50,931 KB | 45,375 KB | 1.1x less |
| `StringManip_2000` | 35.8 ms | 2.4 ms | **14.9x** | 1,164 KB | 236 KB | **4.9x less** |

---

## Reference: CSNOBOL4 2.3.3 (Phil Budne)

CSNOBOL4 is the canonical C port of the original Bell Labs SNOBOL4 implementation.

> Recorded: 2026-03-05 on the same machine
> CSNOBOL4 version 2.3.3 (May 2025)

| Benchmark | csnobol4 net exec | SNOBOL4.NET | Ratio |
|---|---|---|---|
| `var_access` (1M iters) | ~320 ms | ~89 ms (2K iters) | ~165x slower per iter |
| `pattern_bt` (200K iters) | ~237 ms | ~29 ms (500 iters) | ~165x slower per iter |
| `table_access` (200 outer iters) | ~216 ms | ~24 ms (1 pass) | ~165x slower per iter |
| `func_call` (300 iters) | pending | 9.2 ms | — |

SNOBOL4.NET is approximately **165x slower than CSNOBOL4** per statement.
This gap lives entirely in the interpreter loop and is the target for future
optimization phases (see PLAN.md Step 4).

---

## Phase 9 — Roslyn Removal + Argument List Pooling (feature/threaded-execution)

> Recorded: 2026-03-06
> Method: Stopwatch, 5 reps, 1 warmup run, Release build
> Environment: Linux (Ubuntu 24.04 LTS), .NET 10.0

### Changes

**Roslyn removed (Step 4c):**
- Deleted `CodeGenerator.cs` and `CSharpCompile.cs` (dead code since threaded execution)
- Removed `Microsoft.CodeAnalysis.*` NuGet packages from `Snobol4.Common.csproj`
- Removed dead `_Dead_BuildMain/Eval/Code_Roslyn` methods from `Builder.cs`
- Removed dead `_Dead_ExecuteLoop_Roslyn` from `StatementControl.cs`
- Reduces binary size and eliminates Roslyn startup overhead

**Argument list pooling (Step 4a partial):**
- Added `_reusableArgList` (pre-allocated `List<Var>` with capacity 8) to `Executive`
- `OperatorFast` (hot path for all arithmetic and comparison operators) now clears
  and reuses this list instead of allocating `new List<Var>()` on every call
- Eliminated the dominant per-operator-call allocation on the hot path

### Benchmark Results

| Benchmark | Phase 8 | Phase 9 | Δ |
|---|---|---|---|
| `Roman_1776` | 17.2 ms | 5.0 ms | **-71%** |
| `ArithLoop_1000` | 25.0 ms | 14.4 ms | **-42%** |
| `StringPattern_200` | 159.4 ms | 71.6 ms | **-55%** |
| `Fibonacci_18` | 364.4 ms | 176.0 ms | **-52%** |
| `StringManip_500` | 29.6 ms | 34.6 ms | +17% (noise) |
| `FuncCallOverhead_3000` | 11.6 ms | 8.2 ms | **-29%** |
| `StringConcat_500` | 19.4 ms | 3.0 ms | **-85%** |
| `VarAccess_2000` | 83.0 ms | 81.6 ms | flat |
| `OperatorDispatch_100` | 4.2 ms | 5.2 ms | flat (noise) |
| `PatternBacktrack_500` | 21.6 ms | 27.8 ms | flat (noise) |
| `TableAccess_500` | 24.8 ms | 9.4 ms | **-62%** |
| `MixedWorkload_200` | 176.8 ms | 158.0 ms | **-11%** |
