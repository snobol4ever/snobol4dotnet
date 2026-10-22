namespace Snobol4.Common;

/// <summary>
/// The errors SPITBOL finds while it generates the code of a statement, which are errors in the statement itself: it is reported on the
/// listing page with no column, it compiles to nothing, and the program does not run.
/// <para>
/// An arithmetic operator, or a call of a function marked for pre-evaluation (svpre in its sources), whose operands are all constants
/// is evaluated at once, and an error in it (1 / 0, LEN(-1), 'a' + 1) is an error in the statement. A value where a name is required
/// (cdgnm: a literal or an arithmetic result as the target of =, the right operand of . and $, the operand of unary . @ and &amp;) is
/// error 212, "value used where name is required".
/// </para>
/// This walks the postfix form of each expression of a statement with a stack of operands, each with its constant value when it has
/// one and the kind of thing SPITBOL's cdgnm would make of it, and calls the very builtins the run time calls. Only the first error
/// of a statement is reported, as SPITBOL stops compiling the statement at it.
/// </summary>
internal sealed class StatementChecker(Builder parent)
{
    // The functions SPITBOL pre-evaluates (those its sources mark svfnp), by the lower-case symbol of their entry
    private static readonly HashSet<string> Functions =
    [
        "ln", "any", "cos", "exp", "len", "pos", "sin", "tab", "tan", "atan", "char", "chop", "lpad", "rpad", "rpos", "rtab", "size",
        "span", "sqrt", "arbno", "break", "fence", "remdr", "breakx", "notany", "replace", "reverse", "datatype"
    ];

    // What cdgnm makes of an operand. Constant: a literal. Value: an arithmetic result or a name (. or @) taken as a value, which
    // is no name. Name: a variable, a reference to an array or table, a call, a deferred expression, an indirection or a keyword.
    // Question: a pattern match, whose subject is what a replacement assigns to. Other: anything else, taken as a name here
    // (a concatenation or alternation may be the pattern of a statement, and parentheses are lost in the postfix form).
    // The two marks hold the place of a function name or an array until its arguments or subscripts are in.
    // Combination: a concatenation, an alternation or a choice, which is a pattern or a value as the statement needs it, but no name
    private enum Kind { Constant, Value, Name, Question, Other, Combination, FunctionMark, ArrayMark }

    // Value is the operand's constant value when it has one; Subject is a match's subject
    // A combination is Nested when it stands inside parens or the arguments of a call (SPITBOL's c_cnp, no pattern match), and
    // PatternMatch when it is a concatenation at the statement's own level, or an alternation of one (c_cnc, and c_alt over it), which an
    // = takes for a subject and a pattern
    // Fixed when it is a name that is no computation: a variable, a keyword or a deferred expression, into which a capture of a
    // constant is a constant pattern (an element, an indirection or a call is found as it runs)
    private sealed record Operand(Kind Kind, Var? Value = null, Operand? Subject = null, string Name = "", bool Nested = false,
        bool PatternMatch = false, bool Fixed = false);

    private static readonly Operand ArrayMark = new(Kind.ArrayMark);

    private readonly Parser _parser = new(parent);

    // The number of the first error found in the statement, or 0 for none: its body, then its gotos
    internal int FirstError(SourceLine line)
    {
        if (parent.Execute is null)
            return 0;
        if (FirstError(line.LexBody, false) is var bodyCode and not 0)
            return bodyCode;

        // a goto in angle brackets takes a value (a code), whose kind is found when it is taken; the others take a name
        var both = line.LexSuccessGoto.Count > 0 && line.LexFailureGoto.Count > 0;
        var successDirect = both ? line.SuccessFirst ? line.DirectGotoFirst : line.DirectGotoSecond : line.DirectGotoFirst;
        var failureDirect = both ? line.SuccessFirst ? line.DirectGotoSecond : line.DirectGotoFirst : line.DirectGotoFirst;
        foreach (var (tokens, direct, empty) in new[]
                 {
                     (line.LexSuccessGoto, successDirect, line.EmptySuccessGoto), (line.LexFailureGoto, failureDirect, line.EmptyFailureGoto),
                     (line.LexUnconditionalGoto, line.DirectGotoFirst, line.EmptyUnconditionalGoto)
                 })
        {
            // a goto with nothing in it is a null, which is no name
            if (empty && !direct)
                return ValueWhereNameIsRequired;
            if (FirstError(tokens, !direct) is var code and not 0)
                return code;
        }

        return 0;
    }

