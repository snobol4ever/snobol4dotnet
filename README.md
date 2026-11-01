# <center>SNOBOL4.NET</center>

A compiler for the SNOBOL4 language written in C# and .NET and runs under Windows, Linux, and Mac OS.

A major goal of the project was to implement SNOBOL4 and the SPITBOL extensions using a high-level language that is readable and safe. Another goal was to match SPITBOL functionality as described in Emmer and Quillen's *MACRO SPITBOL: The High-Performance SNOBOL4 Language*, otherwise noted as "the SPITBOL manual".

To run Snobol4.Net, type Snobol4 on a command line. As a rule, any statement about SPITBOL in the SPITBOL manual, applies to SNOBOL4.NET. Original SNOBOL4 and SNOBOL4+ features not supported by SPITBOL are not implemented in SNOBOL4.NET. The  Extensions that differ in their implementation from SPITBOL are outlined in this document. 

One major difference is that SNOBOL4.NET is slower than SPITBOL. This is the trade-off of using a high level language and safe programming practices. Hopefully, people will find the source code readable, and some will improve the code for readability and speed.

## Test Results

SNOBOL4.NET is tested two ways: with its own unit tests, and by running whole SNOBOL4 programs and comparing their output with SPITBOL's.

**Unit tests.** `TestSnobol4` (MSTest), run in batches at commit `c9e85c9` on 2026-10-29: **3,343 tests, 3,331 passed, 0 failed, 12 skipped.**

**Program suites.** Each program is run by SNOBOL4.NET, and its output is compared with the output of SPITBOL x64 (`sbl -bf`), the reference implementation. Programs SPITBOL gives no answer for are excluded rather than counted: SPITBOL refuses or crashes on them, or their expected output is not known. They appear in the *Excluded* column. The suites were all graded on 2026-10-31 by the snobol4ever test harness, which runs the same programs against SNOBOL4.NET and against SCRIP. *Graded at* names the SNOBOL4.NET commit each suite was last graded on.

| Suite | What it is | Passed | Excluded | Graded at |
|---|---|---:|---:|---|
| SnoRungs | the SNOBOL4 master suite: construct-by-construct programs and regression witnesses | 2,192 / 2,192 | 1 | `ff4eedc` |
| Gimpel | the programs of Gimpel's *Algorithms in SNOBOL4*, with their test drivers | 144 / 144 | 1 | `ff4eedc` |
| Flake | the snoflake suite: small programs that each exercise one feature | 126 / 126 | 54 | `ff4eedc` |
| Budne | Philip L. Budne's CSNOBOL4 2.3.3 test suite | 68 / 68 | 64 | `ff4eedc` |
| X64T | SPITBOL x64's own test programs | 33 / 33 | 3 | `ff4eedc` |
| SnoDemo | demonstration programs (beauty, treebank, porter, roman, json and others) | 24 / 24 | – | `ff4eedc` |
| SnoBench | benchmark programs | 23 / 23 | – | `ff4eedc` |
| X32T | SPITBOL x32's own test programs | 16 / 16 | 5 | `ff4eedc` |
| TPgm | SPITBOL's four standard diagnostic programs | 8 / 8 | 0 | `ff4eedc` |
| AIS | the aisnobol programs (an ATN parser among them) | 8 / 8 | 1 | `ff4eedc` |
| Dotnet | programs from this repository's own collection | 6 / 6 | 8 | `ff4eedc` |
| **Total** | | **2,648 / 2,648** | **137** | |

Every suite passes all of its graded programs.

## Benchmarks

These are instruction counts, not times. Each figure is the number of user-space instructions the process executed (`perf stat -e instructions:u`, which also counts the threads the process starts: the .NET JIT compiler and garbage collector are included). A count does not move with the load on the machine, as a clock does. It is also not a run time: SPITBOL's native code and .NET's JIT-compiled code retire instructions at different rates.

Each program was run three times and the median is shown. Start-up is counted on its own (next section) and taken out of every program's figure: a figure is the program's count less the count of an empty program (`END` alone) on the same engine. Programs whose three runs differed by more than 5% were run seven times, and **†** marks the five that still differ by more than 5% (calculator-1-match 5.4%, arith_loop_twin 5.5%, calculator-1-match-fence 7.5%, treebank-alloc 13%, json-match 42%): read those as a range, not a point. calculator-2-match counted 1.47G in its first three runs and 1.11G in seven (spread 0.01%); the seven are shown.

