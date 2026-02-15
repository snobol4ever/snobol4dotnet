// ********************************
// **** MACHINE GENERATED CODE ****
// **** DO NOT EDIT            ****
// **** 7/16/2025 11:09:25 PM   ****
// ********************************

using System;
using System.Collections;
using System.Collections.Generic;
using Snobol4.Common;

namespace NTest_CODE0;

public class CTest_CODE0
{
    delegate int StatementCode(Executive x);

    public int Run(Executive x)
    {
        Executive.BreakPoint();

        x.Parent.CaseFolding = true;
        x.Parent.DisplayListingHeader = false;
        x.Parent.ErrorsToStdout = false;
        x.Parent.FilesToCompile.Add(@"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net.Net\TestSnobol4\Test.sno");
        x.Parent.ListFileName = @"";
        x.Parent.Listing = false;
        x.Parent.ShowExecutionStatistics = false;
        ConsoleExt.SetStdError(x.Parent.Listing, x.Parent.ListFileName, x.Parent.FilesToCompile[^1]);

        x.SourceCode.Add("CODE1(1)\nL  a = a ' ' N");
        x.SourceCode.Add("CODE1(1)\n N = lt(N,10) N + 1 :S(L)F(DONE)");

        x.SourceFiles.Add(@"CODE1");
        x.SourceFiles.Add(@"CODE1");

        x.SourceLineNumbers.Add(1);
        x.SourceLineNumbers.Add(1);

        x.SourceStatementNumbers.Add(6);
        x.SourceStatementNumbers.Add(7);

        x.Statements.Add(Statement0000005);
        x.Statements.Add(Statement0000006);

        x.LabelTable.Add("L", 5);

        x.PreviousStarFunctionCount = x.StarFunctionList.Count;


        return 0;
    }

    private int Statement0000005(Executive x)
    {
        // CODE1(1): L  a = a ' ' N
        x.InitializeStatement(6);
        x.Identifier("a");
        x.Identifier("a");
        x.Constant(" ");
        x.Identifier("N");
        x.Operator("concat", 2);
        x.Operator("concat", 2);
        x._BinaryEquals();
        x.FinalizeStatement();
        return 6;
    }

    private int Statement0000006(Executive x)
    {
        // CODE1(1):  N = lt(N,10) N + 1 :S(L)F(DONE)
        x.InitializeStatement(7);
        x.Identifier("N");
        x.FunctionName("lt");
        x.Identifier("N");
        x.Constant(10);
        x.Function(2);
        x.Identifier("N");
        x.Constant(1);
        x.Operator("binary+", 2);
        x.Operator("concat", 2);
        x._BinaryEquals();
        x.FinalizeStatement();
        if (!x.Failure)
        {
        x.Identifier("L");
            if (x.Failure)
                x.LogRuntimeException(20);
        if (x.LabelTable.ContainsKey(x.SystemStack.Peek().Symbol))
            return x.LabelTable[x.SystemStack.Pop().Symbol];
        x.LogRuntimeException(23);
            return -1;
        }
        x.Failure = false;
        x.Identifier("DONE");
        if (x.Failure)
            x.LogRuntimeException(20);
        x.Failure = true;
        if (x.LabelTable.ContainsKey(x.SystemStack.Peek().Symbol))
            return x.LabelTable[x.SystemStack.Pop().Symbol];
        x.LogRuntimeException(23);
        return -1;
    }
}
