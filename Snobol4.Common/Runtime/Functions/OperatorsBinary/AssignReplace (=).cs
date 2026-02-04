using System.Text;

namespace Snobol4.Common;

//"pattern replacement right operand is not a string" /* 31 */,
//"keyword value assigned is not integer" /* 208 */,
//"keyword in assignment is protected" /* 209 */,
//"keyword value assigned is negative or too large" /* 210 */,
//"value assigned to keyword errtext not a string" /* 211 */,
//"syntax error: value used where name is required" /* 212 */,

public partial class Executive
{
    public void _BinaryEquals()
    {
        using var profile1 = Profiler.Start("_BinaryEquals", ProfileStatements);

        if (Parent.TraceStatements)
            Console.Error.WriteLine(@"_BinaryEquals");
        // Do not delete. Used by DLL

        // Get all arguments and check for prior failure
        List<Var> arguments = [];

        if (SystemStack.ExtractArguments(2, arguments, this, 1))
        {
            return;
        }

        if (arguments[0] is SubjectVar)
        {
            ReplaceMatch(arguments);
        }
        else
        {
            Assign(arguments);
        }
    }

    internal void ReplaceMatch(List<Var> arguments)
    {
        var subjectVar = (SubjectVar)arguments[0];

        // Replacement must resolve to a string
        if (!arguments[1].Convert(VarType.STRING, out _, out var subject, this))
        {
            LogRuntimeException(31);
            return;
        }

        // Perform the replacement
        var resultsVar = subjectVar.MatchReplace((string)subject);
        var v = IdentifierTable[resultsVar.Symbol] = resultsVar;
        SystemStack.Push(v);
    }

    internal void Assign(List<Var> arguments)
    {
        while (arguments[0] is ExpressionVar expressionVar1)
        {
            expressionVar1.FunctionName(this);
            arguments[0] = SystemStack.Pop();
        }

        var leftVar = arguments[0];  // Destination Var
        var rightVar = arguments[1]; // Source Var

        // If source has input channels, get input now
        rightVar = InputArgument(rightVar);

        //LeftPattern side must have a symbol, be keyed to a collection, or be a NameVar
        if (leftVar is { Symbol: "", Key: null } and not NameVar)
        {
            LogRuntimeException(leftVar.IsKeyword ? 211 : 212);
            return;
        }

        // Special checks for keywords
        if (leftVar.IsKeyword)
        {
            // LeftPattern side must be writable
            if (leftVar.IsReadOnly)
            {
                LogRuntimeException(209);
                return;
            }

            // If leftVar side is &errtext, rightVar side must be a string
            if (leftVar.Symbol == "&errtext" && leftVar is not StringVar)
            {
                LogRuntimeException(211);
                return;
            }

            // Otherwise, right side must be an integer
            if (rightVar is not IntegerVar)
            {
                LogRuntimeException(208);
                return;
            }
        }

        switch (leftVar.Collection)
        {
            case ArrayVar arrayVar:
                rightVar.Key = leftVar.Key;
                rightVar.Collection = leftVar.Collection;
                arrayVar.Data[(int)(long)leftVar.Key!] = rightVar;
                SystemStack.Push(rightVar);
                break;

            case TableVar tableVar:
                rightVar.Key = leftVar.Key;
                rightVar.Collection = leftVar.Collection;
                tableVar.Data[leftVar.Key!] = rightVar;
                SystemStack.Push(rightVar);
                break;

            default:
                var newVar = rightVar is ArrayVar or TableVar ? rightVar : rightVar.Clone();
                newVar.Symbol = leftVar.Symbol;
                //newVar.Collection = rightVar.Collection;
                newVar.OutputChannel = leftVar.OutputChannel;
                IdentifierTable[newVar.Symbol] = newVar;
                SystemStack.Push(newVar);
                break;
        }

        if (SystemStack.Peek().OutputChannel == "")
            return;

        // Call output function
        var outputVar = SystemStack.Peek();

        switch (outputVar.OutputChannel)
        {
            case "+console-output":
            case "+terminal-output":
                Console.Error.WriteLine(outputVar.ToString());
                break;

            default:
                StreamOutputs[outputVar.OutputChannel].Write(Encoding.UTF8.GetBytes(outputVar + Environment.NewLine));
                break;
        }
    }
}