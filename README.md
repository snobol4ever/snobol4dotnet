# <center>SNOBOL4.NET</center>

A compiler for the SNOBOL4 language written in C# and .NET and runs under Windows, Linux, and Mac OS.

A major goal of the project was to implement SNOBOL4 and the SPITBOL extensions using a high-level language that is readable and safe. Another goal was to match SPITBOL functionality as described in Emmer and Quillen's *MACRO SPITBOL: The High-Performance SNOBOL4 Language*, otherwise noted as "the SPITBOL manual".

To run Snobol4.Net, type Snobol4 on a command line. As a rule, any statement about SPITBOL in the SPITBOL manual, applies to SNOBOL4.NET. Original SNOBOL4 and SNOBOL4+ features not supported by SPITBOL are not implemented in SNOBOL4.NET. The  Extensions that differ in their implementation from SPITBOL are outlined in this document. 

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

### Experienced Users
If you want to run an existing SPITBOL program, consider the following items that may need to change:

#### Command-line options

The following command line options function the same as SPITBOL. 

* -a   equivalent to -c, -l -x
* -b   Suppress sign on message
* -c   Show Compiler statistics
* -f   Case folding OFF
* -F   Case folding ON
* -h   Suppress listing header
* -k   Stop on run time error
* -l   Show listing
* -n   Suppress execution. Compile only.
* -o   Redirect STDERR to list file
* -u   String passed to HOST(0)
* -v   Generate debug symbols
* -x   Show execution statistics
* -?   Display manual

The following are new to SNOBOL4.NET:

* -w   Write binary code to a dynamic linked library

The .Net framework automatically optimizes all memory allocation on a program-by-program basis and cannot be modified by the users. As such, the following command-line options are no longer supported:

* -d   Max heap size
* -i   Initial heap size
* -m   Max object size
* -s   Stack size

The show listing command line option (-l) creates a listing similar to SPITBOL, but the following command line options are not implemented. A text editor or word processor can manage SPITBOL's command line options as well as preferences not available in SPITBOL, such as line spacing, indentations, justification, and fonts.

* -g   Listing page length
* -p   Long listing format
* -t   Listing line width
* -z   Standard listing options

The following command line options are not implemented:

* -e   Errors to list file only (can be replaced with -o)
* -r   Input from source file following END statement
* -T   Write terminal output to file (Can be accomplished by redirecting STDIO or STDERR)
* -#   Associate file with I/O channel number (Can be done inline with INPUT() or OUTPUT() functions.



















#### INPUT() and OUTPUT() functions.

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