The engines, measured on 2026-10-31 and 2026-11-01 on Linux x86-64:

- **SNOBOL4.NET**: commit `ff4eedc`, Release build with the ReadyToRun pass, .NET 10.
- **SPITBOL**: SPITBOL x64 4.0f built from source with two compatibility patches (upper-case keyword names, a monotonic clock) and none of the monitor instrumentation, run as `sbl -bf`, with the stack size a program declares where it declares one.
- **CSNOBOL4**: version 2.3.3 (Philip L. Budne), the snobol4ever build, run as `snobol4 -b`.

A program counts for CSNOBOL4 only when its output is the same as SPITBOL's. **vs SPITBOL** and **vs CSNOBOL4** are the reference engine's count divided by SNOBOL4.NET's: 1.00x is parity, 0.50x is half the reference's count. Every multiple below is under 1.00x: on every program SNOBOL4.NET executes more instructions than SPITBOL and than CSNOBOL4.

### Start-up

Start-up is an empty program (`END` alone), counted nine times (median) and timed 25 times in a quiet slice of the machine (load 3.5 on 16 cores).

- SPITBOL: 196,172 instructions, 0.89 ms.
- CSNOBOL4: 577,110 instructions, 5.79 ms.
- SNOBOL4.NET: 234,864,959 instructions, 58.8 ms (`dotnet Snobol4.dll`).

Start-up is not the whole fixed cost of a SNOBOL4.NET program: `hello`, a program of one `OUTPUT` statement, still counts 53.4M beyond it. This README does not break that down further.

### General benchmarks

The 23 kernels of `corpus/benchmarks/snobol4`: small programs, each built around one kind of work (arithmetic, function calls, strings, pattern matching, tables, `EVAL`).

| Program | SPITBOL | CSNOBOL4 | SNOBOL4.NET | vs SPITBOL | vs CSNOBOL4 |
|---|---:|---:|---:|---:|---:|
| arith_loop | 231.9K | 1.3M | 104.8M | 0.0022x | 0.013x |
| arith_loop_twin | 77.2M | 365.4M | 3.23G † | 0.024x | 0.11x |
| array_sum | 5.1M | 32.1M | 399.0M | 0.013x | 0.080x |
| eval_fixed | 1.2M | 4.1M | 233.6M | 0.0053x | 0.018x |
| fib_recur | 47.0M | 208.2M | 4.19G | 0.011x | 0.050x |
| fibonacci | 2.6M | 11.7M | 255.9M | 0.010x | 0.046x |
| func_call | 421.7K | 2.0M | 121.0M | 0.0035x | 0.016x |
| ident_call1 | 287.4K | 1.6M | 108.7M | 0.0026x | 0.015x |
| ident_call2 | 284.0K | 1.6M | 109.5M | 0.0026x | 0.014x |
| indirect_dispatch | 319.8K | 1.8M | 113.8M | 0.0028x | 0.016x |
| mixed_workload | 375.3K | 1.5M | 160.8M | 0.0023x | 0.0093x |
| name_indirection | 9.0M | 50.6M | 404.4M | 0.022x | 0.13x |
| op_dispatch | 473.4K | 2.7M | 126.3M | 0.0037x | 0.021x |
| pattern_bt | 1.7M | 7.7M | 263.5M | 0.0064x | 0.029x |
| roman | 1.2M | 8.5M | 245.7M | 0.0049x | 0.034x |
| string_concat | 239.5K | 1.8M | 107.6M | 0.0022x | 0.017x |
| string_concat_twin | 1.1M | 7.0M | 149.8M | 0.0075x | 0.047x |
| string_manip | 517.1K | 2.5M | 137.8M | 0.0038x | 0.018x |
| string_pattern | 754.4K | 3.9M | 175.7M | 0.0043x | 0.022x |
| table_access | 7.1M | 94.2M | 396.3M | 0.018x | 0.24x |
| table_variety | 4.4M | 22.3M | 327.8M | 0.013x | 0.068x |
| test_icon | 251.3K | 291.6K | 153.8M | 0.0016x | 0.0019x |
| var_access | 553.2K | 3.0M | 133.9M | 0.0041x | 0.022x |

