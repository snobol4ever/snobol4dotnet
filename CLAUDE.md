# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

SNOBOL4.NET compiles and runs SNOBOL4 with the SPITBOL extensions in C# on .NET 10. It follows SPITBOL: *"any statement about SPITBOL in the SPITBOL manual applies to SNOBOL4.NET"* (README). When behaviour is in question, SPITBOL is the reference, not CSNOBOL4 or the original SNOBOL4. Lon, 2026-10-18: *"The snobol4dotnet is meant to mimic SPITBOL exactly."* Where the manual is silent, Dewar and Emmer's source decides.

- **Ask the oracle, don't guess.** Run the same program through `/home/resources/x64/bin/sbl` and through `Snobol4.dll`, with the same switches (`-b`, `-f`/`-F` mean the same in both), and diff the output. A test whose expectation is not SPITBOL's answer is corrected only after `sbl` has run that exact program.
- **One defect per commit**, red test first. Subjects read `dotnet-<slug>: <what now happens>, as in SPITBOL` (see `git log`).

## Commands

```bash
dotnet build Snobol4.sln                                   # whole solution, Debug
dotnet build Snobol4/Snobol4.csproj -c Release -p:ReadyToRunBuild=false   # just the console compiler, no ReadyToRun pass
bash scripts/test_in_batches.sh                            # all unit tests (MSTest), in batches of 100
bash scripts/rungs_entries_equal_ref.sh <entry>...         # named entries of corpus/tests/snobol4/ALL.sno, each run alone under -bf against sbl's ref
python3 scripts/sweep/ctxrun.py 5 6                        # differential sweeps against sbl (run in an empty scratch dir; see scripts/sweep/README.md)

# one class, or one method (method names repeat across classes: TEST_Abort_001 is in both Abort and Fence)
dotnet test TestSnobol4/TestSnobol4.csproj --filter "FullyQualifiedName~Test.Pattern.Abort"
dotnet test TestSnobol4/TestSnobol4.csproj --filter "FullyQualifiedName=Test.Pattern.Abort.TEST_Abort_001"

# run a program (-b suppresses the sign-on banner)
dotnet Snobol4/bin/Release/net10.0/Snobol4.dll -b prog.sno < prog.in
dotnet Snobol4/bin/Release/net10.0/Snobol4.dll -w prog.sno   # also saves prog.dll
dotnet Snobol4/bin/Release/net10.0/Snobol4.dll prog.dll      # runs a saved program

bash build_native.sh                                       # rebuild the committed native .so fixtures (gcc)

# wall-clock benchmarks; finds corpus/benchmarks/snobol4/ above the runner, or the folder CORPUS names
dotnet run --project benchmarks/Benchmarks.csproj -c Release
```

- **The suite is green.** At `c9e85c9` it ran 3,343 tests: 3,331 passed, 0 failed and 12 skipped (README, *Test Results*). A failure is therefore new. Run the affected batch before you change anything, so you know whether your change caused it. The README's table of the eleven program suites graded against `sbl -bf` is the other bar.
- **Run the suite in batches of 100, never in one `dotnet test` of the whole suite.** Lon, 2026-10-18: *"You better be running tests in batches of 100."* A whole-suite run intermittently hangs or crashes its test host and loses every result. `scripts/test_in_batches.sh` runs each batch in its own bounded host and names any batch that did not complete.
- **Builds are light by default.** `Directory.Build.rsp` gives every `dotnet build` and `dotnet test` `-maxcpucount:1 -nodeReuse:false -p:UseSharedCompilation=false`, because the machine is shared. A `-m` on the command line overrides it. A Release build of `Snobol4` also runs a ReadyToRun `dotnet publish` after the build and copies the precompiled `Snobol4.dll` and `Snobol4.Common.dll` over the plain ones (the `ReadyToRunBuildOutput` target in `Snobol4/Snobol4.csproj`). That pass is a second compile, and `-p:ReadyToRunBuild=false` skips it while you iterate.
- `Snobol4W` targets `net10.0-windows` and sets `EnableWindowsTargeting` in its own csproj, so the solution builds on Linux without extra flags.
- There is no separate linter. `.editorconfig` holds the style rules: file-scoped namespaces, `_camelCase` private fields, `using` outside the namespace.
- The bridge smokes in `scripts/test_smoke_dot_bridge*.sh` run the Release `Snobol4.dll` against the `beauty` demo. Their defaults (`SNO4_REPO=/home/claude/snobol4dotnet`, `CORPUS=/home/claude/corpus`, `BEAUTY=$CORPUS/programs/snobol4/beauty`) usually need overriding. In the current corpus layout, beauty lives at `corpus/demos/snobol4/beauty`.

## Architecture

