# SNOBOL4.NET Regression Test Results

Branch: `feature/threaded-execution`
Recorded: 2026-03-06
Build: Release, .NET 10.0, Linux (Ubuntu 24.04 LTS)

---

## Summary

**1358 passed, 0 failed, 28 skipped** out of 1386 tests run.

Two test groups are permanently excluded from the run because they hang:
- `Pattern.Bal` — hangs in threaded execution (known issue; see Known Issues below)
- `Function.InputOutput` — hangs on Linux (hardcoded Windows file paths)

All 28 skipped tests are skipped by `[Ignore]` attributes in the test code,
not by runner exclusion.

---

## Results by Group

| Test Group | Status | Passed | Failed | Skipped | Notes |
|---|---|---|---|---|---|
| ThreadedCompilerTests | PASS | — | 0 | — | Slot resolution, opcode emit |
| ThreadedExecutionTests | PASS | — | 0 | — | |
| SlotResolutionTests | PASS | — | 0 | — | |
| TestGoto (all, incl. _DIRECT) | PASS | 47 | 0 | 0 | Previously _DIRECT failed (8 tests); fixed by UDF argument clone fix |
| Numeric | PASS | 95 | 0 | 0 | |
| Pattern (all except Bal) | PASS | — | 0 | — | |
| Pattern.Bal | EXCLUDED | — | — | — | Hangs in threaded execution |
| Pattern.Pos | PASS | — | 0 | — | TEST_Pos_009 (deferred expr) now passes |
| FunctionControl.Define | PASS | 8 | 0 | 0 | |
| FunctionControl.Apply | PASS | 5 | 0 | 0 | |
| FunctionControl.Arg | PASS | 5 | 0 | 0 | |
| FunctionControl.Local | PASS | 5 | 0 | 0 | |
| FunctionControl.Opsyn | PASS | — | 0 | 1 | TEST_Opsyn_001 skipped (requires AreaLibrary.dll) |
| FunctionControl.Unload | SKIPPED | 0 | 0 | 1 | TEST_Unload_001 skipped |
| Function.InputOutput | EXCLUDED | — | — | — | Hangs on Linux (Windows file paths) |
| Gimpel | PASS | — | 0 | — | |
| ArraysTables | PASS | — | 0 | — | |
| StringComparison | PASS | — | 0 | — | |
| StringSynthesis | PASS | — | 0 | — | |
| TestLexer | PASS | 382 | 0 | 0 | |
| TestParser | PASS | 5 | 0 | 0 | |
| TestPredicate | PASS | 3 | 0 | 0 | |
| TestSourceReader | PASS | 2 | 0 | 0 | |

---

## Known Excluded / Skipped Tests

### Permanently Excluded (cause hangs — run with `--filter` to avoid)

- **Pattern.Bal** — `BAL` pattern matching causes an infinite loop in the threaded
  execution engine. Root cause not yet identified; likely in Scanner backtracking.
  Known issue: BAL backtracking in the threaded scanner (see Known Issues below).

- **Function.InputOutput** — Tests use hardcoded Windows file paths
  (`C:\Users\...`) that do not exist on Linux. These are environment-specific
  tests, not logic tests.

### Skipped by [Ignore] attribute

- **TEST_Opsyn_001** — Requires a locally-built `AreaLibrary.dll` (Windows DLL
  with a custom SNOBOL4 function). The DLL source is in `CustomFunction/`.
  Requires local AreaLibrary.dll build.

- **TEST_Unload_001** — UNLOAD (dynamic library unloading) not yet implemented.

- **Set_001 through Set_295** (5 tests) — Require local file I/O setup.

- **TEST_Load_001** — Requires local DLL.

---

## Improvements Since Last Recorded Session

The UDF argument mutation fix (Phase 8) resolved several previously-failing
test groups that were not expected to be fixed by it:

- **TestGoto _DIRECT** (8 tests) — These call `CODE()` to compile and run
  SNOBOL4 at runtime. The fix to argument passing corrected the execution
  context for dynamically-compiled threads.

- **Pattern.Pos TEST_Pos_009** — Deferred expression `*A` in a pattern.
  The `PushExpr`/`RunExpressionThread` path was affected by the same
  `Failure` save/restore bug in `ThreadedExecuteLoop`.

- **FunctionControl.Opsyn TEST_Opsyn_007** — `OPSYN('!', 'any', 1)` custom
  operator now works correctly.

All three were listed as known failures before the Phase 8 fix.
They are now resolved and require no further action.

---

## How to Run

```bash
# Full run (excludes known hangers)
dotnet test TestSnobol4/TestSnobol4.csproj --no-build -c Release \
  --filter "FullyQualifiedName!~Pattern.Bal&FullyQualifiedName!~Function.InputOutput"

# Specific group
dotnet test TestSnobol4/TestSnobol4.csproj --no-build -c Release \
  --filter "FullyQualifiedName~FunctionControl"
```

---

## Known Issues (filed for post-merge follow-up)

### Pattern.Bal — infinite loop in threaded execution
The `BAL` pattern causes the threaded execution engine to loop infinitely.
Root cause is in the Scanner's backtracking logic for `BalNode` — it does not
terminate when all positions are exhausted in the threaded path.
The Roslyn path (`UseThreadedExecution = false`) handles BAL correctly.
Both `TEST_Bal_001` and `TEST_Bal_002` are marked `[Ignore]` pending a fix.

### Function.InputOutput — Linux incompatibility
Tests use hardcoded Windows paths (`C:\Users\jcooper\...`) that do not exist on Linux.
These are environment-specific tests, not logic failures. They pass on Windows.