### Work-horse demonstration programs

The demonstration programs of `corpus/demos/snobol4`, each on the input its `.workhorse` file names (some repeated to a larger size). `beauty`, the 24th demonstration program, is not counted: it reads sixteen include files.

| Program | SPITBOL | CSNOBOL4 | SNOBOL4.NET | vs SPITBOL | vs CSNOBOL4 |
|---|---:|---:|---:|---:|---:|
| arithmetic | 16.3K | 25.9K | 73.0M | 0.00022x | 0.00036x |
| calculator-1 | 138.8M | no `FENCE(P)` | 10.09G | 0.014x | – |
| calculator-1-match | 314.9M | different answer | 12.98G † | 0.024x | – |
| calculator-1-match-fence | 123.0M | no `FENCE(P)` | 9.06G † | 0.014x | – |
| calculator-2 | 155.8M | no `FENCE(P)` | 11.14G | 0.014x | – |
| calculator-2-match | 12.0M | different answer | 1.11G | 0.011x | – |
| calculator-2-match-fence | 14.4M | no `FENCE(P)` | 1.86G | 0.0078x | – |
| claws5 | 671.2M | no `FENCE(P)` | 15.37G | 0.044x | – |
| claws5-match | 57.1M | different answer | 2.75G | 0.021x | – |
| claws5-match-fence | 60.8M | no `FENCE(P)` | 3.21G | 0.019x | – |
| counter | 13.3K | 24.1K | 84.9M | 0.00016x | 0.00028x |
| hello | 3.6K | 4.3K | 53.4M | 0.000068x | 0.000081x |
| json | 307.4M | no `FENCE(P)` | 13.88G | 0.022x | – |
| json-match | 45.1M | different answer | 2.58G † | 0.018x | – |
| json-match-fence | 50.1M | no `FENCE(P)` | 3.23G | 0.016x | – |
| pattern_test | 9.8K | 13.0K | 77.7M | 0.00013x | 0.00017x |
| porter | 5.20G | 29.54G | 367.18G | 0.014x | 0.080x |
| roman | 2.0M | 13.8M | 311.8M | 0.0063x | 0.044x |
| treebank | 1.71G | no `FENCE(P)` | 36.79G | 0.047x | – |
| treebank-alloc | 3.30G | no `FENCE(P)` | 13.18G † | 0.25x | – |
| treebank-match | 16.0M | different answer | 1.24G | 0.013x | – |
| treebank-match-fence | 17.2M | no `FENCE(P)` | 1.39G | 0.012x | – |
| wordcount | 589.3M | 2.24G | 16.29G | 0.036x | 0.14x |

CSNOBOL4 gives no figure for 16 of these programs, which are left out rather than counted as slow. Eleven use `FENCE(P)`, which this CSNOBOL4 build does not define (`calculator-1`, `calculator-2`, `claws5`, `json`, `treebank`, `treebank-alloc` and the five `-match-fence` programs). The other five (the `-match` programs without `FENCE`) read their input through SPITBOL's `INPUT(.INPUT, 9, '[-f0 -r4194304]')` record option, and CSNOBOL4 prints a different answer for them.

## Chapter 1: Installation

### About This Manual
#### Scope

[Replace with:]

This manual covers the .NET implementation of SNOBOL4 and the SPITBOL extensions. SNOBOL4.NET runs on Windows, Linux, and Mac OS.

### MS-DOS, Windows 95, Windows NT

[Deleted]

### Windows 11 or later

The Windows version consist of three files:

* Snobol4.exe
* Snobol4W.exe
* Snobol.Common.dll

These three files should be stored in the same folder and the folder placed anywhere accessible from the search path. Snobol4.exe is the command line version and Snobol4W is the Windows version. Snobol4.Common.dll is the code in common with both.

### Installing SPITBOL

[Deleted]

### DOS-Extended SPITBOL-386

[Deleted]

### SPITBOL-8088

[Deleted]

### Checkout

### Experienced Users
If you want to run an existing SPITBOL program, consider the following items that may need to change:

#### Command-line options

The following command line options function the same as SPITBOL. The listing and statistics options write what SPITBOL writes, in
SPITBOL's layout.

