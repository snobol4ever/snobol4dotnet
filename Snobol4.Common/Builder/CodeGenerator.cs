using System.Text;

namespace Snobol4.Common;

public class GenerateCSharpCode(Builder parent)
{
    private readonly StringBuilder _csharpCode = new();
    private Builder _parent = parent;
    private CompileTarget _compileTarget;

    // Constants for runtime exception codes
    //private const int GOTO_EVALUATION_FAILURE = 20;
    //private const int GOTO_NOT_NATURAL_VARIABLE = 23;
    //private const int GOTO_NOT_CODE = 24;

    // Code generation indentation
    //private const string INDENT = "        ";

    public enum CompileTarget
    {
        PROGRAM = 1,
        CODE = 2,
        EVAL = 3
    }

    #region Public API

    public string GenerateCSharp(string nameSpace, string className, bool firstInit, CompileTarget compileTarget, Builder parent)
    {
        _compileTarget = compileTarget;
        _parent = parent;

        GeneratePre(nameSpace, className, firstInit);
        // In threaded mode, statement C# methods are not used (ThreadedExecuteLoop
        // dispatches via Instruction[] instead). Skip generation to avoid Roslyn
        // errors from missing Star method references.
        if (!_parent.BuildOptions.UseThreadedExecution)
        {
            GenerateStatements();
            GenerateExpressions();
        }

        _csharpCode.AppendLine("}");
        MarkCodeAsCompiled();

        if (_parent.BuildOptions.WriteCSharpCode)
            WriteCode(className + "_" + nameSpace + ".cs");

        return _csharpCode.ToString();
    }

    #endregion

    #region Pre-Generation

    private void GeneratePre(string nameSpace, string className, bool firstInit)
    {
        GenerateFileHeader();
        GenerateNamespaceAndClass(nameSpace, className);
        GenerateRunMethod(firstInit);
    }

    private void GenerateFileHeader()
    {
        _csharpCode.AppendLine("// ********************************");
        _csharpCode.AppendLine("// **** MACHINE GENERATED CODE ****");
        _csharpCode.AppendLine("// **** DO NOT EDIT            ****");
        _csharpCode.AppendLine($"// **** {DateTime.Now}   ****");
        _csharpCode.AppendLine("// ********************************");
        _csharpCode.AppendLine();
    }

    private void GenerateNamespaceAndClass(string nameSpace, string className)
    {
        _csharpCode.AppendLine("using System;");
        _csharpCode.AppendLine("using System.Collections;");
        _csharpCode.AppendLine("using System.Collections.Generic;");
        _csharpCode.AppendLine("using Snobol4.Common;");
        _csharpCode.AppendLine();
        _csharpCode.AppendLine($"namespace {nameSpace};");
        _csharpCode.AppendLine();
        _csharpCode.AppendLine($"public class {className}");
        _csharpCode.AppendLine("{");
        _csharpCode.AppendLine("    delegate int StatementCode(Executive x);");
        _csharpCode.AppendLine();
    }

    private void GenerateRunMethod(bool firstInit)
    {
        _csharpCode.AppendLine("    public int Run(Executive x)");
        _csharpCode.AppendLine("    {");

        if (_compileTarget == CompileTarget.EVAL)
        {
            GenerateEvalMode();
            return;
        }

        GenerateBreakpoint();
        GenerateInitialSettings();
        GenerateSourceCodeData();
        GenerateKeywordData();
        // In threaded mode, Statements[] and StarFunctionList are populated by
        // the threaded compiler — skip Roslyn references to missing methods.
        if (!_parent.BuildOptions.UseThreadedExecution)
        {
            GenerateStatementMappings();
            GenerateStarFunctions();
        }
        GenerateLabelMappings();

        if (firstInit)
        {
            GenerateSystemLabels();
            // In threaded mode, BuildMain calls ExecuteLoop(0) directly after
            // setting up Thread — don't call it again from Roslyn's Run().
            if (!_parent.BuildOptions.UseThreadedExecution)
                _csharpCode.AppendLine("        x.ExecuteLoop(0);");
        }

        _csharpCode.AppendLine();
        _csharpCode.AppendLine("        return 0;");
        _csharpCode.AppendLine("    }");
    }

