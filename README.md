# <center>SNOBOL4.NET</center>

A compiler for the SNOBOL4 language written in C# and .NET and runs under Windows, Linux, and Mac OS.

A major goal of the project was to implement SNOBOL4 and the SPITBOL extensions using a high-level language that is readable and safe. Another goal was to match SPITBOL functionality as described in Emmer and Quillen's *MACRO SPITBOL: The High-Performance SNOBOL4 Language*, otherwise noted as "the SPITBOL manual".

As a rule, any statement about SPITBOL in the SPITBOL manual, applies to SNOBOL4.NET. Original SNOBOL4 and SNOBOL4+ features not supported by SPITBOL are not implemented in SNOBOL4.NET. Extensions that differ in their implementation from SPITBOL are outlined in this document. 

One major difference is that SNOBOL4.NET is slower than SPITBOL. This is the trade-off of using a high level language and safe programming practices. Hopefully, people will find the source code readable, and some will improve the code for readability and speed.

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

SNOBOL4.NET is graded by the snobol4ever test harness on 35 suites of SNOBOL4 programs, 4,166 programs in all, collected from the sources named below. Each program's output is compared with what SPITBOL x64 prints for it (`sbl -bf`). A program SPITBOL gives no answer for is excluded rather than counted. Graded 18 and 19 November 2026, at commit `1b8d437`.