* -a   equivalent to -l -c -x
* -b   Suppress sign on message
* -c   Show compiler statistics, on the listing stream
* -e   Errors to the list file only: the lines of an error report are not also written to standard error
* -f   Case folding OFF
* -F   Case folding ON
* -g#  Listing page length (60)
* -h   Suppress the identity and date lines of the listing head
* -k   Run despite compilation errors (a statement in error is error 7 when reached)
* -l   List the source to the first source file's name with the extension .lst (to the -o file when there is one)
* -n   Suppress execution. Compile only. The exit status is 231.
* -o=file   The listing file (file.lst when it has no extension)
* -p   Long listing format (form feeds between pages)
* -r   INPUT reads the source lines that follow END
* -t#  Listing line width (120)
* -u   String passed to HOST(0)
* -x   Show execution statistics, on the listing stream
* -z   Standard listing options (the same as -p here)
* -#=file[options]   Associate a file with I/O channel number #
* -?   Display manual

The listing, the statistics and the reports of errors go to standard output unless -l, -a, -p, -z or -o sends them to a file. Wherever
they go, standard error also gets a copy of the lines of an error report (the error line and its position and run summary, or a
statement in error with its pointer) when the listing is not the terminal, as SPITBOL's interactive channel does.

The exit status is 231 when execution was suppressed (a compilation error without -k, or -n), 1 when the source could not be
read (a file that cannot be opened, no END statement, an -INCLUDE that cannot be opened), and otherwise &CODE.

The following are new to SNOBOL4.NET:

* -v   Generate debug symbols
* -w   Write binary code to a dynamic linked library (the program's source and options, which Snobol4 runs again)

The .Net framework automatically optimizes all memory allocation on a program-by-program basis and cannot be modified by the users. As such, the following command-line options are accepted and have no effect (-m sets &MAXLNGTH):

* -d   Max heap size
* -i   Initial heap size

The following command line options are not implemented:

* -T   Write terminal output to file (Can be accomplished by redirecting STDIO or STDERR)
* -y   Write a save file

-s is the stack of SPITBOL's program (4m by default) and it is kept: the program runs on a thread whose .NET stack is sized from it, and a
call of a function takes 64 bytes of it and 8 for each parameter, local and operand waiting in the calling expression, as SPITBOL's
stack takes them, from the 3,216 bytes it starts with; past -s it is error 246, stack overflow (58,209 levels of a function with one
parameter at the default). A recursion that sbl allows no longer dies of the .NET stack, and one it refuses is refused here.

EXIT(n, file) with a negative n writes a save file (a.spx without a name; a real is cut toward zero, a numeric string is its number) and
ends the run, except -4, which goes on. The file holds the program, as -w writes it, and not its state: running it starts the program over,
where SPITBOL's resumes after the EXIT. An EXIT that ends the run leaves at once, with no end-of-run dump and no -x statistics. EXIT(s)
runs the command s through the shell (SHELL, else /bin/sh) and ends the run with its status. A load module (EXIT(3)) is error 105, as in
SPITBOL, and a second argument that is no string is error 288.

#### INPUT() and OUTPUT() functions.

In SPITBOL the arguments are INPUT(.Variable, Channel, "filename[options]") and the same for OUTPUT(); SPITBOL ignores anything after the third. In SNOBOL4.NET a fourth argument is the file mode option, a fifth the file share option, and a sixth for OUTPUT controls whether an end of line is printed. If 0 (the default), an end of line is printed. If non-zero, an end of line is not printed. An argument that is no value of its kind is ignored, as SPITBOL ignores it. If options are not used, INPUT() and OUTPUT() function identically between SNOBOL4.NET and SPITBOL. As in SPITBOL a file name needs a channel to be opened on (INPUT 116, OUTPUT 160 without one) and a channel with nothing open on it needs a file name.

The options of the file name, in square brackets or after a blank, are SPITBOL's: -a appends, -f# uses a file descriptor (-f0 is standard input, -f1 and -f2 standard output and error), -l# is the longest record INPUT returns (1,024 by default; characters beyond it up to the end of the line are ignored), and -r# or -q# or -c read in raw mode: an input request returns exactly # characters (1 for -c), line ends included, nothing is trimmed, and the request fails when none is left (INPUT(.INPUT, 9, '[-f0 -r4194304]') takes all of standard input at once). In SNOBOL4.NET records are counted in characters, not bytes, and fixed length records are not supported due to conflicts with UNICODE support.

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