    private void GenerateEvalMode()
    {
        if (!_parent.BuildOptions.UseThreadedExecution)
        {
            var startIndex = _parent.Execute?.PreviousStarFunctionCount ?? 0;
            for (var jStar = startIndex; jStar < _parent.ExpressionList.Count; ++jStar)
            {
                _csharpCode.AppendLine($"        // jStar: {jStar}");
                _csharpCode.AppendLine($"        x.StarFunctionList.Add(Star{jStar:D8});");
            }
        }

        _csharpCode.AppendLine();
        _csharpCode.AppendLine("        x.PreviousStarFunctionCount = x.StarFunctionList.Count;");
        _csharpCode.AppendLine("        return 0;");
        _csharpCode.AppendLine("    }");
    }

    private void GenerateBreakpoint()
    {
        _csharpCode.AppendLine("        Executive.BreakPoint();");
        _csharpCode.AppendLine();
    }

    private void GenerateInitialSettings()
    {
        _csharpCode.AppendLine($"        x.Parent.BuildOptions.SuppressListingHeader = {_parent.BuildOptions.SuppressListingHeader.ToString().ToLower()};");
        _csharpCode.AppendLine($"        x.Parent.FilesToCompile.Add(@\"{_parent.FilesToCompile[^1]}\");");
        _csharpCode.AppendLine($"        x.Parent.BuildOptions.ListFileName = @\"{_parent.BuildOptions.ListFileName}\";");
        _csharpCode.AppendLine($"        x.Parent.BuildOptions.ShowExecutionStatistics = {_parent.BuildOptions.ShowExecutionStatistics.ToString().ToLower()};");
        _csharpCode.AppendLine();
    }

    private void GenerateSourceCodeData()
    {
        // Make source code file name available for debugging
        foreach (var line in _parent.Code.SourceLines)
        {
            if (line.Compiled)
                continue;

            var escapedLine = line.Text.Replace('\t', ' ').Replace("\"", "\"\"");
            var codeCount = 1 + line.LineCountFile - line.BlankLineCount - line.CommentContinuationDirectiveCount;
            var listCount = 1 + line.LineCountFile - line.CommentContinuationDirectiveCount;
            var lineCount = 1 + line.LineCountFile;
            var pathLine = $"{Path.GetFileName(line.PathName)}:{codeCount}/{listCount}/{lineCount})\\n";
            _csharpCode.AppendLine($"        x.SourceCode.Add(\"{pathLine}\" + @\"{escapedLine}\");");
        }

        _csharpCode.AppendLine();
    }

    private void GenerateKeywordData()
    {
        // Generate &FILE keyword data
        foreach (var pathLine in from line in _parent.Code.SourceLines
                                 where !line.Compiled
                                 select line.PathName)
        {
            _csharpCode.AppendLine($"        x.SourceFiles.Add(@\"{pathLine}\");");
        }
        _csharpCode.AppendLine();

        // Generate &LINE keyword data
        foreach (var line in _parent.Code.SourceLines.Where(line => !line.Compiled))
        {
            _csharpCode.AppendLine($"        x.SourceLineNumbers.Add({line.LineCountFile});");
        }
        _csharpCode.AppendLine();

        // Generate &STNO keyword data
        var statementNumber = _parent.StatementCount;
        foreach (var _ in _parent.Code.SourceLines)
        {
            _csharpCode.AppendLine($"        x.SourceStatementNumbers.Add({++statementNumber});");
        }
        _csharpCode.AppendLine();
    }

    private void GenerateStatementMappings()
    {
        // Map methods to line numbers
        var statementNumber = _parent.StatementCount;
        foreach (var _ in _parent.Code.SourceLines)
        {
            _csharpCode.AppendLine($"        x.Statements.Add(Statement{statementNumber++:D7});");
        }
        _csharpCode.AppendLine();
    }

