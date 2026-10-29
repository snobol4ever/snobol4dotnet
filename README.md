# <center>SNOBOL4.NET</center>

A compiler for the SNOBOL4 language written in C# and .NET and runs under Windows, Linux, and Mac OS.

A major goal of the project was to implement SNOBOL4 and the SPITBOL extensions using a high-level language that is readable and safe. Another goal was to match SPITBOL functionality as described in Emmer and Quillen's *MACRO SPITBOL: The High-Performance SNOBOL4 Language*, otherwise noted as "the SPITBOL manual".

To run Snobol4.Net, type Snobol4 on a command line. As a rule, any statement about SPITBOL in the SPITBOL manual, applies to SNOBOL4.NET. Original SNOBOL4 and SNOBOL4+ features not supported by SPITBOL are not implemented in SNOBOL4.NET. The  Extensions that differ in their implementation from SPITBOL are outlined in this document. 

One major difference is that SNOBOL4.NET is slower than SPITBOL. This is the trade-off of using a high level language and safe programming practices. Hopefully, people will find the source code readable, and some will improve the code for readability and speed.

## Test Results

SNOBOL4.NET is tested two ways: with its own unit tests, and by running whole SNOBOL4 programs and comparing their output with SPITBOL's.

**Unit tests.** `TestSnobol4` (MSTest), run in batches at commit `c9e85c9` on 2026-10-29: **3,343 tests, 3,331 passed, 0 failed, 12 skipped.**

**Program suites.** Each program is run by SNOBOL4.NET, and its output is compared with the output of SPITBOL x64 (`sbl -bf`), the reference implementation. Programs SPITBOL gives no answer for are excluded rather than counted: SPITBOL refuses or crashes on them, or their expected output is not known. They appear in the *Excluded* column. The suites were graded between 2026-10-22 and 2026-10-29 by the snobol4ever test harness, which runs the same programs against SNOBOL4.NET and against SCRIP. *Graded at* names the SNOBOL4.NET commit each suite was last graded on.

| Suite | What it is | Passed | Excluded | Graded at |
|---|---|---:|---:|---|
| SnoRungs | the SNOBOL4 master suite: construct-by-construct programs and regression witnesses | 2,185 / 2,185 | 1 | `06e21c1` |
| Gimpel | the programs of Gimpel's *Algorithms in SNOBOL4*, with their test drivers | 144 / 144 | 1 | `38cdeac` |
| Flake | the snoflake suite: small programs that each exercise one feature | 126 / 126 | 54 | `a3f5f8f` |
| Budne | Philip L. Budne's CSNOBOL4 2.3.3 test suite | 68 / 68 | 64 | `38cdeac` |
| X64T | SPITBOL x64's own test programs | 33 / 33 | 3 | `38cdeac` |
| SnoDemo | demonstration programs (beauty, treebank, porter, roman, json and others) | 24 / 24 | – | `749359e` |
| SnoBench | benchmark programs | 23 / 23 | – | `6aa2b8e` |
| X32T | SPITBOL x32's own test programs | 16 / 16 | 5 | `4fac20f` |
| TPgm | SPITBOL's four standard diagnostic programs | 8 / 8 | 0 | `cb2da46` |
| AIS | the aisnobol programs (an ATN parser among them) | 8 / 8 | 1 | `d0c88de` |
| Dotnet | programs from this repository's own collection | 6 / 6 | 8 | `b2b059f` |
| **Total** | | **2,641 / 2,641** | **137** | |

Every suite passes all of its graded programs.

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