Nearly everything lives in **`Snobol4.Common`**. `Snobol4/MainConsole.cs` (the CLI) and `Snobol4W` (WinForms) are thin shells around it. `Snobol4.Common` declares `InternalsVisibleTo("TestSnobol4")`, so tests reach `internal` members directly.

### Compile pipeline (`Builder/`, driven by `Builder.BuildMain`)

1. **Lex**: `Lexer.cs` + `LexTable.cs` run a DFA over source lines into `Token`s, handling labels, gotos, implicit concatenation and continuation lines.
2. **Parse**: `Parser.cs` uses shunting-yard to turn each statement's expressions into postfix token lists (`Builder.ParseExpression`).
3. **Resolve**: `BuilderResolve.cs` maps variables, functions and constants to slot indices (`VariableSlots`, `FunctionSlots`, `ConstantPool`).
4. **Emit MSIL**: `BuilderEmitMsil.cs` JIT-compiles each statement's expression body into a `DynamicMethod` (`Func<Executive,int>`) cached in `MsilDelegates`.
5. **Thread**: `ThreadedCodeCompiler.cs` builds a flat `Instruction[]` (`OpCode` in `Instruction.cs`) in two passes: emit, then patch jump fixups. Statement bodies become `CallMsil`. Control flow (`Init`, `Jump*`, `GotoIndirect`, `Halt`) stays as opcodes.
6. **Run**: `Runtime/Execution/ThreadedExecuteLoop.cs` dispatches the thread. A fast path spins when the thread holds only `CallMsil`/`Halt` (`ThreadIsMsilOnly`). Exit codes: -1 end, -2 RETURN, -3 FRETURN, -4 NRETURN. The loop re-enters recursively for user-defined function calls.

Variants of the same pipeline:
- `BuildEval` compiles for `EVAL`.
- `BuildCode` compiles for `CODE` and appends to the live thread (`AppendCompile`).
- Deferred `*expr` compiles to "star functions" (`CompileStarFunctions`, `DeferredExpression.cs`).
- `-w` writes a DLL (`BuilderSaveDll.cs`), and `RunDll` reloads one.

A statement in error does not stop the compile: it is listed on the page `sbl` prints (`BuilderErrors.WriteStatementInError`), marked `SourceLine.CompileError`, compiles to nothing, and the program does not run unless `-k` or `-ERRORS` asks. Two things raise such errors, both per statement in `Lexer.LexLines`, so they list in source order: the lexer itself, and `StatementChecker`, which does what SPITBOL's code generation does. It evaluates an arithmetic operator or an `svpre` function (`LEN`, `TAB`, `CHAR`, `REMDR`, …) whose operands are all constants, by calling the real builtin with `Executive.PreEvaluating` set, so `1 / 0` or `LEN(-1)` is a compile error with the run-time error's number; and it finds a value where a name is required (error 212, as in `'A' = 2` or `X . 1`). The run's exit status is `Builder.ExitStatus` (231 when execution is suppressed, 1 when the source cannot be read).

The old Roslyn C#-codegen engine is gone, even though `PR_DESCRIPTION.md`, some comments and the `-cs` option still mention it.

### Runtime (`Runtime/`)

- **`Executive`** (a `partial class` spread across many files) is the interpreter state: symbol tables (`IdentifierTable`, `LabelTable`, `FunctionTable`, `UserFunctionTable`), the `SystemStack`, the `Failure` flag, I/O streams and trace tables.
- **Builtins:** the `Executive` constructor (`Executive.cs`) registers every builtin as a `FunctionTableEntry(name, handler, argCount, …)`. Binary operators are keyed `__+`, `__$`, …; unary operators are keyed `_-`, `_&`, … (the OPSYN namespace). Each handler is an `Executive` method in `Runtime/Functions/<Category>/<Name>.cs` that takes `List<Var>`. Errors are reported with `LogRuntimeException(n)` using SPITBOL error numbers, and each such file lists the numbers it uses in a comment at the top. The table holds every builtin under both a lower-case key and an upper-case key, so a new builtin needs its handler plus both entries. Operator files are named after their symbol, as in `OperatorsBinary/PatternMatch (Question Mark).cs` and `ImmediateAssignment ($).cs`, and `StringVar/StringVar .cs` has a space before the dot, so always quote paths.
- **Values** (`Runtime/Variable/`): `Var` is the abstract base. There is one folder per datatype (`StringVar`, `IntegerVar`, `RealVar`, `PatternVar`, `ArrayVar`, `TableVar`, `CodeVar`, `ExpressionVar`, `NameVar`, `ProgramDefinedDataVar`, `ExternalVar`, …). Each type delegates to five strategy classes: Arithmetic, Comparison, Conversion, Cloning, Formatting. To change how a type converts or prints, edit its strategy file, not `Var.cs`.
- **Patterns:** `Runtime/Pattern/*Pattern.cs` are the pattern objects, built by `PatternFactories.cs`. At match time, `PatternMatching/AbstractSyntaxTree.Build` flattens a pattern into a node list, and `Scanner` walks it with alternate/backtrack stacks. `Executive.PatternMatch` (the `?` operator) saves and isolates the Alpha/Beta stacks used for `.` and `$` assignment on every call. Two caches in this layer are easy to break:
  - `*X` (`UnevaluatedPattern`) grafts sub-ASTs, cached per (caller node, pattern).
  - `Scanner` memoizes failures of side-effect-free terminals at a given cursor. A terminal with side effects must not be added to that memo list.