    private void GenerateLabelMappings()
    {
        // Map labels to line numbers
        var statementNumber = _parent.StatementCount;
        foreach (var line in _parent.Code.SourceLines)
        {
            if (line.Label.Length > 0 && !line.Compiled && ((_parent.BuildOptions.CaseFolding && line.Label.ToUpper() is not "END") || (!_parent.BuildOptions.CaseFolding && line.Label is not "end")))
            {
                _csharpCode.AppendLine($"        x.LabelTable[\"{line.Label}\"] = {statementNumber};");
            }
            statementNumber++;
        }
        _csharpCode.AppendLine();
    }

    private void GenerateStarFunctions()
    {
        for (var iStar = _parent.RecordedExpressionCount; iStar < _parent.ExpressionList.Count; ++iStar)
        {
            _csharpCode.AppendLine($"        x.StarFunctionList.Add(Star{iStar:D8});");
        }

        _csharpCode.AppendLine();
        _csharpCode.AppendLine("        x.PreviousStarFunctionCount = x.StarFunctionList.Count;");
        _csharpCode.AppendLine();
    }

    private void GenerateSystemLabels()
    {
        // TODO: Implement return, sreturn, and freturn
        _csharpCode.AppendLine("        x.LabelTable.Add(\"end\",-1);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"return\", -2);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"freturn\", -3);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"nreturn\", -4);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"abort\", -5);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"continue\", -6);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"scontinue\", -7);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"END\",-1);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"RETURN\", -2);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"FRETURN\", -3);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"NRETURN\", -4);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"ABORT\", -5);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"CONTINUE\", -6);");
        _csharpCode.AppendLine("        x.LabelTable.Add(\"SCONTINUE\", -7);");
        _csharpCode.AppendLine();
    }

    #endregion

    #region Statement Generation

    private void GenerateStatements()
    {
        if (_compileTarget == CompileTarget.EVAL)
            return;

        var statementNumber = _parent.StatementCount;

        foreach (var line in _parent.Code.SourceLines)
        {
            if (line.Compiled)
            {
                statementNumber++;
                continue;
            }

            GenerateSingleStatement(line, statementNumber);
            statementNumber++;
        }
    }

    private void GenerateSingleStatement(SourceLine line, int statementNumber)
    {
        var methodName = $"Statement{statementNumber:D7}";
        _csharpCode.AppendLine();
        _csharpCode.AppendLine($"    private int {methodName}(Executive x)");
        _csharpCode.AppendLine("    {");
        GenerateStatementComment(line);
        _csharpCode.AppendLine($"        x.InitializeStatement({statementNumber});");

        if ((_parent.BuildOptions.CaseFolding && line.Label.ToUpper() is "END") || (!_parent.BuildOptions.CaseFolding && line.Label is "end"))
        {
            _csharpCode.AppendLine("        return -1;");
            _csharpCode.AppendLine("    }");
            return;
        }

        GenerateStatementBody(line);
        _csharpCode.AppendLine("        x.FinalizeStatement();");
        _csharpCode.AppendLine("        if (x.ErrorJump > 0) x.ProcessTrappedError();");
        GenerateStatementGotos(line, statementNumber + 1);
        _csharpCode.AppendLine("    }");
    }

    private void GenerateStatementComment(SourceLine line)
    {
        var pathLine = $"{Path.GetFileName(line.PathName)}({line.LineCountTotal}): ";
        var escapedLine = line.Text.Replace('\t', ' ').Replace("\"", "\\\"");
        _csharpCode.AppendLine($"        // {pathLine}{escapedLine}");
    }

    private void GenerateStatementBody(SourceLine line)
    {
        if (line.ParseBody.Count > 0)
        {
            _csharpCode.Append(ToCSharp(line.ParseBody));
        }
    }

    private void GenerateStatementGotos(SourceLine line, int nextStatement)
    {
        // Unconditional goto
        if (line.ParseUnconditionalGoto.Count > 0)
        {
            GenerateUnconditionalGoto(line);
            return;
        }

        // No gotos - fall through to next statement
        if (line.ParseFailureGoto.Count == 0 && line.ParseSuccessGoto.Count == 0)
        {
            _csharpCode.AppendLine($"        return {nextStatement};");
            return;
        }

        // Both success and failure gotos
        if (line.ParseSuccessGoto.Count > 0 && line.ParseFailureGoto.Count > 0)
        {
            GenerateBothGotos(line);
            return;
        }

        // Success goto only
        if (line.ParseSuccessGoto.Count > 0)
        {
            GenerateSuccessGoto(line, nextStatement);
            return;
        }

        // Failure goto only
        if (line.ParseFailureGoto.Count > 0)
        {
            GenerateFailureGoto(line, nextStatement);
        }
    }

