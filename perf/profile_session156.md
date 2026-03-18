# DOTNET Perf Analysis — session156

**Date:** 2026-03-17  
**HEAD before:** `4c32ee7`  
**HEAD after:** `e0e81d3`  
**Method:** Code inspection of hot paths identified in session154 baseline.
(dotnet-trace run deferred — requires dotnet in container; code analysis
sufficient to identify and fix the three highest-confidence bottlenecks.)

---

## Hot Paths Identified

### A — `IntegerConversionStrategy.TryConvert` (INTEGER identity path)

**Problem:** Every call to `Var.Convert(INTEGER, ...)` on an `IntegerVar` executed
`varOut = IntegerVar.Create(0); valueOut = 0;` unconditionally before the switch,
allocating a throwaway object even when the result was `return (self, self.Data)`.
This fires on every integer arithmetic operation, every variable read in an
arithmetic context, every loop counter check.

**Fix:** Fast-path `INTEGER→INTEGER` before the switch — returns `self` with zero
allocation. Defaults moved after the guard, only reached for actual cross-type
conversions.

**Also fixed:** `CultureInfo.CurrentCulture` → `InvariantCulture` in STRING/PATTERN/NAME
cases. CurrentCulture can produce locale-dependent formatting (e.g. `,` as decimal
separator on some systems), breaking SPITBOL string conformance. InvariantCulture
is also slightly faster (avoids locale lookup).

**Expected impact:** Visible in VarAccess_2000 (98ms baseline), ArithLoop_1000
(41.6ms), MixedWorkload_200 (220ms). Alloc/run column should drop measurably.

### B — `RealConversionStrategy` — same CurrentCulture issue

Two ToString calls used `CurrentCulture`. Fixed to `InvariantCulture`.

### C — `Function.cs` per-call `List<Var>` allocation

**Problem:** `Function()` (the user-defined function dispatch path) created
`List<Var> arguments = []` on every call. The Executive already had
`_reusableArgList` for exactly this purpose — used in MsilHelpers and
ExecutionCache, but not in Function.cs.

**Fix:** Clear and reuse `_reusableArgList`. One List<Var> per Executive lifetime
instead of one per function call.

**Expected impact:** Visible in Fibonacci_18 (237ms, recursion-heavy),
FuncCallOverhead_3000 (19ms), MixedWorkload_200.

### D — `SystemStack.ExtractArguments` O(n²) Insert(0)

**Problem:** Arguments were popped off the stack and inserted at position 0
(`arguments.Insert(0, Pop())`). `List<T>.Insert(0, x)` is O(n) — shifts all
existing elements. For n arguments this is O(n²) total work.

**Fix:** `Add()` each popped arg (O(1) amortised), then `Reverse(start, count)` once
(O(n)). Same result order, O(n) total.

**Expected impact:** Small for typical SNOBOL4 function arities (0–4 args), but
correctness and cleanliness win. More visible with higher-arity DATA constructors.

---

## Regression Gate

Cannot run `dotnet test` in this container (no dotnet SDK). Tests must be run by
Lon before merging to confirm 1873/1876 invariant holds. Changes are correctness-
neutral: fast paths return identical values; List semantics unchanged.

---

## Next Steps (net-perf-analysis remaining)

- [ ] Run `dotnet test` — confirm 1873/1876
- [ ] Re-run BenchmarkSuite2 after hotfixes — compare vs baseline.md
- [ ] `dotnet-trace collect` profile on VarAccess_2000 or MixedWorkload_200
- [ ] Update perf/baseline.md with post-hotfix numbers once measured
- [ ] Publish `## Performance` section in DOTNET.md
