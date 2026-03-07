# MSIL Delegate Performance Report

**Comparing:** `master` (Roslyn code generation) vs `feature/msil-trace` (MSIL `DynamicMethod` delegates)  
**Date:** 2026-03-07  
**Platform:** Linux / .NET 10.0.103 / Release build  
**Methodology:** 3 warmup runs + 15 timed runs per benchmark, median reported. Each run calls `BuildMain()` from scratch (compile + execute), so both compile-time and execute-time costs are included.

---

## Results

| Benchmark | `master` (Roslyn) | `feature/msil-trace` | Speedup |
|---|---:|---:|:---:|
| Roman numerals (recursive, 4 calls) | 96 ms | 7 ms | **13.7×** |
| Fibonacci(20) recursive | 591 ms | 322 ms | **1.8×** |
| Counter loop 10,000 iters | 168 ms | 97 ms | **1.7×** |
| Pattern scan (vowel count) | 40 ms | 4 ms | **10.3×** |
| String build 500 concat | 39 ms | 18 ms | **2.2×** |

---

## What's Being Measured

Each benchmark compiles and runs a complete SNOBOL4 program. The total wall-clock time includes:

1. **Parse** — lexing and building the parse tree (identical on both branches)
2. **Compile** — where the paths diverge significantly
3. **Execute** — where the runtime dispatch model differs

### master — Roslyn code generation

`BuildMain()` generates a full C# source file from the SNOBOL parse tree, then invokes the Roslyn compiler (`CSharpCompilation.EmitToStream`) to produce a real .NET assembly, loads it with `AssemblyLoadContext`, and reflectively instantiates and calls the generated class. Statements execute as `Statements[i](this)` — each is a compiled `Func<Executive, int>` method from the generated class.

The Roslyn pipeline is the dominant cost on short programs. Even a program with 5 statements pays the full Roslyn startup overhead (~30–100ms on this machine depending on JIT state).

### feature/msil-trace — MSIL DynamicMethod delegates

`BuildMain()` calls `EmitMsilForAllStatements`, which walks the parse tree and emits `DynamicMethod` delegates directly via `System.Reflection.Emit.ILGenerator`. No intermediate C# source is generated; no Roslyn invocation happens. Each statement becomes a `Func<Executive, int>` built with `DynamicMethod(owner: typeof(Executive), skipVisibility: true)`.

The thread is reduced to `CallMsil × N + Halt`. Every element of every statement — Init, expression body, Finalize, all goto logic, and TRACE hooks — is inside the delegate. The execute loop's hot path is a tight two-case loop with no `switch` overhead.

---

## Why Fibonacci Sees Less Improvement

Fibonacci(20) makes ~21,891 recursive calls. Each recursive call goes through `Define.cs`→`RunExpressionThread`, which reuses the *same* compiled thread — the MSIL delegates are compiled once and reused across all recursive calls. The residual ~322ms is dominated by the actual recursive computation overhead (stack operations, `ExecuteLoop` re-entry, function dispatch) which is not yet eliminated by the MSIL work.

The master's 591ms for the same program includes the same recursive overhead *plus* Roslyn compilation. The ~1.8× gap on Fibonacci reflects that Roslyn's one-time cost is amortized across 21,891 calls — the per-call overhead from the old dispatch model still dominates over the new one in this case.

---

## Why Roman and Pattern Show the Most Improvement

**Roman (13.7×):** A short program where Roslyn's fixed startup cost dominates on master. The MSIL path compiles in microseconds, making the per-run overhead nearly zero for programs this size. The goto absorption (Steps 9–11) also eliminates all the `GotoIndirect` / `SaveFailure` / `CheckGotoFailure` machinery from the execute loop.

**Pattern scan (10.3×):** Similar profile — short program, tight scan loop, Roslyn startup dominates the master measurement. The MSIL path's tight execute loop and direct `CallMsil` dispatch make the per-statement overhead negligible.

---

## Architecture Changes (Steps 1–13)

| Step | Change | Effect |
|------|--------|--------|
| 1–5 | MSIL emitter: expression bodies → `DynamicMethod` delegates | Eliminated per-opcode `switch` for expression evaluation |
| 6 | `Init`/`Finalize` inlined into delegates | Removed 2 `switch` cases per statement from hot path |
| 7 | Delegates: `Action<Executive>` → `Func<Executive, int>` | Return value carries next-IP; goto decisions stay in delegate |
| 8 | Fall-through `Jump` absorbed (`int.MinValue` convention) | `Jump` opcode eliminated from thread |
| 9 | Unconditional `:(LABEL)` absorbed | `SaveFailure` + `GotoIndirect` + 3 other opcodes eliminated |
| 10 | Conditional `:S(L)`/`:F(L)` absorbed | `JumpOnSuccess`/`JumpOnFailure` eliminated |
| 11 | Indirect `:(EXPR)`, `:<VAR>` absorbed | All remaining goto opcodes eliminated |
| 12 | Fast execute loop path | `switch` replaced by two-case tight loop for pure-MSIL threads |
| 13 | Full TRACE hooks in helper methods | Feature parity with master; zero cost when `AmpTrace = 0` |

Thread instruction count for `Roman.sno`:

| Metric | master (Roslyn) | feature/msil-trace |
|--------|----------------:|-----------:|
| Thread instructions | N/A (generated class) | 9 |
| Goto opcodes in thread | N/A | 0 |
| Init/Finalize opcodes in thread | N/A | 0 |
| Compile path | Roslyn (C# → assembly) | ILGenerator (direct IL emission) |

---

## Test Coverage

| Branch | Tests |
|--------|------:|
| `master` | 1,271 passing |
| `feature/msil-trace` | 1,464 passing (+193) |

The +193 tests cover the threaded execution pipeline (Steps 1–12) and all TRACE hook paths (Step 13).

---

## Next Step

**Step 14 (stretch goal):** Eliminate the `Instruction[]` array entirely for fully-compiled programs — dispatch directly from a `Func<Executive, int>[]` array indexed by IP. This removes the last struct-dereference indirection between the fast loop and the JIT delegates, and allows the GC to collect the instruction array after compilation.
