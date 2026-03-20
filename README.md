# <center>SNOBOL4.NET</center>

A compiler for the SNOBOL4 language written in C# and .NET and runs under Windows, Linux, and Mac OS.

A major goal of the project was to implement SNOBOL4 and the SPITBOL extensions using a high-level language that is readable and safe. Another goal was to match SPITBOL functionality as described in Emmer and Quillen's *MACRO SPITBOL: The High-Performance SNOBOL4 Language*, otherwise noted as "the SPITBOL manual".

As a rule, any statement about SPITBOL in the SPITBOL manual, applies to SNOBOL4.NET. Original SNOBOL4 and SNOBOL4+ features not supported by SPITBOL are not implemented in SNOBOL4.NET. Extensions that differ in their implementation from SPITBOL are outlined in this document. 

One major difference is that SNOBOL4.NET is slower than SPITBOL. This is the trade-off of using a high level language and safe programming practices. Hopefully, people will find the source code readable, and some will improve the code for readability and speed.

That said, performance is a continuous focus. A threaded-code JIT compiles hot statement paths to MSIL delegates at runtime, and the pattern engine uses the Byrd Box model — the same four-port (α/β/γ/ω) state machine used by native SPITBOL — so patterns carry zero interpreter dispatch overhead on the matched path. Current benchmarks, Release build, .NET 10, Linux x64:

| Benchmark | Mean | Alloc/run | Notes |
|-----------|-----:|----------:|-------|
| Roman_1776 | 25 ms | 434 KB | Recursive function, 4 levels deep |
| ArithLoop_1000 | 39 ms | 1,662 KB | Tight counter loop |
| Fibonacci_18 | 189 ms | 11,853 KB | ~21,000 recursive calls |
| FuncCallOverhead_3000 | 20 ms | 805 KB | Function call throughput |
| StringPattern_200 | 109 ms | 5,820 KB | CSV parsing via BREAK pattern |
| PatternBacktrack_500 | 69 ms | 1,971 KB | Backtracking pattern |
| MixedWorkload_200 | 204 ms | 13,931 KB | Realistic mixed workload |

## Chapter 1: Installation

### About This Manual
#### Scope

[Replace with:]

This manual covers the .NET implementation of SNOBOL4 and the SPITBOL extensions. SNOBOL4.NET runs on Windows, Linux, and Mac OS.

**Test coverage: 1,874 / 1,876 tests passing. Zero failures.** (2 skipped — require native `.so` on Linux only.) The full SNOBOL4/SPITBOL corpus of 106 programs passes across all 11 language rungs: output, assignment, arithmetic, control flow, patterns, capture, strings, keywords/predicates, functions/recursion, and aggregate types (ARRAY/TABLE/DATA).

### MS-DOS, Windows 95, Windows NT

[Deleted]

### Windows 11 or later

The Windows version consist of three files:

* Snobol4.exe
* Snobol4W.exe
* Snobol.Common.dll

These three files should be stored in the same folder and the folder placed anywhere accessible from the search path. Snobol4.exe is the command line version and Snobol4W is the Windows version. Snobol4.Common.dll is the code in common with both.

### Linux and macOS

On Linux and macOS, only the command-line runner is available. Build from source with the .NET 10 SDK:

```bash
dotnet build Snobol4.sln -c Release -p:EnableWindowsTargeting=true
```

The `snobol4` binary is in `Snobol4/bin/Release/net10.0/`. Add it to your PATH. Requires .NET 10 runtime.

### Installing SPITBOL

[Deleted]

### DOS-Extended SPITBOL-386

[Deleted]

### SPITBOL-8088

[Deleted]

### Experienced Users
If you want to run an existing SPITBOL program, consider the following items that may need to change:

* Command-line options

Check the command-line options. Some command line options are not implemented or work differently than SPITBOL.

```
snobol4 [options] files[.sno .sbl .spt]

  -a  = -c -l -x (all stats + listing)   -b  suppress sign-on message
  -c  compiler statistics                 -f  don't fold names to upper-case
  -F  fold to upper-case (default)        -h  suppress listing header
  -k  stop on runtime error               -l  show listing
  -n  suppress execution                  -o=file  listing/stats/dump file
  -r  INPUT reads lines after END         -u "string"  HOST(0) parameter
  -v  generate debug symbols              -w  write .dll save file
  -x  execution statistics                -?  display help

option defaults: -F
```

The `-w` switch saves the compiled program as a `.dll` assembly that can be reloaded and run without the original source — equivalent to SPITBOL's save file mechanism.

* INPUT() and OUTPUT() functions.

