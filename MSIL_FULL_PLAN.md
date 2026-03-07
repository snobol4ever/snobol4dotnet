# MSIL Full Absorption Plan
## Goal: replace ThreadedExecuteLoop with pure MSIL delegates

Current state: expression bodies are JIT-compiled (`CallMsil`).
Control flow (`Init`, `Finalize`, `Jump*`, goto machinery) still runs through
the threaded switch.

End state: every statement is a single `Func<Executive, int>` delegate that
executes the full statement — body + boundary + goto — and returns the next
instruction pointer.  The execute loop becomes a trivial `while` over an array
of delegates.

All steps are test-driven: existing 1432 tests must stay green after every commit.
New tests are added before or alongside each step.

---

## Step 6 — Inline Init/Finalize into delegates

**What moves:** `OpCode.Init` and `OpCode.Finalize` cases leave the loop and
become two new `Executive` helper methods called at the top and bottom of each
delegate.

**New helpers (MsilHelpers.cs):**
```
internal bool InitStatement(int stmtIdx)   // returns true if statement-limit abort
internal void FinalizeStatement()
```
`InitStatement` mirrors the `Init` case exactly:
- `AmpCurrentLineNumber = stmtIdx`
- `Failure = false`
- `AlphaStack.Clear(); BetaStack.Clear()`
- `SystemStack.Push(new StatementSeparator())`
- increment/check `AmpStatementCount` vs `AmpStatementLimit`
- returns `true` if the limit was hit (delegate should abort)

`FinalizeStatement` mirrors the `Finalize` case:
- pop stack to `StatementSeparator`
- `AmpLastLineNumber = AmpCurrentLineNumber`
- call `ProcessTrappedErrorThreaded()` if `ErrorJump > 0`

**Changes to BuilderEmitMsil.cs:**
`EmitAndCache` wraps the expression IL with:
```
ldarg.0
ldc.i4 stmtIdx
call InitStatement   → brfalse.s body_label; ldc.i4 <halt_ip>; ret
body_label:
... existing expression IL ...
ldarg.0
call FinalizeStatement
ret
```
But the delegate still returns `void` for now — this step only inlines the
boundary calls, it does not yet change the delegate signature.

**Changes to ThreadedCodeCompiler.cs:**
When emitting a `CallMsil`, skip emitting the surrounding `Init`/`Finalize` —
the delegate handles them.  The thread sequence becomes:
```
CallMsil(k)       ← full Init+body+Finalize
...goto opcodes...
```

**Changes to ThreadedExecuteLoop.cs:**
Remove `case OpCode.Init` and `case OpCode.Finalize` from the switch.
They are dead for any statement that has a delegate; keep as no-ops in the
default case for safety (or throw if unexpectedly reached).

**New tests (MsilEmitterTests.cs):**
- `Init_StatementLimitAbort` — set `AmpStatementLimit = 2`, verify abort fires
- `Finalize_ErrorJumpHandled` — trigger error and verify `ErrorJump` fires
- regression: full Roman suite still passes

**Commit:** `"Step 6: Inline Init/Finalize into MSIL delegates"`

---

## Step 7 — Change delegate signature to Func<Executive, int>

**What changes:** delegates now return the next instruction pointer (int).
`-1` = halt.  Falling through to the next instruction is expressed as
returning `InstructionPointer` (which the execute loop already advanced past
the `CallMsil` instruction).

**Changes to BuilderEmitMsil.cs:**
- `DynamicMethod` return type changes from `typeof(void)` to `typeof(int)`
- All `ret` statements become `ldc.i4 <fallthrough_ip>; ret` or
  `ldc.i4 -1; ret` (halt)
- `MsilDelegates` type changes to `List<Func<Executive, int>>`
- The fallthrough IP is not known at delegate-compile time — pass it in as a
  parameter OR have the delegate return a sentinel (e.g. `0`) meaning
  "advance normally", and the loop handles it

  **Chosen approach:** return `int` where:
  - `>= 0` = jump to this instruction pointer
  - `-1`   = halt
  - `int.MinValue` = "fall through" (loop advances IP normally, as today)

  This means the common case (no goto) costs one compare and branch — cheap.

**Changes to ThreadedExecuteLoop.cs:**
```csharp
case OpCode.CallMsil:
{
    var next = Parent.MsilDelegates[instr.IntOperand](this);
    if      (next == int.MinValue) { /* fall through — IP already advanced */ }
    else if (next == -1)           { exitCode = -1; goto Done; }
    else                           { InstructionPointer = next; }
    break;
}
```

**Changes to MsilHelpers.cs:**
`InitStatement` signature: returns `int` (the halt IP if limit hit, or
`int.MinValue` to continue).  Saves callers needing a branch.