    private void GenerateUnconditionalGoto(SourceLine line)
    {
        _csharpCode.AppendLine("        // Process unconditional goto");
        _csharpCode.AppendLine("        bool bSaveStatus = x.Failure;");
        _csharpCode.AppendLine("        x.Failure = false;");
        _csharpCode.Append(ToCSharp(line.ParseUnconditionalGoto));
        _csharpCode.AppendLine("        if (x.Failure)");
        _csharpCode.AppendLine($"            x.LogRuntimeException({20});");
        _csharpCode.AppendLine("        x.SaveStatus(bSaveStatus);");

        if (line.DirectGotoFirst)
            DirectGoto();
        else
            LabelGoto();

        _csharpCode.AppendLine("        return -1;");
    }

    private void GenerateBothGotos(SourceLine line)
    {
        if (line.SuccessFirst)
        {
            // Success goto first, then failure
            _csharpCode.AppendLine("        if (!x.Failure)");
            _csharpCode.AppendLine("        {");
            _csharpCode.Append(ToCSharp(line.ParseSuccessGoto));
            _csharpCode.AppendLine("            if (x.Failure)");
            _csharpCode.AppendLine($"                x.LogRuntimeException({20});");

            if (line.DirectGotoFirst)
                DirectGoto();
            else
                LabelGoto();

            _csharpCode.AppendLine("            return -1;");
            _csharpCode.AppendLine("        }");
            _csharpCode.AppendLine("        x.Failure = false;");
            _csharpCode.Append(ToCSharp(line.ParseFailureGoto));
            _csharpCode.AppendLine("        if (x.Failure)");
            _csharpCode.AppendLine($"            x.LogRuntimeException({20});");
            _csharpCode.AppendLine("        x.Failure = true;");
        }
        else
        {
            // Failure goto first, then success
            _csharpCode.AppendLine("        if (x.Failure)");
            _csharpCode.AppendLine("        {");
            _csharpCode.AppendLine("        x.Failure = false;");
            _csharpCode.Append(ToCSharp(line.ParseFailureGoto));
            _csharpCode.AppendLine("            if (x.Failure)");
            _csharpCode.AppendLine($"                x.LogRuntimeException({20});");
            _csharpCode.AppendLine("        x.Failure = true;");

            if (line.DirectGotoFirst)
                DirectGoto();
            else
                LabelGoto();

            _csharpCode.AppendLine("            return -1;");
            _csharpCode.AppendLine("        }");
            _csharpCode.Append(ToCSharp(line.ParseSuccessGoto));
            _csharpCode.AppendLine("        if (x.Failure)");
            _csharpCode.AppendLine($"            x.LogRuntimeException({20});");
        }

        if (line.DirectGotoSecond)
            DirectGoto();
        else
            LabelGoto();

        _csharpCode.AppendLine("        return -1;");
    }

    private void GenerateSuccessGoto(SourceLine line, int nextStatement)
    {
        _csharpCode.AppendLine("        if (x.Failure)");
        _csharpCode.AppendLine($"            return {nextStatement};");
        _csharpCode.Append(ToCSharp(line.ParseSuccessGoto));
        _csharpCode.AppendLine("        if (x.Failure)");
        _csharpCode.AppendLine($"            x.LogRuntimeException({20});");

        if (line.DirectGotoFirst)
            DirectGoto();
        else
            LabelGoto();

        _csharpCode.AppendLine("        return -1;");
    }

