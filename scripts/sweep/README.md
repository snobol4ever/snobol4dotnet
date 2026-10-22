# Differential sweeps against the oracle

snobol4dotnet mimics SPITBOL, and the oracle is `sbl` (`/home/resources/x64/bin/sbl`). These scripts put many values through
one function, operator or statement shape under both `sbl -bf` and the dotnet console and print where the two differ. They
found the unhandled exceptions on `ARRAY('')`, `S<1>` of a string, `ITEM('x', 1)` and `TRACE` in a statement, and the
aggregate swap (`L = V` then `V = TABLE()` assigned `L`). A difference is a defect of this project unless the oracle itself
crashes (`ARRAY(9223372036854775807)` dumps core).

Run them from an **empty scratch directory** (they write `*.sno`, `*.sbl`, `*.net`), after
`dotnet build Snobol4/Snobol4.csproj -c Release`. `SBL` and `DLL` override the two engines.

| script | what it does |
|---|---|
| `funcsweep.sh FUNC N` | `FUNC` called with every N-tuple of a pool of 21 values (null, blanks, numbers, reals, a name, signs, exponents), through variables so nothing is folded at compile time; prints the diff of the two runs (`N=40` shows more lines) |
| `funcgen.py FUNC N [SEED]` | the program `funcsweep.sh` runs (for 3 arguments a seeded sample of 350 of the tuples) |
| `opgen.py OP NAME` | the same for an infix operator (`OP` is the operator, `NAME` the file) over every pair of the pool; `set -f` first for `*` |
| `ctxgen.py` | the tables: `VALUES` (a value of every datatype: string, number, null, integer, real, DATA object, array, table, pattern, expression, name, code) and `CTX` (95 statements, each with the value as `V`: operands, function arguments, pattern positions, subscripts, keyword assignments); `python3 ctxgen.py` lists them |
| `ctxrun.py [N...]` | every context (or those numbered) against every value, one program each, on both engines; a run that dies is repeated value by value so a crash shows as `CRASH <Exception>` and a hang as `TIMEOUT` |
| `ctxtest.py CLASS NS OUTFILE SUMMARY 'NAME=CTX;;CTX@VAL,VAL'...` | writes a C# test class whose expected lines are what `sbl -b` printed for the named contexts and values; it refuses to overwrite a file |

A sweep gives the *cases*; a fix still needs a test that is red before and green after, with the oracle's line as its
expectation (`TestSnobol4/Function/.../ArgumentRules.cs`, `NonStringValues.cs`, `KeywordAssignment.cs` are examples). Where an old
test asserts a non-SPITBOL answer, run its exact program under `sbl -b` before correcting it.
