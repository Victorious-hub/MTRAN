using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using MTRAN.Interpreter;

namespace MTRAN.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            FileChooserDialog fileChooser = new FileChooserDialog(
                "Select a .txt or .pl file",
                null,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept
            );

            // Add file filters
            FileFilter filter = new FileFilter();
            filter.AddPattern("*.txt");
            filter.AddPattern("*.pl");
            filter.AddPattern("*.*");
            fileChooser.Filter = filter;

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                string filePath = fileChooser.Filename;

                try
                {
                    // Read the file content
                    string fileContent = File.ReadAllText(filePath);
                    Console.WriteLine("File content successfully read:\n" + fileContent);

                    // Tokenize and parse the Perl code
                    var lexer = new PerlLexerParser(fileContent);
                    lexer.Tokenize();
                    // lexer.PrintUniqueTokens();
                    var parser = new Parser(lexer);
                    ASTNode ast = parser.Parse();

                    //  var parser = new Parser(lexer);
                    // ASTNode ast = parser.Parse();
                    PrintAST(ast, 0, "Program");

                    // Perform semantic analysis
                    // var interpreterAnalyzer = new InterpreterAnalyzer();
                    // interpreterAnalyzer.Analyze(ast);

                    // // // Console.WriteLine("Semantic analysis completed successfully.");
                    // // // Console.WriteLine("\nSemantic Tree:");
                    // // // semanticAnalyzer.PrintSymbolTable();
                    // interpreterAnalyzer.ReportErrors();   // Print all semantic errors

                    // // Perform interpretation
                    // Console.WriteLine("\nInterpreting the program...");
                    // interpreterAnalyzer.Interpret(ast);   // Interpret the AST
                    // interpreterAnalyzer.PrintVariableValues(); // Print variable values after interpretation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No file selected.");
            }

            fileChooser.Destroy();
            Application.Quit();
            static void PrintAST(ASTNode node, int indent, string prefix = "")
            {

                if (prefix == "Program")
                {
                    Console.WriteLine(prefix);
                    prefix = "  ";
                }
                 if (node is InputNode)
                {
                    Console.WriteLine(prefix + "InputNode: <STDIN>");
                }
                //  if (node is ShiftNode)
                // {
                //     Console.WriteLine(prefix + "ShiftNode: Retrieves the first element from @_");
                // }
                if (node is LastNode lastNode)
                {
                    Console.WriteLine(prefix + "Break operation");

                    // Print the condition if it exists
                    if (lastNode.Condition != null)
                    {
                        Console.WriteLine(prefix + "  Condition:");
                        PrintAST(lastNode.Condition, indent + 4, prefix + "    ");
                    }
                }
                 if (node is KeywordNode keywordNode)
    {
        Console.WriteLine(prefix + "Keyword: " + keywordNode.Keyword);
    }
                 if (node is RegexNode regexNode)
                {
                    Console.WriteLine(prefix + "Regex Pattern: " + regexNode.Pattern);
                }
                if (node is ReferenceNode referenceNode)
                {
                    Console.WriteLine(prefix + "Reference:");
                    Console.WriteLine(prefix + "    Referenced Variable: " + referenceNode.ReferencedVariable);
                }
                if (node is ErrorNode errorNode)
                {
                    Console.WriteLine(prefix + "Error:");
                    Console.WriteLine(prefix + "├── Message: " + errorNode.ErrorMessage);
                }
                // if (node is ImplicitParameterNode implicitParameterNode)
                // {
                //     Console.WriteLine(prefix + "Implicit Parameter:");
                //     Console.WriteLine(prefix + "    Name: " + implicitParameterNode.ParameterName);

                //     if (implicitParameterNode.Value != null)
                //     {
                //         Console.WriteLine(prefix + "    Value:");
                //         PrintAST(implicitParameterNode.Value, indent + 4, prefix + "        ");
                //     }
                //     else
                //     {
                //         Console.WriteLine(prefix + "    Value: (null)");
                //     }
                // }
                else if (node is StatementListNode statementList)
                {
                    foreach (var statement in statementList.Statements)
                    {
                        PrintAST(statement, indent, prefix);
                    }
                }
                else if (node is HashAccessNode hashAccessNode)
                {
                    Console.WriteLine(prefix + "Hash Access:");
                    Console.WriteLine(prefix + "    Hash Name: " + hashAccessNode.HashName);
                    Console.WriteLine(prefix + "    Key:");
                    PrintAST(hashAccessNode.Key, indent + 4, prefix + "        ");
                }
                else if (node is ConstructorCallNode constructorCall)
                {
                    Console.WriteLine(prefix + "Constructor Call:");
                    Console.WriteLine(prefix + "    Package Name: " + constructorCall.PackageName);
                    Console.WriteLine(prefix + "    Constructor Name: " + constructorCall.ConstructorName);
                    Console.WriteLine(prefix + "    Arguments:");
                    foreach (var argument in constructorCall.Arguments)
                    {
                        PrintAST(argument, indent + 4, prefix + "        ");
                    }
                }
                else if (node is UseNode useNode)
                {
                    Console.WriteLine($"{prefix}Use Statement:");
                    Console.WriteLine($"{prefix}  Constant Name: {useNode.ConstantName}");
                    Console.WriteLine($"{prefix}  Value:");
                    PrintAST(useNode.Value, indent + 2, prefix + "    ");
                }
                else if (node is ConstantDeclarationNode constantNode)
                {
                    Console.WriteLine(prefix + "Constant Declaration:");
                    Console.WriteLine(prefix + "  Name: " + constantNode.Name);
                    Console.WriteLine(prefix + "  Value:");
                    PrintAST(constantNode.Value, indent + 4, prefix + "    ");
                }
                 else if (node is UnlessNode unlessNode)
                {
                    Console.WriteLine(prefix + "Unless:");
                    Console.WriteLine(prefix + "      Condition:");
                    PrintAST(unlessNode.Condition, indent + 8, prefix + "          ");
                    Console.WriteLine(prefix + "      Body:");
                    PrintAST(unlessNode.ThenBranch, indent + 8, prefix + "          ");
                    if (unlessNode.ElseBranch != null)
                    {
                        Console.WriteLine(prefix + "Else:");
                        Console.WriteLine(prefix + "      Body:");
                        PrintAST(unlessNode.ElseBranch, indent + 8, prefix + "          ");
                    }
                }
                else if (node is InterpolatedStringNode interpolatedStringNode)
                {
                    Console.WriteLine($"{prefix}Interpolated String:");
                    foreach (var part in interpolatedStringNode.Parts)
                    {
                        PrintAST(part, indent + 2, prefix + "    ");
                    }
                }
                else if (node is ForRangeNode forRangeNode)
            {
                Console.WriteLine(prefix + "ForRange:");
                Console.WriteLine(prefix + "      Loop Variable:");
                PrintAST(forRangeNode.LoopVariable, indent + 8, prefix + "          ");
                Console.WriteLine(prefix + "      Start:");
                PrintAST(forRangeNode.Start, indent + 8, prefix + "          ");
                Console.WriteLine(prefix + "      End:");
                PrintAST(forRangeNode.End, indent + 8, prefix + "          ");
                Console.WriteLine(prefix + "      Body:");
                PrintAST(forRangeNode.Body, indent + 8, prefix + "          ");
            }
                else if (node is ArrayAccessNode arrayAccessNode)
                {
                    Console.WriteLine(prefix + "Array Access:");
                    Console.WriteLine(prefix + "    Array Name: " + arrayAccessNode.ArrayName);
                    Console.WriteLine(prefix + "    Index:");
                    PrintAST(arrayAccessNode.Index, indent + 4, prefix + "        ");
                }
                else if (node is BlockNode block)
                {
                    // Console.WriteLine(prefix + "Block:");
                    foreach (var statement in block.Statements)
                    {
                        PrintAST(statement, indent + 4, prefix + "");
                    }
                }
                else if (node is VariableDeclarationNode declaration)
                {
                    Console.WriteLine(prefix + "Declaration:");
                    Console.WriteLine(prefix + "      Keyword: " + declaration.Keyword);
                    Console.WriteLine(prefix + "      Variable: " + declaration.Variable);

                    if (declaration.Value is CompoundAssignmentNode compoundAssignment)
                    {
                        Console.WriteLine(prefix + "      Assignment (" + compoundAssignment.OperatorSymbol + "):");
                        PrintAST(compoundAssignment.Value, indent + 8, prefix + "          ");
                    }
                    else if (declaration.Value != null)
                    {
                        Console.WriteLine(prefix + "      Assignment (=):");
                        PrintAST(declaration.Value, indent + 8, prefix + "          ");
                    }
                    // Console.WriteLine(prefix + "├── │   Punctuation: ;");
                }
                else if (node is AssignmentNode assignment)
                {
                    Console.WriteLine(prefix + "Assignment (=):");
                    Console.WriteLine(prefix + "      Variable: " + assignment.Variable);
                    Console.WriteLine(prefix + "      Value:");
                    PrintAST(assignment.Value, indent + 8, prefix + "      ");
                }
                else if (node is CompoundAssignmentNode compoundAssignment)
                {
                    Console.WriteLine(prefix + "Compound Assignment (" + compoundAssignment.OperatorSymbol + "):");
                    Console.WriteLine(prefix + "      Variable: " + compoundAssignment.Variable);
                    Console.WriteLine(prefix + "      Value:");
                    PrintAST(compoundAssignment.Value, indent + 8, prefix + "      ");
                }
                else if (node is BinaryOperationNode binaryOp)
                {
                    string operatorType;

                    if (IsComparisonOperator(binaryOp.Operator))
                    {
                        operatorType = "Comparison Operator";
                    }
                    else if (IsLogicalOperator(binaryOp.Operator))
                    {
                        operatorType = "Logical Operator";
                    }
                    else
                    {
                        operatorType = "Binary Operator";
                    }

                    Console.WriteLine(prefix + operatorType + " (" + binaryOp.Operator + "):");
                    Console.WriteLine(prefix + "      Left:");
                    PrintAST(binaryOp.Left, indent + 8, prefix + "      ");
                    Console.WriteLine(prefix + "      Right:");
                    PrintAST(binaryOp.Right, indent + 8, prefix + "      ");
                }
                else if (node is NumberNode number)
                {
                    Console.WriteLine(prefix + "      Literal: " + number.Value);
                }
                else if (node is StringNode str)
                {
                    Console.WriteLine(prefix + "      Literal: \"" + str.Value + "\"");
                }
                else if (node is VariableNode variable)
                {
                    Console.WriteLine(prefix + "      Variable: " + variable.Name);
                }
                else if (node is IfNode ifNode)
                {
                    Console.WriteLine(prefix + "If:");
                    Console.WriteLine(prefix + "      Condition:");
                    PrintAST(ifNode.Condition, indent + 8, prefix + "          ");
                    Console.WriteLine(prefix + "      Body:");
                    PrintAST(ifNode.ThenBranch, indent + 8, prefix + "          ");
                    foreach (var elseIfNode in ifNode.ElseIfBranches)
                    {
                        Console.WriteLine(prefix + "ElseIf:");
                        Console.WriteLine(prefix + "      Condition:");
                        PrintAST(elseIfNode.Condition, indent + 8, prefix + "          ");
                        Console.WriteLine(prefix + "      Body:");
                        PrintAST(elseIfNode.ThenBranch, indent + 8, prefix + "          ");
                    }
                    if (ifNode.ElseBranch != null)
                    {
                        Console.WriteLine(prefix + "Else:");
                        Console.WriteLine(prefix + "      Body:");
                        PrintAST(ifNode.ElseBranch, indent + 8, prefix + "          ");
                    }
                }
                else if (node is ForNode forNode)
                {
                    Console.WriteLine(prefix + "For:");
                    PrintAST(forNode.Initialization, indent + 4, prefix + "      ");
                    Console.WriteLine(prefix + "      Body:");
                    PrintAST(forNode.Body, indent + 4, prefix + "          ");
                }
                else if (node is ForeachNode foreachNode)
                {
                    Console.WriteLine(prefix + "Foreach:");
                    Console.WriteLine(prefix + "      Variable Declaration:");
                    PrintAST(foreachNode.VariableDeclaration, indent + 8, prefix + "          ");
                    Console.WriteLine(prefix + "      Array:");
                    PrintAST(foreachNode.Array, indent + 8, prefix + "          "); // Added to output the array
                    Console.WriteLine(prefix + "      Body:");
                    PrintAST(foreachNode.Body, indent + 8, prefix + "         ");
                }
                else if (node is WhileNode whileNode)
                {
                    Console.WriteLine(prefix + "While:");
                    Console.WriteLine(prefix + "    Condition:");
                    PrintAST(whileNode.Condition, indent + 8, prefix + "      ");
                    Console.WriteLine(prefix + "      Body:");
                    PrintAST(whileNode.Body, indent + 8, prefix + "           ");
                }
                else if (node is UnaryOperationNode unaryOp)
                {
                    Console.WriteLine(prefix + "Unary Operation (" + unaryOp.Operator + "):");
                    Console.WriteLine(prefix + "      Variable: " + unaryOp.Variable);
                }
                else if (node is GroupedAssignmentNode groupedAssignment)
                {
                    Console.WriteLine(prefix + "Declaration:");
                    Console.WriteLine(prefix + "    Keyword: my");
                    Console.WriteLine(prefix + "    Variables:");
                    foreach (var groupedVariable in groupedAssignment.Variables) // Renamed 'variable' to 'groupedVariable'
                    {
                        Console.WriteLine(prefix + "        Variable: " + ((VariableNode)groupedVariable).Name);
                    }
                    Console.WriteLine(prefix + "    Value:");
                    PrintAST(groupedAssignment.Value, indent + 8, prefix + "    ");
                }
                else if (node is FunctionNode functionNode)
                {
                    Console.WriteLine(prefix + (functionNode.IsMethod ? "Method:" : "Function:"));
                    Console.WriteLine(prefix + "    Name: " + functionNode.Name);

                    if (functionNode.ClassName != null)
                    {
                        Console.WriteLine(prefix + "    Class: " + functionNode.ClassName);
                    }
                    Console.WriteLine(prefix + "    Class: " + functionNode.ClassName);

                    Console.WriteLine(prefix + "    Parameters:");
                    foreach (var parameter in functionNode.Parameters)
                    {
                        PrintAST(parameter, indent + 4, prefix + "        ");
                    }

                    Console.WriteLine(prefix + "    Body:");
                    PrintAST(functionNode.Body, indent + 8, prefix + "        ");
                }
                else if (node is ParameterNode parameterNode)
                {
                    Console.WriteLine(prefix + "Parameter: " + parameterNode.Name);
                }
                else if (node is ReturnNode returnNode)
                {
                    Console.WriteLine(prefix + "Return:");
                    PrintAST(returnNode.Value, indent + 8, prefix + "");
                }
                else if (node is ArrayNode arrayNode)
                {
                    Console.WriteLine(prefix + "Array:");
                    foreach (var element in arrayNode.Elements)
                    {
                        PrintAST(element, indent + 8, prefix + "      ");
                    }
                }
                else if (node is ParenthesizedExpression parenthesizedExpression)
                {
                    Console.WriteLine(prefix + "ParenthesizedExpression:");
                    Console.WriteLine(prefix + "    Delimiter: (");
                    PrintAST(parenthesizedExpression.Expression, indent + 8, prefix + "      ");
                    Console.WriteLine(prefix + "    Delimiter: )");
                }
                else if (node is PunctuationNode punctuationNode)
                {
                    Console.WriteLine(prefix + "Delimiter: " + punctuationNode.Punctuation);
                }
                else if (node is FunctionCallNode functionCall)
                {
                    Console.WriteLine(prefix + "Function Call:");
                    Console.WriteLine(prefix + "    Function Name: " + functionCall.FunctionName);
                    Console.WriteLine(prefix + "    Arguments:");
                    foreach (var argument in functionCall.Arguments)
                    {
                        PrintAST(argument, indent + 4, prefix + "    ");
                    }
                }
                else if (node is HashNode hashNode)
                {
                    Console.WriteLine(prefix + "Hash:");
                    foreach (var (key, value) in hashNode.Elements)
                    {
                        Console.WriteLine(prefix + "      Key:");
                        PrintAST(key, indent + 8, prefix + "");
                        Console.WriteLine(prefix + "      Value:");
                        PrintAST(value, indent + 8, prefix + "");
                    }
                }
                else if (node is PrintStatementNode printStatement)
                {
                    Console.WriteLine(prefix + "Print Statement:");
                    foreach (var argument in printStatement.Arguments)
                    {
                        PrintAST(argument, indent + 4, prefix + "");
                    }
                }
                else if (node is BlessNode blessNode)
                {
                    Console.WriteLine(prefix + "Bless:");
                    Console.WriteLine(prefix + "    Object: ");
                    PrintAST(blessNode.Object, indent + 4, prefix + "    ");
                    Console.WriteLine(prefix + "└── Class: ");
                    PrintAST(blessNode.Class, indent + 4, prefix + "    ");
                }
                else if (node is ClassNode classNode)
                {
                    Console.WriteLine(prefix + "Package:");
                    Console.WriteLine(prefix + "└── Name: " + classNode.ClassName);
                    foreach (var method in classNode.Methods)
                    {
                        PrintAST(method, indent + 4, prefix + "   ");
                    }
                }
                else if (node is ObjectNode objectNode)
                {
                    Console.WriteLine(prefix + "Object:");
                    Console.WriteLine(prefix + "    Name: " + objectNode.Name);

                    foreach (var (key, value) in objectNode.Properties)
                    {
                        Console.WriteLine(prefix + "    Property:");
                        Console.WriteLine(prefix + "        Key:");
                        PrintAST(key, indent + 8, prefix + "        ");
                        Console.WriteLine(prefix + "    └── Value:");
                        PrintAST(value, indent + 8, prefix + "        ");
                    }
                }
            }

            static bool IsComparisonOperator(string op)
            {
                return op == "<" || op == "<=" || op == ">" || op == ">=" || op == "==" || op == "!=";
            }

            static bool IsLogicalOperator(string op)
            {
                return op == "&&" || op == "and" || op == "||" || op == "or" || op == "xor" || op == "^";
            }
        }
    }
}