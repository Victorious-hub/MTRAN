using System;
using Gtk;

namespace MTRAN.Parser
{
    class PerlTokenizer
    {
        [STAThread]
        static void Main()
        {
            Application.Init();

            FileChooserDialog fileChooser = new FileChooserDialog(
                "Select a .txt or .pl file",
                null,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept
            );

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.txt");
            fileChooser.Filter.AddPattern("*.pl");
            fileChooser.Filter.AddPattern("*.*");

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                string filePath = fileChooser.Filename;

                try
                {
                    string fileContent = System.IO.File.ReadAllText(filePath);
                    Console.WriteLine("File content successfully read:\n" + fileContent);

                    var lexer = new PerlLexer(fileContent);
                    lexer.Tokenize();
                    var parser = new Parser(lexer);
                    ASTNode ast = parser.Parse();
                    PrintAST(ast, 0, "Program");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No file selected.");
            }

            fileChooser.Destroy();
            Application.Quit();
        }

        static void PrintAST(ASTNode node, int indent, string prefix = "")
        {
            if (prefix == "Program")
            {
                Console.WriteLine(prefix);
                prefix = "├── ";
            }

            if (node is StatementListNode statementList)
            {
                foreach (var statement in statementList.Statements)
                {
                    PrintAST(statement, indent, prefix);
                }
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
                Console.WriteLine(prefix + "├── │   Keyword: " + declaration.Keyword);
                Console.WriteLine(prefix + "├── │   Variable: " + declaration.Variable);

                if (declaration.Value is CompoundAssignmentNode compoundAssignment)
                {
                    Console.WriteLine(prefix + "├── │   Assignment (" + compoundAssignment.OperatorSymbol + "):");
                    PrintAST(compoundAssignment.Value, indent + 8, prefix + "├── │   │   ");
                }
                else if (declaration.Value != null)
                {
                    Console.WriteLine(prefix + "├── │   Assignment (=):");
                    PrintAST(declaration.Value, indent + 8, prefix + "├── │   │   ");
                }
                // Console.WriteLine(prefix + "├── │   Punctuation: ;");
            }
            else if (node is AssignmentNode assignment)
            {
                Console.WriteLine(prefix + "Assignment (=):");
                Console.WriteLine(prefix + "├── │   Variable: " + assignment.Variable);
                Console.WriteLine(prefix + "├── │   Value:");
                PrintAST(assignment.Value, indent + 8, prefix + "├── │   ");
            }
            else if (node is CompoundAssignmentNode compoundAssignment)
            {
                Console.WriteLine(prefix + "Assignment (" + compoundAssignment.OperatorSymbol + "):");
                Console.WriteLine(prefix + "├── │   Variable: " + compoundAssignment.Variable);
                Console.WriteLine(prefix + "├── │   Value:");
                PrintAST(compoundAssignment.Value, indent + 8, prefix + "├── │   ");
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
                Console.WriteLine(prefix + "├── │   Left:");
                PrintAST(binaryOp.Left, indent + 8, prefix + "├── │   │   ");
                Console.WriteLine(prefix + "├── │   Right:");
                PrintAST(binaryOp.Right, indent + 8, prefix + "├── │   │   ");
            }
            else if (node is NumberNode number)
            {
                Console.WriteLine(prefix + "├── │   Literal: " + number.Value);
            }
            else if (node is StringNode str)
            {
                Console.WriteLine(prefix + "├── │   Literal: \"" + str.Value + "\"");
            }
            else if (node is VariableNode variable)
            {
                Console.WriteLine(prefix + "├── │   Variable: " + variable.Name);
            }
            else if (node is IfNode ifNode)
            {
                Console.WriteLine(prefix + "If:");
                Console.WriteLine(prefix + "├── │   Condition:");
                PrintAST(ifNode.Condition, indent + 8, prefix + "├── │   │   ");
                Console.WriteLine(prefix + "├── │   Body:");
                PrintAST(ifNode.ThenBranch, indent + 8, prefix + "├── │   │   ");
                foreach (var elseIfNode in ifNode.ElseIfBranches)
                {
                    Console.WriteLine(prefix + "ElseIf:");
                    Console.WriteLine(prefix + "├── │   Condition:");
                    PrintAST(elseIfNode.Condition, indent + 8, prefix + "├── │   │   ");
                    Console.WriteLine(prefix + "├── │   Body:");
                    PrintAST(elseIfNode.ThenBranch, indent + 8, prefix + "├── │   │   ");
                }
                if (ifNode.ElseBranch != null)
                {
                    Console.WriteLine(prefix + "Else:");
                    Console.WriteLine(prefix + "├── │   Body:");
                    PrintAST(ifNode.ElseBranch, indent + 8, prefix + "├── │   │   ");
                }
            }
            else if (node is ForNode forNode)
            {
                Console.WriteLine(prefix + "For:");
                PrintAST(forNode.Initialization, indent + 4, prefix + "├──| ");
                Console.WriteLine(prefix + "├── │   Body:");
                PrintAST(forNode.Body, indent + 4, prefix + "├── │   │ ");
            }
            else if (node is ForeachNode foreachNode)
            {
                Console.WriteLine(prefix + "Foreach:");
                Console.WriteLine(prefix + "├── │   Variable Declaration:");
                PrintAST(foreachNode.VariableDeclaration, indent + 8, prefix + "├── │   │   ");
                Console.WriteLine(prefix + "├── │   Body:");
                PrintAST(foreachNode.Body, indent + 8, prefix + "├── │   │");
            }
            else if (node is WhileNode whileNode)
            {
                Console.WriteLine(prefix + "While:");
                Console.WriteLine(prefix + "├── Condition:");
                PrintAST(whileNode.Condition, indent + 8, prefix + "├── │   ");
                Console.WriteLine(prefix + "├── │   Body:");
                PrintAST(whileNode.Body, indent + 8, prefix + "├── │   │    ");
            }
            else if (node is UnaryOperationNode unaryOp)
            {
                Console.WriteLine(prefix + "Unary Operation (" + unaryOp.Operator + "):");
                Console.WriteLine(prefix + "├── │   Variable: " + unaryOp.Variable);
            }
            else if (node is GroupedAssignmentNode groupedAssignment)
            {
                Console.WriteLine(prefix + "Declaration:");
                Console.WriteLine(prefix + "├── Keyword: my");
                Console.WriteLine(prefix + "├── Variables:");
                foreach (var groupedVariable in groupedAssignment.Variables) // Renamed 'variable' to 'groupedVariable'
                {
                    Console.WriteLine(prefix + "│   ├── Variable: " + ((VariableNode)groupedVariable).Name);
                }
                Console.WriteLine(prefix + "├── Value:");
                PrintAST(groupedAssignment.Value, indent + 8, prefix + "│   ");
            }
            else if (node is FunctionNode functionNode)
            {
                Console.WriteLine(prefix + "Function:");
                Console.WriteLine(prefix + "├── Name: " + functionNode.Name);
                Console.WriteLine(prefix + "├── Parameters:");
                foreach (var parameter in functionNode.Parameters)
                {
                    if (parameter is VariableDeclarationNode declaration1)
                    {
                        Console.WriteLine(prefix + "│   ├── Variable: " + declaration1.Variable);
                    }
                    else if (parameter is VariableNode variable1)
                    {
                        Console.WriteLine(prefix + "│   ├── Variable: " + variable1.Name);
                    }
                }
                Console.WriteLine(prefix + "├── Body:");
                PrintAST(functionNode.Body, indent + 8, prefix + "│   ");
            }
            else if (node is ParameterNode parameterNode)
            {
                Console.WriteLine(prefix + "Parameter: " + parameterNode.Name);
            }
            else if (node is ReturnNode returnNode)
            {
                Console.WriteLine(prefix + "Return:");
                PrintAST(returnNode.Value, indent + 8, prefix + "├── │  ");
            }
            else if (node is ArrayNode arrayNode)
            {
                Console.WriteLine(prefix + "Array:");
                foreach (var element in arrayNode.Elements)
                {
                    PrintAST(element, indent + 8, prefix + "├── │   ");
                }
            }
            else if (node is ParenthesizedExpression parenthesizedExpression)
            {
                Console.WriteLine(prefix + "ParenthesizedExpression:");
                Console.WriteLine(prefix + "├── Delimiter: (");
                PrintAST(parenthesizedExpression.Expression, indent + 8, prefix + "├── │   ");
                Console.WriteLine(prefix + "├── Delimiter: )");
            }
            else if (node is PunctuationNode punctuationNode)
            {
                Console.WriteLine(prefix + "Punctuation: " + punctuationNode.Punctuation);
            }
            else if (node is FunctionCallNode functionCall)
            {
                Console.WriteLine(prefix + "Function Call:");
                Console.WriteLine(prefix + "├── Function Name: " + functionCall.FunctionName);
                Console.WriteLine(prefix + "├── Arguments:");
                foreach (var argument in functionCall.Arguments)
                {
                    PrintAST(argument, indent + 4, prefix + "│   ");
                }
            }
            else if (node is HashNode hashNode)
            {
                Console.WriteLine(prefix + "Hash:");
                foreach (var (key, value) in hashNode.Elements)
                {
                    Console.WriteLine(prefix + "├── │   Key:");
                    PrintAST(key, indent + 8, prefix + "├── │   │   ");
                    Console.WriteLine(prefix + "├── │   Value:");
                    PrintAST(value, indent + 8, prefix + "├── │   │ ");
                }
            }
            else if (node is PrintStatementNode printStatement)
            {
                Console.WriteLine(prefix + "Print Statement:");
                foreach (var argument in printStatement.Arguments)
                {
                    PrintAST(argument, indent + 4, prefix + "├── ");
                }
            }
            else if (node is BlessNode blessNode)
            {
                Console.WriteLine(prefix + "Bless:");
                Console.WriteLine(prefix + "├── Object: ");
                PrintAST(blessNode.Object, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "└── Class: ");
                PrintAST(blessNode.Class, indent + 4, prefix + "    ");
            }
            else if (node is ClassNode classNode)
            {
                Console.WriteLine(prefix + "Package:");
                Console.WriteLine(prefix + "└── Name: " + classNode.ClassName);
                foreach (var method in classNode.Methods)
                {
                    PrintAST(method, indent + 4, prefix + "    ");
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


// using System;
// using System.IO;
// using Gtk;
// using MTRAN.Parser;

// class PerlTokenizer
// {
//     [STAThread]
//     static void Main()
//     {
//         Application.Init();

//         // Create a file chooser dialog
//         FileChooserDialog fileChooser = new FileChooserDialog(
//             "Select a .txt or .pl file",
//             null,
//             FileChooserAction.Open,
//             "Cancel", ResponseType.Cancel,
//             "Open", ResponseType.Accept
//         );

//         // Add filters for file types
//         FileFilter filter = new FileFilter();
//         filter.AddPattern("*.txt");
//         filter.AddPattern("*.pl");
//         filter.AddPattern("*.*");
//         fileChooser.Filter = filter;

//         if (fileChooser.Run() == (int)ResponseType.Accept)
//         {
//             string filePath = fileChooser.Filename;

//             try
//             {
//                 // Read the file content
//                 string fileContent = File.ReadAllText(filePath);
//                 Console.WriteLine("File content successfully read:\n" + fileContent);

//                 // Process the file content
//                 var lexer = new PerlLexer(fileContent);
//                 var tokens = lexer.Tokenize();
//                 lexer.PrintTokens();

//                 // Save the modified code
//                 lexer.SaveModifiedCode("modified_variables.pl");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error reading file: {ex.Message}");
//             }
//         }
//         else
//         {
//             Console.WriteLine("No file selected.");
//         }

//         fileChooser.Destroy();
//         Application.Quit();
//     }
// }