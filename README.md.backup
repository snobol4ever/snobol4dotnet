# <center>SNOBOL4.NET</center>

A compiler for the SNOBOL4 language written in C# and .NET and runs  under Windows and Linux.

A major goal of the project was to match SPITBOL as described in Emmer and Quillen's *MACRO SPITBOL: The High-Performance SNOBOL4 Language*, otherwise noted as "the SPITBOL manual". SNOBOL4 features not supported by SPITBOL are not implemented in SNOBOL4.NET. Extensions that differ in their implementation are outlined in this document.

## Chapter 1: Installation

The Windows verions consist of three files:

* Snobol4.exe
* Snobol4W.exe
* Snobol.Common.dll

These three files should be stored in the same folder and the folder placed anywhere accessible from the search path. Snobol4 is the command line version and Snobol4 is the Windows version. Snobol4.Common.dll is the code in common with both.

### Experienced Users
If you want to run an existing SPITBOL program, consider the following items that may need to change:

* Command-line options

Check the command-line options. Some command line options are not implemented or work differently than SPITBOL.

* INPUT() and OUTPUT() functions.

In SPITBOL the arguments are INPUT(.Variable, Channel, "filename[options]) and the same for OUTPUT(). In SNOBOL4.NET, the options are moved to the fourth argument. File IO options are based on .NET stream options, rather than Linux file options. If options are not used, INPUT() and OUTPUT() function identically between SNOBOL4.NET and SPITBOL