    // `gotoTarget` is the expression of a goto, which SPITBOL generates by name: a constant, an arithmetic result or a combination of
    // operands (concatenation, alternation, a choice) is no name, so it is 212 there as it is for a target of =
    private int FirstError(List<Token> lexTokens, bool gotoTarget)
    {
        if (!gotoTarget && !HasCandidate(lexTokens))
            return 0;
        var postfix = _parser.ToPostfix(lexTokens);
        return postfix is null ? 0 : Walk(postfix, gotoTarget, out _);
    }

    // The same for a deferred expression's list, with its value when it is a constant: *X of a constant is a constant too
    private int FirstError(List<Token> lexTokens, out Var? value)
    {
        value = null;
        var postfix = _parser.ToPostfix(lexTokens);
        return postfix is null ? 0 : Walk(postfix, false, out value);
    }

    // Whether the list holds an operator or call that could err: one that is pre-evaluated, one that takes a name, or a deferred
    // expression, which could hold either
    private bool HasCandidate(List<Token> lexTokens) => lexTokens.Any(token => token.TokenType switch
    {
        Token.Type.BINARY_PLUS or Token.Type.BINARY_MINUS or Token.Type.BINARY_STAR or Token.Type.BINARY_SLASH
            or Token.Type.BINARY_CARET or Token.Type.BINARY_EQUAL or Token.Type.BINARY_PERIOD or Token.Type.BINARY_DOLLAR
            or Token.Type.EXPRESSION => true,
        Token.Type.UNARY_OPERATOR => token.MatchedString is "-" or "+" or "." or "@" or "&",
        Token.Type.IDENTIFIER_FUNCTION => FunctionEntry(token.MatchedString) is not null,
        _ => false
    });

    private FunctionTableEntry? FunctionEntry(string name)
    {
        var entry = parent.Execute!.FunctionTable[parent.FoldCase(name)];
        return entry is not null && Functions.Contains(entry.Symbol) ? entry : null;
    }

    private const int ValueWhereNameIsRequired = 212;

    // cdgnm accepts a variable, an array or table reference, a call, a deferred expression, an indirection and a keyword; a literal
    // or a value that is no name is error 212
    private static bool IsValue(Operand operand) => operand.Kind is Kind.Constant or Kind.Value;