| Suite | What it is | Source | Pass | Total | Lines of code | Excluded | Time | Percent | Fails (Δ) |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|
| Snobol4AISnobol | the programs of Shafto's *Artificial Intelligence Programming in SNOBOL4* | Catspaw, Inc. (Shafto's report, 1987 diskette) | 8 | 8 | 2,980 | 1 | 4s | 100% | ✓ |
| Snobol4Begoon | the examples of a SNOBOL4 in TypeScript | [begoon/snobol](https://github.com/begoon/snobol) | 11 | 11 | 133 | 0 | 1s | 100% | ✓ |
| Snobol4Bench | the benchmark kernels, graded for their answers | [snobol4ever/corpus](https://github.com/snobol4ever/corpus) | 23 | 23 | 349 | – | 20s | 100% | ✓ |
| Snobol4BinTree | a binary-tree traversal | [ad2108/traverse-binary-tree-in-snobol4](https://github.com/ad2108/traverse-binary-tree-in-snobol4) | 1 | 1 | 0 | 0 | 0s | 100% | ✓ |
| Snobol4Brooker | programs from a college course | [cbrooker-27/snobol](https://github.com/cbrooker-27/snobol) | 21 | 21 | 1,829 | 1 | 2s | 100% | ✓ |
| Snobol4Budne | Philip L. Budne's CSNOBOL4 2.3.3 test suite | [regressive.org/snobol4/csnobol4](http://www.regressive.org/snobol4/csnobol4) | 106 | 106 | 2,761 | 26 | 9s | 100% | ✓ |
| Snobol4Cases | every SNOBOL4 test case the fleet's retired gate scripts carried | [snobol4ever/corpus](https://github.com/snobol4ever/corpus) | 532 | 532 | 6,320 | 81 | 47s | 100% | ✓ |
| Snobol4Cor24 | the examples of a SNOBOL4 for the COR24 machine | [sw-embed/sw-cor24-snobol4](https://github.com/sw-embed/sw-cor24-snobol4) | 54 | 54 | 1,945 | 5 | 4s | 100% | ✓ |
| Snobol4Decus | the samples of Wendell Turner's VAX-11 SNOBOL4 | [DECUS tape VAX84A](https://www.digiater.nl/openvms/decus/vax84a/sdc/snobol/) | 13 | 13 | 1,930 | 0 | 1s | 100% | ✓ |
| Snobol4Demo | demonstration programs: beauty, porter, treebank, json, roman and others | [snobol4ever/corpus](https://github.com/snobol4ever/corpus) | 24 | 24 | 1,998 | – | 8s | 100% | ✓ |
| Snobol4Dotnet | this repository's own programs | [snobol4ever/snobol4dotnet](https://github.com/snobol4ever/snobol4dotnet) | 11 | 11 | 322 | 3 | 1s | 100% | ✓ |
| Snobol4Dump | programs written while learning SNOBOL4 | [James-P-D/SNOBOLDump](https://github.com/James-P-D/SNOBOLDump) | 5 | 5 | 74 | 0 | 0s | 100% | ✓ |
| Snobol4Exercism | the Exercism SNOBOL4 track's exercises | [exercism/snobol4](https://github.com/exercism/snobol4) | 72 | 72 | 1,324 | 0 | 6s | 100% | ✓ |
| Snobol4Exs | exercises from Forte (1967) and Griswold, Poage and Polonsky (1971) | [bitfragment/exs-sno](https://github.com/bitfragment/exs-sno) | 24 | 24 | 627 | 2 | 2s | 100% | ✓ |
| Snobol4Flake | Snoflake's tests: small programs that each exercise one feature | [atdt/snoflake](https://github.com/atdt/snoflake) | 148 | 148 | 5,275 | 32 | 13s | 100% | ✓ |
| Snobol4Gimpel | the programs of Gimpel's *Algorithms in SNOBOL4*, with test drivers | Catspaw, Inc. (Mark Emmer's SPITBOL edition) | 145 | 145 | 2,591 | 0 | 15s | 100% | ✓ |
| Snobol4Haskell | the examples of a SNOBOL4 in Haskell | [meln5674/snobol4hs](https://github.com/meln5674/snobol4hs) | 18 | 18 | 149 | 0 | 2s | 100% | ✓ |
| Snobol4JmuCS430 | a university course's teaching programs | [James Madison University, CS430](https://users.cs.jmu.edu/adamses/Web/CS430/CodeExamples/Snobol/) | 39 | 39 | 912 | 3 | 4s | 100% | ✓ |
| Snobol4MatrixMike | MatrixMike's SNOBOL4 tests | [MatrixMike/snobol4Tests](https://github.com/MatrixMike/snobol4Tests) | 17 | 17 | 324 | 2 | 2s | 100% | ✓ |
| Snobol4MikeBeer | tests, examples and benchmarks of a SNOBOL4 in CREXX and C | [mikebeer/snobol](https://github.com/mikebeer/snobol) | 19 | 19 | 503 | 0 | 3s | 100% | ✓ |
| Snobol4Playground | Joey Gibson's SNOBOL4 experiments | [joeygibson/snobol-playground](https://github.com/joeygibson/snobol-playground) | 26 | 26 | 383 | 0 | 3s | 100% | ✓ |
| Snobol4Procmail | a toy procmail in SPITBOL | [hurufu/snobol-procmail](https://github.com/hurufu/snobol-procmail) | 1 | 1 | 3 | 0 | 0s | 100% | ✓ |
| Snobol4Rosetta | every SNOBOL4 solution on Rosetta Code | [acmeism/RosettaCodeData](https://github.com/acmeism/RosettaCodeData) | 203 | 203 | 2,449 | 7 | 17s | 100% | ✓ |
| Snobol4Rungs | the SNOBOL4 master suite: one construct a program, and regression witnesses | [snobol4ever/corpus](https://github.com/snobol4ever/corpus) | 2,401 | 2,401 | 25,073 | 3 | 3m16s | 100% | ✓ |
| Snobol4Setl4 | Dave Shields's SETL4, an implementation of SETL in SPITBOL | [setl4/setl4](https://github.com/setl4/setl4) | 11 | 11 | 1,372 | 0 | 1s | 100% | ✓ |
| Snobol4Sil | the test programs of a SIL interpreter in Go | [mdhender/sil](https://github.com/mdhender/sil) | 23 | 23 | 386 | 0 | 3s | 100% | ✓ |
| Snobol4Slobol | the test and trace programs of a SPITBOL in Go | [spitbol/slobol](https://gitlab.com/spitbol/slobol) | 19 | 19 | 3,344 | 0 | 4s | 100% | ✓ |
| Snobol4Snolib | the self-checking tests of Fred Weigel's SNOBOL4 library | [ratboy666/snolib](https://github.com/ratboy666/snolib) | 63 | 63 | 661 | 6 | 8s | 100% | ✓ |
| Snobol4Spitbol88 | the programs of Catspaw's SPITBOL-88 distribution | [spitbol/88-binary](https://github.com/spitbol/88-binary) | 15 | 15 | 2,328 | 0 | 2s | 100% | ✓ |
| Snobol4StrangeCode | the SNOBOL4 programs of Kneusel's *Strange Code* | [rkneusel9/StrangeCodeBook](https://github.com/rkneusel9/StrangeCodeBook) | 16 | 16 | 424 | 4 | 1s | 100% | ✓ |
| Snobol4TestPgms | SPITBOL's four standard diagnostic programs | SPITBOL's own distribution | 8 | 8 | 1,267 | 0 | 1s | 100% | ✓ |
| Snobol4Tkslide | the examples of the TkS*LIDE SNOBOL4 IDE | [tkslide/tkslide](https://github.com/tkslide/tkslide) | 15 | 15 | 153 | 0 | 1s | 100% | ✓ |
| Snobol4Vanilla | the sample programs of Catspaw's Vanilla SNOBOL4 | Catspaw, Inc. (Vanilla SNOBOL4) | 19 | 19 | 1,819 | 2 | 2s | 100% | ✓ |
| Snobol4X32Tests | SPITBOL x32's own test programs | [spitbol/x32](https://github.com/spitbol/x32) | 18 | 18 | 675 | 3 | 2s | 100% | ✓ |
| Snobol4X64Tests | SPITBOL x64's own test programs | [spitbol/x64](https://github.com/spitbol/x64) | 37 | 37 | 81,688 | 8 | 28s | 100% | ✓ |
| **Total** | 35 suites | | **4,166** | **4,166** | **154,371** | **189** | **6m53s** | **100%** | **✓** |

*Lines of code* is the size of the suite's programs. *Time* is how long the suite's last grading took. *Fails (Δ)* is how many of its programs still print something other than SPITBOL's answer; ✓ means none.

### Experienced Users
If you want to run an existing SPITBOL program, consider the following items that may need to change:

* Command-line options

Check the command-line options. Some command line options are not implemented or work differently than SPITBOL.

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