    private void GenerateFailureGoto(SourceLine line, int nextStatement)
    {
        _csharpCode.AppendLine("        if (!x.Failure)");
        _csharpCode.AppendLine($"            return {nextStatement};");
        _csharpCode.AppendLine("        x.Failure = false;");
        _csharpCode.Append(ToCSharp(line.ParseFailureGoto));
        _csharpCode.AppendLine("        if (x.Failure)");
        _csharpCode.AppendLine($"            x.LogRuntimeException({20});");
        _csharpCode.AppendLine("        x.Failure = true;");

        if (line.DirectGotoFirst)
            DirectGoto();
        else
            LabelGoto();

        _csharpCode.AppendLine("        return -1;");
    }

    #endregion

    #region Token to CSharp Conversion

    private static StringBuilder ToCSharp(List<Token> tokenList)
    {
        StringBuilder code = new();

        foreach (var t in tokenList)
        {
            switch (t.TokenType)
            {
                case Token.Type.BINARY_AMPERSAND:
                    code.AppendLine("        x.Operator(\"__&\", 2);");
                    break;

                case Token.Type.BINARY_AT:
                    code.AppendLine("        x.Operator(\"__@\", 2);");
                    break;

                case Token.Type.BINARY_CARET:
                    code.AppendLine("        x.Operator(\"__^\", 2);");
                    break;

                case Token.Type.BINARY_CONCAT:
                    code.AppendLine("        x.Operator(\"___\", 2);");
                    break;

                case Token.Type.BINARY_DOLLAR:
                    code.AppendLine("        x.Operator(\"__$\", 2);");
                    break;

                case Token.Type.BINARY_EQUAL:
                    code.AppendLine("        x._BinaryEquals();");
                    break;

                case Token.Type.BINARY_MINUS:
                    code.AppendLine("        x.Operator(\"__-\", 2);");
                    break;

                case Token.Type.BINARY_PERCENT:
                    code.AppendLine("        x.Operator(\"__%\", 2);");
                    break;

                case Token.Type.BINARY_PERIOD:
                    code.AppendLine("        x.Operator(\"__.\", 2);");
                    break;

                case Token.Type.BINARY_PIPE:
                    code.AppendLine("        x.Operator(\"__|\", 2);");
                    break;

                case Token.Type.BINARY_PLUS:
                    code.AppendLine("        x.Operator(\"__+\", 2);");
                    break;

                case Token.Type.BINARY_QUESTION:
                    code.AppendLine("        x.Operator(\"__?\", 2);");
                    break;

                case Token.Type.BINARY_SLASH:
                    code.AppendLine("        x.Operator(\"__/\", 2);");
                    break;

                case Token.Type.BINARY_STAR:
                    code.AppendLine("        x.Operator(\"__*\", 2);");
                    break;

                case Token.Type.COMMA_CHOICE:
                    code.AppendLine("        if (x.Failure)");
                    code.AppendLine("        {");
                    code.AppendLine("        x.SystemStack.Pop();");
                    code.AppendLine("        x.Failure = false;");
                    break;

                case Token.Type.IDENTIFIER:
                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                    code.AppendLine($"        x.Identifier(\"{t.MatchedString}\");");
                    break;

                case Token.Type.IDENTIFIER_FUNCTION:
                    code.AppendLine($"        x.FunctionName(\"{t.MatchedString}\");");
                    break;

                case Token.Type.INTEGER:
                    code.AppendLine($"        x.Constant({t.MatchedString});");
                    break;

                case Token.Type.NULL:
                    code.AppendLine("        x.Constant(\"\");");
                    break;

                case Token.Type.R_ANGLE:
                    code.AppendLine("        x.IndexCollection();");
                    break;

                case Token.Type.R_PAREN_CHOICE:
                    for (var i = 1; i < t.IntegerValue; ++i)
                        code.AppendLine("        }");
                    break;

                case Token.Type.R_PAREN_FUNCTION:
                    code.AppendLine($"        x.Function({t.IntegerValue});");
                    break;

                case Token.Type.R_SQUARE:
                    code.AppendLine("        x.IndexCollection();");
                    break;

                case Token.Type.REAL:
                    // c# does not consider a trailing decimal point to be a valid real literal,
                    // but snobol4 does, so add a zero if there is a trailing decimal point
                    var matchedString = t.MatchedString;
                    if (matchedString[^1] == '.')
                        matchedString += "0";
                    code.AppendLine($"        x.Constant({matchedString});");
                    break;

                case Token.Type.STRING:
                    var s = t.MatchedString.Replace("\"", "\"\"");
                    code.AppendLine($"""        x.Constant(@"{s}");""");
                    break;

                case Token.Type.UNARY_OPERATOR:
                    switch (t.MatchedString)
                    {
                        case "~":
                        case "?":
                            // Special handling to omit failure check
                            code.AppendLine($"        x.Operator(\"_{t.MatchedString}\", 0);");
                            break;

                        default:
                            code.AppendLine($"        x.Operator(\"_{t.MatchedString}\", 1);");
                            break;
                    }
                    break;

                case Token.Type.EXPRESSION:
                    code.AppendLine($"        x.Constant({t.MatchedString});");
                    break;

                case Token.Type.COLON:
                case Token.Type.COMMA:
                case Token.Type.FAILURE_GOTO:
                case Token.Type.L_ANGLE:
                case Token.Type.L_ANGLE_FAILURE:
                case Token.Type.L_ANGLE_SUCCESS:
                case Token.Type.L_ANGLE_UNCONDITIONAL:
                case Token.Type.L_PAREN_CHOICE:
                case Token.Type.L_PAREN_FAILURE:
                case Token.Type.L_PAREN_FUNCTION:
                case Token.Type.L_PAREN_SUCCESS:
                case Token.Type.L_PAREN_UNCONDITIONAL:
                case Token.Type.L_SQUARE:
                case Token.Type.R_ANGLE_FAILURE:
                case Token.Type.R_ANGLE_SUCCESS:
                case Token.Type.R_ANGLE_UNCONDITIONAL:
                case Token.Type.R_PAREN_SUCCESS:
                case Token.Type.R_PAREN_UNCONDITIONAL:
                case Token.Type.R_PAREN_FAILURE:
                case Token.Type.SPACE:
                case Token.Type.SUCCESS_GOTO:
                case Token.Type.UNARY_STAR:
                case Token.Type.BINARY_HASH:
                case Token.Type.BINARY_TILDE:
                default:
                    throw new ApplicationException("ToCSharp(List<Token> tokenList)");
            }
        }

        return code;
    }

