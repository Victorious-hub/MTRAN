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
                foreach (var statement in block.Statements)
                {
                    PrintAST(statement, indent + 4, prefix + "");
                }
            }
            else if (node is VariableDeclarationNode declaration)
            {
                Console.WriteLine(prefix + "Declaration:");
                Console.WriteLine(prefix + "│   Keyword: " + declaration.Keyword);
                Console.WriteLine(prefix + "│   Variable: " + declaration.Variable);
                if (declaration.Value != null)
                {
                    Console.WriteLine(prefix + "│   Assignment:");
                    PrintAST(declaration.Value, indent + 4, prefix + "│   │   ");
                }
            }
            else if (node is AssignmentNode assignment)
            {
                Console.WriteLine(prefix + "Assignment:");
                Console.WriteLine(prefix + "│   Variable: " + assignment.Variable);
                PrintAST(assignment.Value, indent + 4, prefix + "│   ");
            }
            else if (node is CompoundAssignmentNode compoundAssignment)
            {
                Console.WriteLine(prefix + "Compound Assignment:");
                Console.WriteLine(prefix + "│   Variable: " + compoundAssignment.Variable);
                Console.WriteLine(prefix + "│   Operator: " + compoundAssignment.OperatorSymbol);
                PrintAST(compoundAssignment.Value, indent + 4, prefix + "│   ");
            }
            else if (node is BinaryOperationNode binaryOp)
            {
                Console.WriteLine(prefix + "│   Expression:");
                Console.WriteLine(prefix + "│   │   Operator: " + binaryOp.Operator);
                Console.WriteLine(prefix + "│   │   Left:");
                PrintAST(binaryOp.Left, indent + 4, prefix + "│   │    ");
                Console.WriteLine(prefix + "│   │   Right:");
                PrintAST(binaryOp.Right, indent + 4, prefix + "│   │    ");
            }
            else if (node is NumberNode number)
            {
                Console.WriteLine(prefix + "│   Literal: " + number.Value);
            }
            else if (node is StringNode str)
            {
                Console.WriteLine(prefix + "│   Literal: \"" + str.Value + "\"");
            }
            else if (node is VariableNode variable)
            {
                Console.WriteLine(prefix + "│   Variable: " + variable.Name);
            }
            else if (node is IfNode ifNode)
            {
                Console.WriteLine(prefix + "If:");
                Console.WriteLine(prefix + "│   Condition:");
                PrintAST(ifNode.Condition, indent + 4, prefix + "│   │   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(ifNode.ThenBranch, indent + 4, prefix + "│   │   ");
                foreach (var elseIfNode in ifNode.ElseIfBranches)
                {
                    Console.WriteLine(prefix + "ElseIf:");
                    Console.WriteLine(prefix + "│   Condition:");
                    PrintAST(elseIfNode.Condition, indent + 4, prefix + "│   ");
                    Console.WriteLine(prefix + "│   Body:");
                    PrintAST(elseIfNode.ThenBranch, indent + 4, prefix + "│   │   ");
                }
                if (ifNode.ElseBranch != null)
                {
                    Console.WriteLine(prefix + "Else:");
                    Console.WriteLine(prefix + "│   Body:");
                    PrintAST(ifNode.ElseBranch, indent + 4, prefix + "│   │   ");
                }
            }
            else if (node is ForNode forNode)
            {
                Console.WriteLine(prefix + "For:");
                PrintAST(forNode.Initialization, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Condition:");
                PrintAST(forNode.Condition, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Increment/Decrement:");
                PrintAST(forNode.Increment, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(forNode.Body, indent + 4, prefix + "│   │   ");
            }
            else if (node is ForeachNode foreachNode)
            {
                Console.WriteLine(prefix + "Foreach:");
                Console.WriteLine(prefix + "│   Variable Declaration:");
                PrintAST(foreachNode.VariableDeclaration, indent + 4, prefix + "│   │   ");
                Console.WriteLine(prefix + "│   Array:");
                PrintAST(foreachNode.Array, indent + 4, prefix + "│   │   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(foreachNode.Body, indent + 4, prefix + "│   │   ");
            }
            else if (node is WhileNode whileNode)
            {
                Console.WriteLine(prefix + "While:");
                Console.WriteLine(prefix + "│   Condition:");
                PrintAST(whileNode.Condition, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(whileNode.Body, indent + 4, prefix + "│   │   ");
            }
            else if (node is UnaryOperationNode unaryOp)
            {
                Console.WriteLine(prefix + "│   Unary Operation:");
                Console.WriteLine(prefix + "│   │   Variable: " + unaryOp.Variable);
                Console.WriteLine(prefix + "│   │   Operator: " + unaryOp.Operator);
            }
            else if (node is FunctionNode functionNode)
            {
                Console.WriteLine(prefix + "Function:");
                Console.WriteLine(prefix + "│   Name: " + functionNode.Name);
                Console.WriteLine(prefix + "│   Parameters:");
                foreach (var parameter in functionNode.Parameters)
                {
                    PrintAST(parameter, indent + 4, prefix + "│   │   ");
                }
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(functionNode.Body, indent + 4, prefix + "│   │   ");
            }
            else if (node is ParameterNode parameterNode)
            {
                Console.WriteLine(prefix + "Parameter: " + parameterNode.Name);
            }
            else if (node is ReturnNode returnNode)
            {
                Console.WriteLine(prefix + "Return:");
                PrintAST(returnNode.Value, indent + 4, prefix + "│   ");
            }
            else if (node is ArrayNode arrayNode)
            {
                Console.WriteLine(prefix + "Array:");
                foreach (var element in arrayNode.Elements)
                {
                    PrintAST(element, indent + 4, prefix + "│   ");
                }
            }
            else if (node is ParenthesizedExpression parenthesizedExpression)
            {
                Console.WriteLine(prefix + "ParenthesizedExpression:");
                Console.WriteLine(prefix + "│   Delimiter: (");
                PrintAST(parenthesizedExpression.Expression, indent + 4, prefix + "│   │   ");
                Console.WriteLine(prefix + "│   Delimiter: )");
            }
            else if (node is PunctuationNode punctuationNode)
            {
                Console.WriteLine(prefix + "Punctuation: " + punctuationNode.Punctuation);
            }
            else if (node is HashNode hashNode)
            {
                Console.WriteLine(prefix + "Hash:");
                foreach (var (key, value) in hashNode.Elements)
                {
                    Console.WriteLine(prefix + "│   Key:");
                    PrintAST(key, indent + 4, prefix + "│   │   ");
                    Console.WriteLine(prefix + "│   Value:");
                    PrintAST(value, indent + 4, prefix + "│   │   ");
                }
            }
        }
    }
}