**New tests:**
- `Delegate_ReturnsFallthrough` — compile a simple stmt, verify delegate
  returns `int.MinValue` (not a jump)
- `Delegate_ReturnsMinus1OnHalt` — verify a program that hits `Halt` returns -1
- All existing tests still green

**Commit:** `"Step 7: Change delegate signature to Func<Executive, int>"`

---

## Step 8 — Absorb fall-through gotos (no goto at all)

**What this covers:** statements with no goto clause, which currently emit:
```
CallMsil(k)
Jump(next_stmt)
```

After this step, the delegate itself returns the next IP for the
fall-through case, and `ThreadedCodeCompiler` emits nothing after `CallMsil`
for fall-through statements — just moves to the next statement's `CallMsil`.

**Changes to ThreadedCodeCompiler.cs:**
In `EmitGotos`, when `ParseUnconditionalGoto`, `ParseSuccessGoto`, and
`ParseFailureGoto` are all empty, emit nothing — the delegate already returned
`int.MinValue` which advances normally.

**Changes to BuilderEmitMsil.cs:**
The delegate already returns `int.MinValue` from Step 7.  No change needed
unless we want the delegate to return the exact next-statement IP — but
`int.MinValue` (fall through) is sufficient and simpler.

**New tests:**
- `NoGoto_ThreadContainsNoJump` — verify compiled thread for `N = 1` contains
  no `Jump` instruction following the `CallMsil`
- `NoGoto_CorrectExecution` — multi-statement program with no gotos runs cleanly

**Commit:** `"Step 8: Absorb fall-through gotos into delegates"`

---

## Step 9 — Absorb direct unconditional gotos `:(LABEL)`

**What this covers:** `line.ParseUnconditionalGoto.Count > 0` AND
`line.DirectGotoFirst == true` (label is a compile-time constant, no
expression evaluation needed at runtime).

Currently emits:
```
SaveFailure
CallMsil(goto_expr)
CheckGotoFailure
RestoreFailure
GotoIndirect(23)
```

After this step the delegate handles it entirely and returns the target IP.

**New helper (MsilHelpers.cs):**
```
internal int ResolveLabel(string label)
```
Returns the instruction pointer for `label` by looking up `LabelTable` then
`StatementInstructionStarts`, matching `GotoIndirect` exactly.  Returns `-1`
for end-of-program, `-2...-7` for `RETURN`/`FRETURN`/etc., and throws (or
returns an error code) for unknown labels.

**Changes to BuilderEmitMsil.cs:**
New path in `EmitAndCache` for statements with a direct unconditional goto:
```
... Init IL ...
... body expression IL ...
... Finalize IL ...
ldarg.0
ldstr "LABEL"        ← label name as a string literal baked into the IL
call ResolveLabel
ret                  ← returns the target IP directly
```

**Changes to ThreadedCodeCompiler.cs:**
When `DirectGotoFirst == true` and the goto expression fits this path,
skip emitting the `SaveFailure / CallMsil / CheckGotoFailure / RestoreFailure /
GotoIndirect` sequence — the delegate handles it.

**New tests:**
- `DirectGoto_UnconditionalJumps` — `:(LOOP)` loops correctly
- `DirectGoto_ReturnFromFunction` — `:(RETURN)` exits correctly
- Roman suite still green (uses `:(ROMAN_END)`)

**Commit:** `"Step 9: Absorb direct unconditional gotos into delegates"`

---

## Step 10 — Absorb direct conditional gotos `:S(LABEL)` / `:F(LABEL)`

**What this covers:** `DirectGotoFirst == true` for the conditional goto sides.
The four sub-cases from `EmitGotos`:
- success-only  → `JumpOnFailure(next) + CallMsil + CheckGotoFailure + GotoIndirect`
- failure-only  → `JumpOnSuccess(next) + ClearFailure + CallMsil + CheckGotoFailure + SetFailure + GotoIndirect`
- both (success first)
- both (failure first)

**Delegate IL pattern for success-only:**
```
ldarg.0; ldfld Failure
brtrue.s fall_through      // if failure, skip; return next-stmt IP
ldarg.0
ldstr "LABEL"
call ResolveLabel
ret
fall_through:
ldc.i4 <next_stmt_IP_sentinel>
ret
```

**Implementation note:** the "next statement IP" isn't known at delegate-build
time.  Use `int.MinValue` (fall through) — the loop advances normally.
For the failure case: `ClearFailure` + resolve label + `SetFailure` if the
label resolution fails (matching current semantics exactly).

**New helper (MsilHelpers.cs):**
```
internal int ResolveGotoOrFail(string label, bool setFailureOnBadGoto)
```
Handles `CheckGotoFailure` semantics inline: if `Failure` is set when entering,
log error 20 and return fallthrough.

