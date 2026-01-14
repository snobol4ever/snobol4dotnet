using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snobol4.Common;

/// <summary>
/// Factory methods for creating SNOBOL4 pattern instances from function calls.
/// This partial class extends Executive with pattern construction functionality.
/// </summary>
/// <remarks>
/// <para>
/// These factory methods are invoked when SNOBOL4 code calls pattern functions like
/// ANY(), SPAN(), BREAK(), LEN(), etc. Each method validates arguments, converts
/// them to appropriate types, and constructs the corresponding pattern object.
/// </para>
/// <para>
/// Pattern creation follows a consistent pattern:
/// 1. Extract and validate arguments from the argument list
/// 2. Convert arguments to required types (STRING, INTEGER, PATTERN)
/// 3. Log runtime exceptions for invalid arguments
/// 4. Construct the pattern object
/// 5. Push a PatternVar wrapping the pattern onto the system stack
/// </para>
/// <para>
/// Terminal pattern functions with string input have specific null handling rules:
/// <list type="table">
/// <listheader>
/// <term>Pattern</term>
/// <description>Input can be null | Matches null string</description>
/// </listheader>
/// <item>
/// <term>ANY(string)</term>
/// <description>No | No</description>
/// </item>
/// <item>
/// <term>BREAK(string)</term>
/// <description>No | Yes</description>
/// </item>
/// <item>
/// <term>BREAKX(string)</term>
/// <description>No | Yes</description>
/// </item>
/// <item>
/// <term>NOTANY(string)</term>
/// <description>No | No</description>
/// </item>
/// <item>
/// <term>SPAN(string)</term>
/// <description>No | No</description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public partial class Executive
{
    #region Integer argument pattern helper

    /// <summary>
    /// Converts and validates a variable argument as a non-negative integer for pattern functions.
    /// </summary>
    /// <param name="argument">The variable to convert to an integer</param>
    /// <param name="notNumeric">Error code to log if argument is not numeric</param>
    /// <param name="outOfRange">Error code to log if integer is negative or too large</param>
    /// <returns>
    /// The integer value if valid (0 to int.MaxValue),
    /// or 0 if conversion fails or value is out of range
    /// </returns>
    /// <remarks>
    /// <para>
    /// This helper method is used by pattern functions that require integer arguments
    /// (LEN, POS, RPOS, TAB, RTAB). It ensures the argument is:
    /// - Convertible to an integer type
    /// - Non-negative (>= 0)
    /// - Within int.MaxValue range
    /// </para>
    /// <para>
    /// If validation fails, a runtime exception is logged and 0 is returned.
    /// The calling pattern factory will typically continue with the value 0,
    /// which may result in a valid but potentially unexpected pattern behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid usage
    /// var length = GetInteger(arguments[0], 120, 121);
    /// // If arguments[0] = 10, returns 10
    /// // If arguments[0] = -5, logs error 121, returns 0
    /// // If arguments[0] = "test", logs error 120, returns 0
    /// </code>
    /// </example>
    internal int GetInteger(Var argument, int notNumeric, int outOfRange)
    {
        if (!argument.Convert(VarType.INTEGER, out _, out var n, this))
        {
            LogRuntimeException(notNumeric);
            return 0;
        }

        var l = (long)n;

        if (l is >= 0 and <= int.MaxValue)
            return (int)l;

        LogRuntimeException(outOfRange);
        return 0;
    }

    #endregion

    #region Create ANY Pattern (STRING)

    /// <summary>
    /// Creates an ANY pattern that matches a single character from a specified set.
    /// In SNOBOL4: ANY(string)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the character set (string or expression)
    /// </param>
    /// <remarks>
    /// <para>
    /// ANY matches exactly one character that appears in the character set string.
    /// The pattern fails if the cursor is at the end of the subject or if the
    /// current character is not in the set.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a string: Logs error 59 (argument must be non-null string)
    /// - If argument is empty string: Logs error 59
    /// - If argument is an expression: Pushes expression for later evaluation
    /// </para>
    /// <para>
    /// The character set cannot be null or empty, as there would be no characters to match.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// vowel = any('aeiou')    // Matches single vowel
    /// digit = any('0123456789') // Matches single digit
    /// 
    /// // Expression argument (evaluated at match time)
    /// charset = 'abc'
    /// pattern = any(charset)
    /// </code>
    /// </example>
    public void CreateAnyPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new AnyPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(59);
            return;
        }

        SystemStack.Push(new PatternVar(new AnyPattern((string)s)));
    }

    #endregion

    #region Create ARB Pattern ()

    /// <summary>
    /// Creates an ARB (arbitrary string) pattern that matches any sequence of characters.
    /// In SNOBOL4: ARB
    /// </summary>
    /// <param name="_">Arguments list (unused, ARB takes no arguments)</param>
    /// <remarks>
    /// <para>
    /// ARB matches any sequence of characters (including empty string) using backtracking
    /// to find the shortest match that allows the overall pattern to succeed.
    /// </para>
    /// <para>
    /// ARB is implemented as a complex structure: NULL (NULL | ARB) to enable proper
    /// backtracking behavior. Initially matches zero characters, then progressively
    /// tries longer matches via backtracking if needed.
    /// </para>
    /// <para>
    /// This is one of the most powerful and commonly used patterns in SNOBOL4.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'prefix-data-suffix'
    /// subject 'prefix-' arb . middle '-suffix'
    /// // middle = "data"
    /// </code>
    /// </example>
    // ReSharper disable once UnusedMember.Global
    public void CreateArbPattern(List<Var> _)
    {
        SystemStack.Push(new PatternVar(ArbPattern.Structure()));
    }

    #endregion

    #region Create ARBNO Pattern (PATTERN)

    /// <summary>
    /// Creates an ARBNO pattern that matches zero or more consecutive occurrences of a pattern.
    /// In SNOBOL4: ARBNO(pattern)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the pattern to repeat
    /// </param>
    /// <remarks>
    /// <para>
    /// ARBNO always succeeds (matching zero occurrences if necessary) and uses backtracking
    /// to try progressively more matches. It's implemented as: NULL (NULL | ARBNO(pattern))
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a pattern: Logs error 70 (argument must be pattern)
    /// </para>
    /// <para>
    /// ARBNO is essential for matching variable-length sequences like lists, repeated
    /// keywords, or arbitrary repetitions of a pattern.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// item = span('0123456789')
    /// list = pos(0) '(' item arbno(',' item) ')' rpos(0)
    /// subject = '(1,2,3)'  // Matches, arbno matches ',2' and ',3'
    /// </code>
    /// </example>
    public void CreateArbNoPattern(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.PATTERN, out _, out var pattern, this))
        {
            LogRuntimeException(70);
            return;
        }

        SystemStack.Push(new PatternVar(ArbNoPattern.Structure((Pattern)pattern)));
    }

    #endregion

    #region Create BREAK Pattern (STRING)

    /// <summary>
    /// Creates a BREAK pattern that matches characters up to a specified break character.
    /// In SNOBOL4: BREAK(string)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the break character set (string or expression)
    /// </param>
    /// <remarks>
    /// <para>
    /// BREAK matches zero or more characters, stopping immediately before the first
    /// character that appears in the break set. The break character itself is NOT consumed.
    /// BREAK always succeeds if any break character is found, even matching zero characters.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a string: Logs error 69 (argument must be non-null string)
    /// - If argument is empty string: Logs error 69
    /// - If argument is an expression: Creates BreakPattern with expression for runtime evaluation
    /// </para>
    /// <para>
    /// Unlike ANY and SPAN, BREAK can have an empty string argument in some contexts,
    /// though it typically should have at least one break character.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'name:value'
    /// subject break(':') . key ':' rem . val
    /// // key = "name", val = "value"
    /// </code>
    /// </example>
    public void CreateBreakPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new BreakPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(69);
            return;
        }

        SystemStack.Push(new PatternVar(new BreakPattern((string)s)));
    }

    #endregion

    #region Create BREAKX Pattern (STRING)

    /// <summary>
    /// Creates a BREAKX (break extended) pattern that matches characters up to and including a break character.
    /// In SNOBOL4: BREAKX(string)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the break character set (string)
    /// </param>
    /// <remarks>
    /// <para>
    /// BREAKX is similar to BREAK but includes the break character in the match.
    /// It's implemented as: BREAK(s) ARBNO(LEN(1) BREAK(s))
    /// </para>
    /// <para>
    /// This structure allows BREAKX to:
    /// 1. Match up to the first break character (BREAK)
    /// 2. Optionally match one character plus more text up to another break character (ARBNO)
    /// 3. Include the final break character in the match
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a string: Logs error 45
    /// - If argument is empty string: Logs error 45
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'path/to/file'
    /// subject breakx('/') . first_segment
    /// // first_segment = "path/" (includes the slash)
    /// </code>
    /// </example>
    public void CreateBreakXPattern(List<Var> arguments)
    {
        var v0 = arguments[0];

        if (!v0.Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string?)s))
        {
            LogRuntimeException(45);
            return;
        }

        var arbNoArg = new ConcatenatePattern(new LenPattern(1), new BreakPattern((string)s));
        var arbNo = ArbNoPattern.Structure(arbNoArg);
        var breakX = new ConcatenatePattern(new BreakPattern((string)s), arbNo);
        SystemStack.Push(new PatternVar(breakX));
    }

    #endregion

    #region Create FENCE function (PATTERN)

    /// <summary>
    /// Creates a FENCE pattern that prevents backtracking past a certain point.
    /// In SNOBOL4: FENCE(pattern)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the pattern to fence
    /// </param>
    /// <remarks>
    /// <para>
    /// FENCE is implemented as: pattern | ABORT
    /// If the pattern succeeds, FENCE succeeds. If the pattern fails, ABORT prevents
    /// all backtracking, terminating the match.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a pattern: Logs error 259 (argument must be pattern)
    /// </para>
    /// <para>
    /// FENCE is crucial for optimizing patterns by eliminating futile backtracking
    /// and implementing committed choice in pattern matching.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'test123'
    /// subject fence('test') span(&amp;digit)
    /// // Commits to 'test' or aborts, no backtracking
    /// </code>
    /// </example>
    public void CreateFenceFunction(List<Var> arguments)
    {
        var v0 = arguments[0];

        if (!v0.Convert(VarType.PATTERN, out _, out var p, this))
        {
            LogRuntimeException(259);
            return;
        }

        SystemStack.Push(new PatternVar(new AlternatePattern((Pattern)p, new AbortPattern())));
    }

    #endregion

    #region Create LEN Pattern (INTEGER)

    /// <summary>
    /// Creates a LEN pattern that matches exactly N characters.
    /// In SNOBOL4: LEN(n)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the number of characters to match (integer)
    /// </param>
    /// <remarks>
    /// <para>
    /// LEN matches a fixed number of characters regardless of content. LEN(0) is valid
    /// and matches zero characters (similar to NULL).
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not numeric: Logs error 120
    /// - If argument is negative or > int.MaxValue: Logs error 121, uses 0
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'ABCDEF'
    /// subject len(3) . first    // first = "ABC"
    /// </code>
    /// </example>
    public void CreateLenPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 120, 121);
        SystemStack.Push(new PatternVar(new LenPattern(i)));
    }

    #endregion

    #region Create NOTANY Pattern (STRING)

    /// <summary>
    /// Creates a NOTANY pattern that matches a single character NOT in a specified set.
    /// In SNOBOL4: NOTANY(string)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the excluded character set (string or expression)
    /// </param>
    /// <remarks>
    /// <para>
    /// NOTANY is the inverse of ANY - it matches one character that does NOT appear
    /// in the specified set. Fails if at end of subject or if character IS in the set.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a string: Logs error 151 (argument must be non-null string)
    /// - If argument is empty string: Logs error 151
    /// - If argument is an expression: Creates NotAnyPattern with expression for runtime evaluation
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// consonant = notany('aeiou')
    /// subject = 'test'
    /// subject consonant . c    // c = "t"
    /// 
    /// // Expression argument (evaluated at match time)
    /// charset = 'abc'
    /// pattern = notany(charset)
    /// </code>
    /// </example>
    public void CreateNotAnyPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new NotAnyPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(151);
            return;
        }

        SystemStack.Push(new PatternVar(new NotAnyPattern((string)s)));
    }

    #endregion

    #region Create POS Pattern (INTEGER)

    /// <summary>
    /// Creates a POS pattern that succeeds only if the cursor is at a specific position.
    /// In SNOBOL4: POS(n)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the required position (integer)
    /// </param>
    /// <remarks>
    /// <para>
    /// POS checks the cursor position without consuming input. POS(0) matches at the
    /// beginning of the string, POS(n) matches after n characters.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not numeric: Logs error 162
    /// - If argument is negative or > int.MaxValue: Logs error 163, uses 0
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'test'
    /// subject pos(0) 'test' rpos(0)  // Anchored match
    /// </code>
    /// </example>
    public void CreatePosPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 162, 163);
        SystemStack.Push(new PatternVar(new PosPattern(i)));
    }

    #endregion

    #region Create RPOS Pattern (STRING)

    /// <summary>
    /// Creates an RPOS pattern that succeeds only if the cursor is at a specific position from the end.
    /// In SNOBOL4: RPOS(n)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the position from end (integer)
    /// </param>
    /// <remarks>
    /// <para>
    /// RPOS checks the cursor position relative to the end of the string without
    /// consuming input. RPOS(0) matches at the end, RPOS(n) matches n characters before the end.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not numeric: Logs error 185
    /// - If argument is negative or > int.MaxValue: Logs error 186, uses 0
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'test.txt'
    /// subject arb '.txt' rpos(0)  // Matches if ends with '.txt'
    /// </code>
    /// </example>
    public void CreateRPosPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 185, 186);
        SystemStack.Push(new PatternVar(new RPosPattern(i)));
    }

    #endregion

    #region Create RTAB Pattern (INTEGER)

    /// <summary>
    /// Creates an RTAB pattern that advances the cursor to a position relative to the end.
    /// In SNOBOL4: RTAB(n)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the position from end (integer)
    /// </param>
    /// <remarks>
    /// <para>
    /// RTAB advances the cursor to n characters from the end of the string.
    /// Can only move forward (toward the end).
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not numeric: Logs error 181
    /// - If argument is negative or > int.MaxValue: Logs error 182, uses 0
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'filename.txt'
    /// subject rtab(4) rem . ext  // ext = ".txt"
    /// </code>
    /// </example>
    public void CreateRTabPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 181, 182);
        SystemStack.Push(new PatternVar(new RTabPattern(i)));
    }

    #endregion

    #region Create SPAN Pattern (STRING)

    /// <summary>
    /// Creates a SPAN pattern that matches one or more consecutive characters from a set.
    /// In SNOBOL4: SPAN(string)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the character set (string)
    /// </param>
    /// <remarks>
    /// <para>
    /// SPAN matches a sequence of one or more characters, all from the specified set.
    /// Fails if the first character is not in the set. Greedy - matches as many as possible.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not a string: Logs error 188 (argument must be non-null string)
    /// - If argument is empty string: Logs error 188
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// digits = span('0123456789')
    /// subject = '12345abc'
    /// subject digits . num    // num = "12345"
    /// </code>
    /// </example>
    public void CreateSpanPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new SpanPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(188);
            return;
        }

        SystemStack.Push(new PatternVar(new SpanPattern((string)s)));

    }

    #endregion

    #region Create TAB Pattern (INTEGER)

    /// <summary>
    /// Creates a TAB pattern that advances the cursor to a specific position.
    /// In SNOBOL4: TAB(n)
    /// </summary>
    /// <param name="arguments">
    /// Single-element list containing the target position (integer)
    /// </param>
    /// <remarks>
    /// <para>
    /// TAB advances the cursor to position n (0-based). Can only move forward.
    /// Fails if cursor is already past position n.
    /// </para>
    /// <para>
    /// Error handling:
    /// - If argument is not numeric: Logs error 183
    /// - If argument is negative or > int.MaxValue: Logs error 184, uses 0
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // SNOBOL4 usage
    /// subject = 'test data'
    /// subject tab(5) rem . rest  // rest = "data"
    /// </code>
    /// </example>
    public void CreateTabPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 183, 184);
        SystemStack.Push(new PatternVar(new TabPattern(i)));
    }

    #endregion
}