    private int Walk(List<Token> postfix, bool gotoTarget, out Var? result)
    {
        result = null;
        List<Operand> stack = [];
        foreach (var token in postfix)
        {
            switch (token.TokenType)
            {
                case Token.Type.INTEGER:
                    stack.Add(new Operand(Kind.Constant, new IntegerVar(token.IntegerValue)));
                    break;
                case Token.Type.REAL:
                    stack.Add(new Operand(Kind.Constant, new RealVar(token.DoubleValue)));
                    break;
                case Token.Type.STRING:
                    stack.Add(new Operand(Kind.Constant, new StringVar(token.MatchedString)));
                    break;
                case Token.Type.NULL:
                    stack.Add(new Operand(Kind.Other, StringVar.Null()));
                    break;
                // a variable is a name; one of the predefined pattern variables is a constant too (Executive.ConstantVariable)
                case Token.Type.IDENTIFIER:
                    stack.Add(new Operand(Kind.Name, parent.Execute!.ConstantVariable(parent.FoldCase(token.MatchedString)), Name: token.MatchedString,
                        Fixed: true));
                    break;

                case Token.Type.EXPRESSION:
                {
                    // a deferred expression is compiled where it stands, and its statements are checked then. It is a constant, an
                    // expression, whatever it holds, as SPITBOL's compiler takes it: *X + 1 is 1 and SIZE(*X) 189 as it is compiled
                    var index = int.Parse(token.MatchedString[4..]);
                    Var? deferred = null;
                    if (index < parent.ExpressionList.Count && FirstError(parent.ExpressionList[index].ExpressionList, out deferred) is var inner and not 0)
                        return inner;
                    stack.Add(new Operand(Kind.Name, Deferred(deferred), Fixed: true));
                    break;
                }

                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                    stack.Add(ArrayMark);
                    break;
                case Token.Type.R_ANGLE:
                case Token.Type.R_SQUARE:
                    // the subscripts and the array under them become one name
                    while (stack.Count > 0 && !ReferenceEquals(stack[^1], ArrayMark))
                        stack.RemoveAt(stack.Count - 1);
                    if (stack.Count == 0)
                        return 0;
                    stack[^1] = new Operand(Kind.Name);
                    break;

                case Token.Type.IDENTIFIER_FUNCTION:
                    stack.Add(new Operand(Kind.FunctionMark, Name: token.MatchedString));
                    break;
                case Token.Type.R_PAREN_FUNCTION:
                {
                    var count = (int)token.IntegerValue;
                    if (stack.Count < count + 1 || stack.Skip(stack.Count - count).Any(item => item.Kind is Kind.FunctionMark or Kind.ArrayMark)
                        || stack[stack.Count - count - 1].Kind != Kind.FunctionMark)
                        return 0;
                    var arguments = stack.GetRange(stack.Count - count, count);
                    var mark = stack[stack.Count - count - 1];
                    stack.RemoveRange(stack.Count - count - 1, count + 1);
                    // a call is a name wherever a name is wanted, and its value when its arguments are all constants
                    var entry = FunctionEntry(mark.Name);
                    if (entry is null || arguments.Any(argument => argument.Value is null))
                    {
                        stack.Add(new Operand(Kind.Name));
                        break;
                    }

                    // SPITBOL gives a function the number of arguments it declares: missing ones are null, extra ones are dropped
                    var list = arguments.Select(argument => argument.Value!).Take(entry.ArgumentCount).ToList();
                    while (list.Count < entry.ArgumentCount)
                        list.Add(StringVar.Null());
                    if (Evaluate(entry, list, out var value) is var code and not 0)
                        return code;
                    stack.Add(new Operand(Kind.Name, value));
                    break;
                }

                // a choice (A,B) is not a constant; each alternative before the last leaves nothing
                case Token.Type.COMMA_CHOICE:
                    if (stack.Count == 0)
                        return 0;
                    stack.RemoveAt(stack.Count - 1);
                    break;
                case Token.Type.R_PAREN_CHOICE:
                    if (stack.Count == 0)
                        return 0;
                    stack[^1] = new Operand(Kind.Combination, Nested: true);
                    break;

                case Token.Type.BINARY_PLUS or Token.Type.BINARY_MINUS or Token.Type.BINARY_STAR or Token.Type.BINARY_SLASH
                    or Token.Type.BINARY_CARET:
                {
                    if (!TakeTwo(stack, out var left, out var right))
                        return 0;
                    var key = token.TokenType switch
                    {
                        Token.Type.BINARY_PLUS => "__+",
                        Token.Type.BINARY_MINUS => "__-",
                        Token.Type.BINARY_STAR => "__*",
                        Token.Type.BINARY_SLASH => "__/",
                        _ => "__^"
                    };
                    var entry = left.Value is null || right.Value is null ? null : parent.Execute!.FunctionTable[key];
                    if (entry is null)
                    {
                        stack.Add(new Operand(Kind.Value));
                        break;
                    }

                    if (Evaluate(entry, [left.Value!, right.Value!], out var value) is var code and not 0)
                        return code;
                    stack.Add(new Operand(Kind.Value, value));
                    break;
                }

                case Token.Type.UNARY_OPERATOR:
                {
                    if (stack.Count < 1 || stack[^1].Kind is Kind.FunctionMark or Kind.ArrayMark)
                        return 0;
                    var operand = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    switch (token.MatchedString)
                    {
                        case "-" or "+":
                        {
                            var entry = operand.Value is null ? null : parent.Execute!.FunctionTable[token.MatchedString == "-" ? "_-" : "_+"];
                            if (entry is null)
                            {
                                stack.Add(new Operand(Kind.Value));
                                break;
                            }

                            if (Evaluate(entry, [operand.Value!], out var value) is var code and not 0)
                                return code;
                            stack.Add(new Operand(Kind.Value, value));
                            break;
                        }

                        // the name operators take a name: a literal, a value or a combination of operands (which only parens
                        // can bring there) is error 212. A keyword is a name, and a constant too when its value never changes
                        // (&ALPHABET, &FENCE: -&FENCE is 10 as the statement is compiled); the others (. and @) give a value
                        case "." or "@" or "&":
                            if (IsValue(operand) || operand.Kind == Kind.Combination)
                                return ValueWhereNameIsRequired;
                            stack.Add(token.MatchedString != "&" ? new Operand(Kind.Value)
                                : new Operand(Kind.Name, operand.Name == "" ? null : parent.Execute!.ConstantKeyword(parent.FoldCase(operand.Name)),
                                    Fixed: true));
                            break;

                        // negation and interrogation give a value, whatever the operand: no name
                        case "~" or "?":
                            stack.Add(new Operand(Kind.Value));
                            break;

                        // an indirection takes a value and is a name
                        case "$":
                            stack.Add(new Operand(Kind.Name));
                            break;

                        default:
                            stack.Add(new Operand(Kind.Other));
                            break;
                    }

                    break;
                }

                // *X is a constant, an expression, whatever X is (as for a deferred expression above)
                case Token.Type.UNARY_STAR:
                    if (stack.Count < 1)
                        return 0;
                    stack[^1] = new Operand(Kind.Other, Deferred(stack[^1].Value), Fixed: true);
                    break;

                // the right operand of . and $ is a name; the result is a value
                case Token.Type.BINARY_PERIOD or Token.Type.BINARY_DOLLAR:
                {
                    if (!TakeTwo(stack, out var left, out var right))
                        return 0;
                    if (IsValue(right) || right.Kind == Kind.Combination)
                        return ValueWhereNameIsRequired;
                    // a capture of a constant into a fixed name is a constant pattern, as SPITBOL's compiler builds it; which pattern
                    // does not matter to what is evaluated after it, so it stands for one
                    stack.Add(new Operand(Kind.Value, left.Value is not null && right.Fixed ? new PatternVar(new NullPattern()) : null));
                    break;
                }

                // a match gives a value, and keeps its subject: a replacement assigns to it
                case Token.Type.BINARY_QUESTION:
                {
                    if (!TakeTwo(stack, out var left, out _))
                        return 0;
                    stack.Add(new Operand(Kind.Question, Subject: left));
                    break;
                }

                // the target of an assignment is a name; for a replacement it is the subject of the match
                case Token.Type.BINARY_EQUAL:
                {
                    if (!TakeTwo(stack, out var left, out _))
                        return 0;
                    var target = left.Kind == Kind.Question ? left.Subject! : left;
                    // a concatenation at the statement's level is the subject and pattern of an =; inside parens or the arguments of a
                    // call it is SPITBOL's c_cnp, no pattern match, and an alternation that is not over a concatenation is none either
                    if (IsValue(target) || target.Kind == Kind.Combination && (target.Nested || !target.PatternMatch))
                        return ValueWhereNameIsRequired;
                    stack.Add(new Operand(Kind.Value));
                    break;
                }

                // every other operator takes two operands and gives a value that is neither a constant nor a name
                case Token.Type.BINARY_CONCAT or Token.Type.BINARY_PIPE:
                {
                    if (!TakeTwo(stack, out var left, out var right))
                        return 0;
                    var concatenation = token.TokenType == Token.Type.BINARY_CONCAT;
                    // a concatenation or an alternation of constants is a constant, as SPITBOL's compiler builds it: ('a' 'b') + 1 is 1
                    Var? joined = null;
                    if (left.Value is not null && right.Value is not null
                        && Evaluate(parent.Execute!.FunctionTable[concatenation ? "___" : "__|"]!, [left.Value, right.Value], out joined) is var code
                            and not 0)
                        return code;
                    stack.Add(new Operand(Kind.Combination, joined, Nested: token.Index > 0,
                        PatternMatch: concatenation || left.Kind == Kind.Combination && left.PatternMatch));
                    break;
                }

                case Token.Type.BINARY_AMPERSAND or Token.Type.BINARY_AT or Token.Type.BINARY_HASH or Token.Type.BINARY_PERCENT
                    or Token.Type.BINARY_TILDE:
                    if (!TakeTwo(stack, out _, out _))
                        return 0;
                    stack.Add(new Operand(Kind.Other));
                    break;
            }
        }

        result = stack.Count == 1 ? stack[0].Value : null;
        return gotoTarget && stack.Count == 1 && stack[0].Kind is Kind.Constant or Kind.Value or Kind.Combination ? ValueWhereNameIsRequired : 0;
    }

    // The two operands on top of the stack, taken off it; false when the postfix form is not what the walk expects
    private static bool TakeTwo(List<Operand> stack, out Operand left, out Operand right)
    {
        left = right = null!;
        if (stack.Count < 2 || stack[^1].Kind is Kind.FunctionMark or Kind.ArrayMark || stack[^2].Kind is Kind.FunctionMark or Kind.ArrayMark)
            return false;
        right = stack[^1];
        left = stack[^2];
        stack.RemoveRange(stack.Count - 2, 2);
        return true;
    }

    // *X as the compiler holds it: an expression giving X's value when that is a constant, and failing when it is not, so a pre-evaluated
    // function that evaluated it (none does) would be abandoned rather than run the program's code as it is compiled
    private static ExpressionVar Deferred(Var? value) =>
        new(value is null ? x => x.Failure = true : x => x.SystemStack.Push(value));

    // The value of the call, or the number of the error it raised
    private int Evaluate(FunctionTableEntry entry, List<Var> arguments, out Var? value)
    {
        value = parent.Execute!.PreEvaluate(entry, arguments, out var code);
        return code;
    }
}