**Changes to ThreadedCodeCompiler.cs:**
Conditional goto sequences that are `DirectGotoFirst/Second == true` no longer
emit `JumpOnSuccess/Failure`, `ClearFailure`, `SetFailure`, `CheckGotoFailure`,
or `GotoIndirect` — all absorbed into the delegate.

**New tests:**
- `ConditionalGoto_SuccessOnly` — `:s(LOOP)` counter loop
- `ConditionalGoto_FailureOnly` — `:f(END)` early exit
- `ConditionalGoto_Both` — `:s(A)f(B)` branching
- Full Roman suite (uses `:F(RETURN)` / `:S(RETURN)F(FRETURN)`)

**Commit:** `"Step 10: Absorb direct conditional gotos into delegates"`

---

## Step 11 — Absorb indirect gotos (computed labels)

**What this covers:** `DirectGotoFirst == false` — the goto label is a runtime
expression (e.g. `:(LABEL_VAR)` where `LABEL_VAR` is a variable holding the
label name).  This is already handled by `GotoIndirect`/`GotoIndirectCode`.

**Changes:** no new helpers needed — `ResolveLabel` already handles this by
taking a string.  The delegate evaluates the goto expression (already a
`CallMsil` for the goto token list), peeks the top of the system stack for the
label string, and calls `ResolveLabel`.  The `GotoIndirectCode` variant
(for `CODE()`-defined labels) needs a separate `ResolveCodeLabel(string)` helper
that mirrors the `GotoIndirectCode` case.

**Changes to ThreadedCodeCompiler.cs:**
All remaining `GotoIndirect` / `GotoIndirectCode` sequences are absorbed.
`SaveFailure` / `RestoreFailure` for unconditional indirect gotos are also
absorbed into the delegate.

After this step: `GotoIndirect`, `GotoIndirectCode`, `SaveFailure`,
`RestoreFailure`, `SetFailure`, `ClearFailure`, `CheckGotoFailure`,
`JumpOnSuccess`, `JumpOnFailure`, `Jump` are all dead for fully-compiled
statements.

**New tests:**
- `IndirectGoto_Variable` — goto via variable label
- `IndirectGoto_CODE` — label set by `CODE()` function

**Commit:** `"Step 11: Absorb indirect gotos into delegates"`

---

## Step 12 — Collapse the execute loop

**What remains in the loop after Steps 6–11:**
- `CallMsil` — dispatch delegates (all statements now)
- `Halt` — end of program
- All the old expression opcodes (`PushVar`, `OpAdd`, etc.) — dead for compiled
  programs, kept as fallback for the Roslyn path and any edge cases

**New execute loop:**
```csharp
while (true)
{
    var instr = thread[InstructionPointer++];
    switch (instr.Op)
    {
        case OpCode.CallMsil:
            var next = Parent.MsilDelegates[instr.IntOperand](this);
            if      (next == int.MinValue) break;
            else if (next < 0)            { exitCode = next; goto Done; }
            else                          { InstructionPointer = next; }
            break;
        case OpCode.Halt:
            exitCode = -1;
            goto Done;
        default:
            // Legacy Roslyn path or un-compiled statement — full switch below
            LegacyDispatch(instr);
            break;
    }
}
```

The old 200-line switch moves to a private `LegacyDispatch(Instruction)` method
so the hot path has minimal branches.

**New tests:**
- `ExecuteLoop_OnlyCallMsilAndHalt` — verify the thread for a complete program
  contains ONLY `CallMsil` and `Halt` instructions
- `ExecuteLoop_LegacyFallback` — verify the Roslyn path still works (if
  `UseThreadedExecution = false`)
- Entire 1432-test suite green

**Commit:** `"Step 12: Collapse execute loop — hot path is CallMsil + Halt only"`

---

## Step 13 — Eliminate the thread array entirely (stretch goal)

**What this covers:** instead of `Instruction[]` + delegate index, store
delegates directly in a `Func<Executive, int>[]`.  Each "statement" is one
entry.  The execute loop becomes:

```csharp
var stmts = StatementDelegates;
int ip = entryStatementIdx;
while (ip >= 0 && ip < stmts.Length)
{
    ip = stmts[ip](this);
    if (ip == int.MinValue) ip++;  // fall through: next statement
}
```

This eliminates the `Instruction[]` array, the `CallMsil` opcode, and the
`InstructionPointer` machinery for the fully-compiled path.  Gotos resolve
directly to statement indices.  `RETURN`/`FRETURN` return their sentinel values.

**Prerequisite:** all goto resolution must already return statement indices
(not instruction pointer indices), which requires a small adjustment to
`ResolveLabel` in Step 9.

**This step is optional** — Steps 6–12 already give essentially-pure MSIL
execution.  Step 13 is the clean-room rewrite that removes the last vestige ofthe threaded infrastructure from the hot path.

