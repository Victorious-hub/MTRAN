namespace MTRAN.Semantic
{
    public class SemanticAnalyzer
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();

        public void Analyze(ASTNode node)
        {
            if (node is ProgramNode programNode)
            {
                _symbolTable.EnterScope("Block"); // Default scope is Block
                try
                {
                    foreach (var child in programNode.Children)
                    {
                        Analyze(child);
                    }
                }
                finally
                {
                    _symbolTable.ExitScope();
                }
            }
            else if (node is VariableDeclarationNode declarationNode)
            {
                if (_symbolTable.IsVariableDeclared(declarationNode.Variable))
                {
                    throw new Exception($"Semantic Error: Variable '{declarationNode.Variable}' is already declared.");
                }

                string type = declarationNode.Value != null ? InferType(declarationNode.Value) : "unknown";
                _symbolTable.DeclareVariable(declarationNode.Variable, type);

                if (declarationNode.Value != null)
                {
                    Analyze(declarationNode.Value);
                }
            }
            else if (node is AssignmentNode assignmentNode)
            {
                if (!_symbolTable.IsVariableDeclared(assignmentNode.Variable))
                {
                    throw new Exception($"Semantic Error: Variable '{assignmentNode.Variable}' is not declared.");
                }

                Analyze(assignmentNode.Value);
            }
            else if (node is IfNode ifNode)
            {
                Analyze(ifNode.Condition);

                _symbolTable.EnterScope("If-Then");
                try
                {
                    // Analyze the body of the If-Then without creating a new Block scope
                    Analyze(ifNode.ThenBranch);
                }
                finally
                {
                    _symbolTable.ExitScope();
                }

                foreach (var elseIfBranch in ifNode.ElseIfBranches)
                {
                    _symbolTable.EnterScope("ElseIf");
                    try
                    {
                        Analyze(elseIfBranch.Condition);
                        Analyze(elseIfBranch.ThenBranch);
                    }
                    finally
                    {
                        _symbolTable.ExitScope();
                    }
                }

                if (ifNode.ElseBranch != null)
                {
                    _symbolTable.EnterScope("Else");
                    try
                    {
                        Analyze(ifNode.ElseBranch);
                    }
                    finally
                    {
                        _symbolTable.ExitScope();
                    }
                }
            }
            else if (node is BlockNode blockNode)
            {
                // Only enter a new Block scope if not already in a specific scope like If-Then
                if (!_symbolTable.IsCurrentScope("If-Then") && !_symbolTable.IsCurrentScope("ElseIf") && !_symbolTable.IsCurrentScope("Else"))
                {
                    _symbolTable.EnterScope("Block");
                }

                try
                {
                    foreach (var statement in blockNode.Statements)
                    {
                        Analyze(statement);
                    }
                }
                finally
                {
                    if (!_symbolTable.IsCurrentScope("If-Then") && !_symbolTable.IsCurrentScope("ElseIf") && !_symbolTable.IsCurrentScope("Else"))
                    {
                        _symbolTable.ExitScope();
                    }
                }
            }
            else if (node is BinaryOperationNode binaryNode)
            {
                Analyze(binaryNode.Left);
                Analyze(binaryNode.Right);
            }
            else if (node is VariableNode variableNode)
            {
                if (!_symbolTable.IsVariableDeclared(variableNode.Name))
                {
                    throw new Exception($"Semantic Error: Variable '{variableNode.Name}' is not declared.");
                }
            }
            else if (node is ParenthesizedExpression parenthesizedExpression)
            {
                Analyze(parenthesizedExpression.Expression);
            }
            else if (node is StringNode || node is NumberNode || node is IntNode)
            {
                // Literals require no semantic checks
            }
            else if (node is PunctuationNode punctuationNode)
            {
                if (punctuationNode.Punctuation != ";")
                {
                    throw new Exception($"Semantic Error: Unexpected punctuation '{punctuationNode.Punctuation}'. Expected ';'.");
                }
            }
            else
            {
                throw new Exception($"Semantic Error: Unsupported AST node type '{node.GetType().Name}'.");
            }
        }

        private string InferType(ASTNode node)
        {
            if (node is VariableNode variableNode)
            {
                if (_symbolTable.IsVariableDeclared(variableNode.Name))
                {
                    return _symbolTable.GetVariableType(variableNode.Name);
                }
                throw new Exception($"Semantic Error: Variable '{variableNode.Name}' is not declared.");
            }
            if (node is IntNode)
            {
                return "int";
            }
            if (node is NumberNode)
            {
                return "float";
            }
            if (node is StringNode)
            {
                return "string";
            }
            if (node is BinaryOperationNode binaryNode)
            {
                string leftType = InferType(binaryNode.Left);
                string rightType = InferType(binaryNode.Right);

                // Handle comparison operators
                if (binaryNode.Operator == "==" || binaryNode.Operator == "!=" ||
                    binaryNode.Operator == "<" || binaryNode.Operator == "<=" ||
                    binaryNode.Operator == ">" || binaryNode.Operator == ">=")
                {
                    if ((leftType == "int" || leftType == "float") &&
                        (rightType == "int" || rightType == "float"))
                    {
                        return "bool"; // Comparison operators always return bool
                    }
                    throw new Exception($"Semantic Error: Incompatible types for comparison operator '{binaryNode.Operator}'.");
                }

                // Handle arithmetic operators
                if (binaryNode.Operator == "+" || binaryNode.Operator == "-" ||
                    binaryNode.Operator == "*" || binaryNode.Operator == "/")
                {
                    if (leftType == "int" && rightType == "int")
                    {
                        return "int";
                    }
                    if ((leftType == "int" || leftType == "float") &&
                        (rightType == "int" || rightType == "float"))
                    {
                        return "float";
                    }
                    throw new Exception($"Semantic Error: Incompatible types for arithmetic operator '{binaryNode.Operator}'.");
                }

                throw new Exception($"Semantic Error: Unsupported binary operator '{binaryNode.Operator}'.");
            }

            throw new Exception($"Semantic Error: Unable to infer type for node '{node.GetType().Name}'.");
        }

        private bool IsNumeric(ASTNode node)
        {
            string type = InferType(node);
            return type == "int" || type == "float";
        }

        public void PrintSymbolTable()
        {
            _symbolTable.PrintSymbolTable();
        }
    }
}