In SPITBOL the arguments are INPUT(.Variable, Channel, "filename[options]) and the same for OUTPUT(). In SNOBOL4.NET, a third object is the file mode option, a fourth argument is the file share option, and a fifth argument for OUTPUT controls whether an end of line is printed. If 0 (the default), an end of line is printed. If non-zero, an end of line is not printed. If options are not used, INPUT() and OUTPUT() function identically between SNOBOL4.NET and SPITBOL.

In SNOBOL4.NET, fixed length records are not supported due to conflicts with UNICODE support.

File Mode Options: (Default is 4)

1. CreateNew:    Create new file. Error if the file exists
2. Create:       Create file. If file exist, overwrite
3. Open:         Open existing file. Error if file does not exist
4. OpenOrCreate: If file exists, open, else create.
5. Truncate:     Open existing file and truncate to zero bytes.
6. Append:       Open existing file, and seeks the end.

File share options: (Default is 3)

0. No Sharing
1. Share Read
2. Share Writing
3. Share Read/Write
4. Allow subsequent deleting of a file.

End Of Line options: (Default is 0)

0. Output end of line character(s)
1. Do not output end of line character(s)

The end of line characters are operating system dependent
---

## Features Implemented

### Pattern Matching

The full SNOBOL4/SPITBOL pattern vocabulary:

| Pattern | Description |
|---------|-------------|
| `LIT` / string literal | Match exact string |
| `ANY(s)` | Match any single character in string s |
| `NOTANY(s)` | Match any single character not in string s |
| `SPAN(s)` | Match one or more characters from string s |
| `BREAK(s)` | Match zero or more characters up to a character in s |
| `BREAKX(s)` | Like BREAK but resumes after the delimiter |
| `ARB` | Match any string (zero or more characters) |
| `ARBNO(p)` | Match zero or more repetitions of pattern p |
| `BAL` | Match a balanced-parenthesis string |
| `LEN(n)` | Match exactly n characters |
| `POS(n)` | Succeed if cursor is at position n |
| `RPOS(n)` | Succeed if cursor is n from the right |
| `TAB(n)` | Match up to position n |
| `RTAB(n)` | Match up to n characters from the right |
| `REM` | Match the remainder of the subject |
| `FENCE` | Succeed once; prevent backtracking past this point |
| `ABORT` | Immediately fail the entire match |
| `SUCCEED` | Always succeed (forces backtracking loop) |
| `FAIL` | Always fail (forces backtracking) |

Patterns compose with concatenation (juxtaposition) and alternation (`|`). Conditional assignment (`.`) captures the matched substring into a variable on success. Immediate assignment (`$`) captures during the match, before success is confirmed. Named pattern references (`*VAR`) defer evaluation until match time, enabling recursive patterns.

### Functions and Data Types

- `DEFINE('proto(args)locals', 'entry')` — define user functions with proper argument and local scoping
- `RETURN` / `FRETURN` / `NRETURN` — normal return, failure return, null return
- `APPLY(fn, args...)` — call a function by name at runtime
- Recursive functions fully supported
- `ARRAY(dims, value)` — multi-dimensional arrays
- `TABLE(size)` — associative tables (hash maps)
- `DATA('type(fields)')` / `FIELD` — user-defined record types
- `EVAL(expr)` / `CODE(source)` — runtime compilation and execution

### External Functions

SNOBOL4.NET supports loading external functions from .NET assemblies and native C shared libraries:

```snobol
*  Load a C# function from a .NET assembly
        LOAD('MyFunc(STRING)STRING', 'MyLibrary.dll')
        RESULT = MyFunc('hello')

*  Load a native C function
        LOAD('cfunc(INTEGER)INTEGER', 'libmylib.so')
```

The full SPITBOL XNBLK protocol is implemented, including persistent state blocks (`XNBLK`), first-call flags (`xn1st`), and shutdown callbacks (`xncbp`). C#, F#, and VB.NET extensions are supported via reflection-based auto-prototype.

### Keywords

All SPITBOL keywords are implemented, including:

- `&STCOUNT` / `&STLIMIT` — statement count and execution limit
- `&DUMP` — variable dump on termination (levels 0–3)
- `&TRACE` / `&FTRACE` — execution and function tracing
- `&ERRLIMIT` / `&ERRTYPE` / `&ERRTEXT` — error handling
- `&ANCHOR` / `&FULLSCAN` / `&TRIM` — pattern matching control
- `&MAXLNGTH` — maximum string/object size
- `&ALPHABET` / `&UCASE` / `&LCASE` — character set keywords
- `&INPUT` / `&OUTPUT` — I/O control

### Tracing and Debugging

```snobol
*  Trace every call and return of function FOO
        TRACE('FOO', 'CALL')
        TRACE('FOO', 'RETURN')

*  Trace every change to variable X
        TRACE('X', 'VALUE')

*  Limit execution to 10000 statements
        &STLIMIT = 10000

*  Dump all variables on termination
        &DUMP = 1
```

`DUMP()` prints the current variable table to the listing file. `&FTRACE` counts down function call traces. `SETEXIT('label')` intercepts runtime errors for recovery.

---

## A Note on the Pattern Engine

SNOBOL4.NET compiles every pattern to a **Byrd Box** — a four-port state machine with entry (α), retry (β), success (γ), and failure (ω) ports. Boxes wire together into a graph: γ of one box connects to α of the next (concatenation); ω chains back for alternation; ARBNO wires α → γ in a loop. The result is a pure goto structure with no interpreter loop on the hot path.

This is the same execution model used by the original SPITBOL compiler and by snobol4x (the native compiler in the snobol4ever family). The four ports are not an implementation detail — they are the architecture. Every primitive pattern type, every compound pattern, every recursive named pattern reference reduces to this model.

The practical consequence: pattern matching in SNOBOL4.NET is not a regex engine and not an NFA simulator. It is a compiled state machine. It handles the full Chomsky hierarchy — regular languages, context-free languages, context-sensitive languages — from a single engine, without special cases.

---

## Running Programs

```bash
# Compile and run
snobol4 myprogram.sno

# Compile only, no execution
snobol4 -n myprogram.sno

# Compile and save as .dll for later execution
snobol4 -w myprogram.sno
snobol4 myprogram.dll

# Show listing and statistics
snobol4 -a myprogram.sno

# Redirect listing output
snobol4 -l -o=myprogram.lst myprogram.sno
```

A brief program:

```snobol
*  Count vowels
        SUBJECT = 'Hello, World'
        VOWELS  = ANY('AEIOUaeiou')
        COUNT   = 0
LOOP    SUBJECT VOWELS =                   :F(DONE)
        COUNT   = COUNT + 1                :(LOOP)
DONE    OUTPUT  = 'Vowels: ' COUNT
END
```