- **External functions:** `LOAD` lives in `Runtime/Functions/FunctionControl/Load.cs`. It loads .NET assemblies through `ExternalLibrary/IExternalLibrary`, `IStatefulExternalLibrary`, or a reflection-based auto-prototype for plain C#/F#/VB classes. It also loads native C libraries over the SPITBOL XNBLK protocol.
- **Monitor bridge:** `Runtime/Monitor/MonitorIpc.cs` streams VALUE/CALL/RETURN/LABEL and pattern Byrd-box events to an external sync-step monitor. It stays dormant unless `MONITOR_READY_PIPE` and `MONITOR_GO_PIPE` are set, and the bridge smokes check that it really is dormant.
- **The `-s` stack:** `Runtime/Execution/StatementControl.cs` runs the program on a thread whose .NET stack is sized from `-s`. `Define.cs` and `UnevaluatedPattern.cs` charge each call level the bytes SPITBOL's stack would take and raise error 246 past `-s`, so recursion depth matches `sbl` rather than the .NET stack.
- **Snocone:** `Builder/SnoconeLexer.cs` and `SnoconeParser.cs` exist and are unit-tested (`TestSnobol4/TestSnocone`), but nothing in the compile pipeline or command line calls them yet.

### Tests (`TestSnobol4/`, MSTest)

- **The harness:** almost every test calls `SetupTests.SetupScript(directives, script)` from `TestLexer/SetupTests.cs`. `directives` holds command-line switches such as `"-b"`, and `script` is inline SNOBOL4 source. The call compiles and runs in-process and returns the `Builder`.
- **Assertions:**
  - variables: `((StringVar)build.Execute!.IdentifierTable[build.FoldCase("x")]).Data`
  - SPITBOL error numbers: `build.ErrorCodeHistory`
  - printed output: `SetupTests.RunWithInput(script, stdin)`
- **Parallelism:** the assembly runs tests in parallel at method level (`[assembly: Parallelize]` in `Function/ArraysTables/Array.cs`). Any test that touches process-wide state, such as `Executive.ReadLineDelegate` or `Console.SetError` through `RunWithInput`, must sit in a `[DoNotParallelize]` class, as the `Corpus/CorpusRef_*` classes do.
- **`Corpus/CorpusRef_*`:** these embed corpus programs inline with their expected output. They do not read the corpus checkout.
- **Fixtures:** `CustomFunction/*` are LOAD fixtures that `TestSnobol4.csproj` builds with `ReferenceOutputAssembly="false"`. `SetupTests.*LibraryPath` finds them by reading Debug or Release from the test's own bin path, so build fixtures and tests in the same configuration. The native C fixtures are prebuilt `.so` files, committed under `CustomFunction/Spitbol*Lib/`.

## Docs that no longer match the code

Don't take these as truth. Measure instead.

- `BUILDING.md` says xUnit, about 1,876 tests and a required `-p:EnableWindowsTargeting=true`. The suite is MSTest with 3,343 tests, and the flag isn't needed. Its `CORPUS=.../snobol4corpus/benchmarks` example names the old corpus layout; the runner now wants the folder holding the `.sno` files, `corpus/benchmarks/snobol4/`.
- `README.jeff.md` gives old counts (1,874 of 1,876 tests, a corpus of 106 programs) and old benchmark timings. `README.md`'s *Test Results* is current.
- `PR_DESCRIPTION.md` describes a Roslyn fallback switch that no longer exists.
- The `corpus` submodule (`.gitmodules` → `SNOBOL4-plus/SNOBOL4-corpus`) is a gitlink to an empty folder, and the `Makefile` includes a `../SNOBOL4-corpus/run/sno.mk` that doesn't exist. The corpus is the sibling checkout `../corpus` (`snobol4ever/corpus`).
- Six `.dll` files are tracked in git: the four `CustomFunction/Spitbol*Lib/libspitbol_*.dll` fixtures, `Snobol4/Test Files/Init.dll` and `TestSnobol4/test2.dll`. `.gitignore` covers `bin/` and `obj/`, so don't add to them.