    #endregion

    #region Expression Generation

    private void GenerateExpressions()
    {
        if (_parent.Execute is not null)
        {
            _parent.RecordedExpressionCount = _parent.Execute.PreviousStarFunctionCount;
        }

        for (; _parent.RecordedExpressionCount < _parent.ParseExpression.Count; ++_parent.RecordedExpressionCount)
        {
            _csharpCode.AppendLine("");
            _csharpCode.AppendLine($@"    public void Star{_parent.RecordedExpressionCount:D8}(Executive x)");
            _csharpCode.AppendLine("    {");
            _csharpCode.Append(ToCSharp(_parent.ParseExpression[_parent.RecordedExpressionCount]));
            _csharpCode.AppendLine("    }");
        }
    }

    #endregion

    #region Goto Helpers

    private void DirectGoto()
    {
        _csharpCode.AppendLine("        if (x.IdentifierTable.ContainsKey(x.SystemStack.Peek().Symbol))");
        _csharpCode.AppendLine("            return ((CodeVar)(x.IdentifierTable[x.SystemStack.Pop().Symbol])).StatementNumber;");
        _csharpCode.AppendLine($"        x.LogRuntimeException({24});");
    }

    private void LabelGoto()
    {
        _csharpCode.AppendLine("        if (x.LabelTable[x.SystemStack.Peek().Symbol] != Executive.GotoNotFound)");
        _csharpCode.AppendLine("            return x.LabelTable[x.SystemStack.Pop().Symbol];");
        _csharpCode.AppendLine($"        x.LogRuntimeException({23});");
    }

    #endregion

    #region Utility Methods

    private void MarkCodeAsCompiled()
    {
        foreach (var line in _parent.Code.SourceLines)
        {
            line.Compiled = true;
        }
    }

    private void WriteCode(string fileName)
    {
        using StreamWriter srText = new(fileName);
        srText.Write(_csharpCode.ToString());
    }

    #endregion
}