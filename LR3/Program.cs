using System;
using System.Windows.Forms;

namespace MTRAN.LR3
{
    class PerlTokenizer
    {
        [STAThread]
        static void Main()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Perl files (*.pl)|*.pl|All files (*.*)|*.*",
                Title = "Select a .txt or .pl file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

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
                    PrintAST(statement, indent + 4, prefix + "│   ");
                }
            }
            else if (node is VariableDeclarationNode declaration)
            {
                Console.WriteLine(prefix + "Declaration:");
                Console.WriteLine(prefix + "│   Keyword: " + declaration.Keyword);
                Console.WriteLine(prefix + "│   Variable: " + declaration.Variable);
                Console.WriteLine(prefix + "│   Assignment:");
                PrintAST(declaration.Value, indent + 4, prefix + "│   ");
            }
            else if (node is AssignmentNode assignment)
            {
                Console.WriteLine(prefix + "Assignment:");
                Console.WriteLine(prefix + "│   Variable: " + assignment.Variable);
                Console.WriteLine(prefix + "│   Operator: =");
                PrintAST(assignment.Value, indent + 4, prefix + "│   ");
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
                PrintAST(ifNode.Condition, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(ifNode.ThenBranch, indent + 4, prefix + "│   ");
                foreach (var elseIfNode in ifNode.ElseIfBranches)
                {
                    Console.WriteLine(prefix + "ElseIf:");
                    Console.WriteLine(prefix + "│   Condition:");
                    PrintAST(elseIfNode.Condition, indent + 4, prefix + "│   ");
                    Console.WriteLine(prefix + "│   Body:");
                    PrintAST(elseIfNode.ThenBranch, indent + 4, prefix + "│   ");
                }
                if (ifNode.ElseBranch != null)
                {
                    Console.WriteLine(prefix + "Else:");
                    Console.WriteLine(prefix + "│   Body:");
                    PrintAST(ifNode.ElseBranch, indent + 4, prefix + "│   ");
                }
            }
            else if (node is ForNode forNode)
            {
                Console.WriteLine(prefix + "For:");
                Console.WriteLine(prefix + "│   Initialization:");
                PrintAST(forNode.Initialization, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Condition:");
                PrintAST(forNode.Condition, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Increment:");
                PrintAST(forNode.Increment, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(forNode.Body, indent + 4, prefix + "│   ");
            }
            else if (node is ForeachNode foreachNode)
            {
                Console.WriteLine(prefix + "Foreach:");
                Console.WriteLine(prefix + "│   Variable: " + foreachNode.Variable);
                Console.WriteLine(prefix + "│   Collection:");
                PrintAST(foreachNode.Collection, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(foreachNode.Body, indent + 4, prefix + "│   ");
            }
            else if (node is WhileNode whileNode)
            {
                Console.WriteLine(prefix + "While:");
                Console.WriteLine(prefix + "│   Condition:");
                PrintAST(whileNode.Condition, indent + 4, prefix + "│   ");
                Console.WriteLine(prefix + "│   Body:");
                PrintAST(whileNode.Body, indent + 4, prefix + "│   ");
            }
            else if (node is UnaryOperationNode unaryOp)
            {
                Console.WriteLine(prefix + "Unary Operation:");
                Console.WriteLine(prefix + "│   Variable: " + unaryOp.Variable);
                Console.WriteLine(prefix + "│   Operator: " + unaryOp.Operator);
            }
        }
    }
}