**New tests:**
- `PureDelegate_ThreadArrayGone` — verify `Execute.Thread` is null or empty
  for a fully-compiled program
- `PureDelegate_EntryLabel` — verify programs with a non-zero entry label
  (`-e LABEL` flag) still start at the right statement
- Full 1432-test suite green

**Commit:** `"Step 13: Eliminate Instruction[] — pure delegate dispatch"`

---

## Summary table

| Step | What moves into MSIL                        | Opcodes retired                                      | Complexity |
|------|---------------------------------------------|------------------------------------------------------|------------|
| 6    | Init + Finalize                             | `Init`, `Finalize`                                   | Low        |
| 7    | Delegate returns next-IP                    | (signature change only)                              | Medium     |
| 8    | Fall-through gotos                          | `Jump` (fall-through case)                           | Low        |
| 9    | Direct unconditional gotos `:(LABEL)`       | `SaveFailure`, `RestoreFailure`, `GotoIndirect` (direct) | Medium |
| 10   | Direct conditional gotos `:S/:F`            | `JumpOnSuccess`, `JumpOnFailure`, `ClearFailure`, `SetFailure`, `CheckGotoFailure`, `GotoIndirect` (direct) | Medium |
| 11   | Indirect/computed gotos                     | `GotoIndirect`, `GotoIndirectCode` (all remaining)   | Hard       |
| 12   | Collapse execute loop                       | Loop reduces to `CallMsil` + `Halt` only             | Low        |
| 13   | Eliminate `Instruction[]` entirely          | `CallMsil`, `InstructionPointer`, thread array       | Medium     |

---

## Invariants to maintain throughout

- **1432 tests green after every commit** — no exceptions
- **Roslyn path unaffected** — `UseThreadedExecution = false` must keep working;
  the old threaded switch stays as a `LegacyDispatch` fallback
- **`BuildEval` / `BuildCode` paths** — both compile new statements at runtime;
  each must call `EmitMsilForAllStatements()` before compiling so new delegates
  are registered before the next execute cycle
- **`AppendCompile`** — when `BuildCode` appends to a live thread, existing
  delegate indices must not shift; new delegates are always appended
- **Recursive `ThreadedExecuteLoop`** — user-defined functions call the loop
  recursively; `savedIP` / `savedFailure` / `savedErrorJump` save-restore
  discipline must be preserved in whatever replaces the loop
- **`LastExpressionFailure`** — set just before `Done:` in the current loop;
  `RunExpressionThread` reads it; must survive the refactor

---

## File map — what changes in each step

```
Step 6:   MsilHelpers.cs          + InitStatement(), FinalizeStatement()
          BuilderEmitMsil.cs      wrap delegates with Init/Finalize IL
          ThreadedCodeCompiler.cs skip Init/Finalize emission around CallMsil
          ThreadedExecuteLoop.cs  remove Init/Finalize cases (or no-op)

Step 7:   BuilderEmitMsil.cs      DynamicMethod returnType → int, ret → ldc+ret
          MsilHelpers.cs          MsilDelegates type → List<Func<Executive,int>>
          ThreadedExecuteLoop.cs  CallMsil case reads return value

Step 8:   ThreadedCodeCompiler.cs omit Jump after fall-through CallMsil
          BuilderEmitMsil.cs      fall-through ret returns int.MinValue

Step 9:   MsilHelpers.cs          + ResolveLabel(string) → int
          BuilderEmitMsil.cs      direct unconditional goto → ldstr + call ResolveLabel + ret
          ThreadedCodeCompiler.cs skip SaveFailure/CallMsil/Check/Restore/GotoIndirect

Step 10:  MsilHelpers.cs          + ResolveGotoOrFail(string, bool) → int
          BuilderEmitMsil.cs      conditional goto IL patterns
          ThreadedCodeCompiler.cs skip JumpOn*/ClearFailure/SetFailure/Check/GotoIndirect

Step 11:  MsilHelpers.cs          + ResolveCodeLabel(string) → int
          BuilderEmitMsil.cs      indirect goto: eval expression, peek stack, call ResolveLabel
          ThreadedCodeCompiler.cs no more GotoIndirect/GotoIndirectCode emitted

Step 12:  ThreadedExecuteLoop.cs  hot-path loop + LegacyDispatch() fallback
          Instruction.cs          mark retired opcodes as obsolete (don't delete yet)

Step 13:  Builder.cs              StatementDelegates: Func<Executive,int>[]
          Executive.cs            replace Thread + InstructionPointer with StatementDelegates + IP
          ThreadedExecuteLoop.cs  rewrite as delegate-array loop
          Instruction.cs          CallMsil opcode removed
```
