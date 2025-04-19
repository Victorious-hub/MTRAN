namespace MTRAN.Semantic
{
    public class SemanticAnalyzer
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();
        private readonly List<string> _errors = new List<string>();

        public void Analyze(ASTNode node)
        {
            if (node is ProgramNode programNode)
            {
                _symbolTable.EnterScope("Block");
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
                    AddError($"Semantic Error: Variable '{declarationNode.Variable}' is already declared.");
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
                    // Implicitly declare the variable if it is not declared
                    string inferredType = assignmentNode.Value != null ? InferType(assignmentNode.Value) : "unknown";
                    _symbolTable.DeclareVariable(assignmentNode.Variable, inferredType);
                    Console.WriteLine($"[DEBUG] Implicitly declared variable '{assignmentNode.Variable}' with type '{inferredType}'.");
                }

                // Analyze the value being assigned
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
                if (variableNode.Name == "self")
                {
                    // Treat '$self' as an implicit variable
                    if (!_symbolTable.IsVariableDeclared("self"))
                    {
                        _symbolTable.DeclareVariable("self", "object");
                        Console.WriteLine($"[DEBUG] Declared variable '$self' of type 'object' in the current scope.");
                    }
                }
                else if (variableNode.Name == "class")
                {
                    // Treat '$class' as an implicit variable representing a class name
                    if (!_symbolTable.IsVariableDeclared("class"))
                    {
                        _symbolTable.DeclareVariable("class", "class");
                        Console.WriteLine($"[DEBUG] Declared variable '$class' of type 'class' in the current scope.");
                    }
                }
                else if (variableNode.Name == "shift")
                {
                    // Treat 'shift' as a built-in function
                    Console.WriteLine($"[DEBUG] Treated 'shift' as a built-in function.");
                }
                else if (!_symbolTable.IsVariableDeclared(variableNode.Name))
                {
                    AddError($"Semantic Error: Variable '{variableNode.Name}' is not declared.");
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
                    AddError($"Semantic Error: Unexpected punctuation '{punctuationNode.Punctuation}'. Expected ';'.");
                }
            }
            else if (node is PrintStatementNode printStatementNode)
            {
                foreach (var argument in printStatementNode.Arguments)
                {
                    Analyze(argument);
                }
            }
            else if (node is ForNode forNode)
            {
                _symbolTable.EnterScope("For");
                try
                {
                    if (forNode.Initialization != null)
                    {
                        Analyze(forNode.Initialization);
                    }

                    if (forNode.Condition != null)
                    {
                        if (!IsNumeric(forNode.Condition))
                        {
                            AddError($"Semantic Error: The condition in the 'for' loop must be numeric.");
                        }
                        Analyze(forNode.Condition);
                    }

                    if (forNode.Increment != null)
                    {
                        Analyze(forNode.Increment);
                    }

                    if (forNode.Body != null)
                    {
                        Analyze(forNode.Body);
                    }
                }
                finally
                {
                    _symbolTable.ExitScope();
                }
            }
            else if (node is StatementListNode statementListNode)
            {
                foreach (var statement in statementListNode.Statements)
                {
                    Analyze(statement);
                }
            }
            else if (node is UnaryOperationNode unaryOperationNode)
            {
                if (!_symbolTable.IsVariableDeclared(unaryOperationNode.Variable))
                {
                    AddError($"Semantic Error: Variable '{unaryOperationNode.Variable}' is not declared.");
                }
                else
                {
                    string variableType = _symbolTable.GetVariableType(unaryOperationNode.Variable);

                    if (unaryOperationNode.Operator != "++" && unaryOperationNode.Operator != "--")
                    {
                        AddError($"Semantic Error: Unsupported unary operator '{unaryOperationNode.Operator}'.");
                    }
                    else if (variableType != "int" && variableType != "float" && variableType != "bool")
                    {
                        AddError($"Semantic Error: Unary operator '{unaryOperationNode.Operator}' can only be applied to variables of type 'int' or 'float', but '{unaryOperationNode.Variable}' is of type '{variableType}'.");
                    }
                }
            }
            else if (node is WhileNode whileNode)
            {
                if (whileNode.Condition != null)
                {
                    if (!IsNumeric(whileNode.Condition) && InferType(whileNode.Condition) != "bool")
                    {
                        AddError($"Semantic Error: The condition in the 'while' loop must be numeric or boolean.");
                    }
                    Analyze(whileNode.Condition);
                }

                _symbolTable.EnterScope("While");
                try
                {
                    if (whileNode.Body != null)
                    {
                        Analyze(whileNode.Body);
                    }
                }
                finally
                {
                    _symbolTable.ExitScope();
                }
            }
            else if (node is ArrayNode arrayNode)
            {
                foreach (var element in arrayNode.Elements)
                {
                    if (element is PunctuationNode punctuationNode1 && punctuationNode1.Punctuation == ",")
                    {
                        continue;
                    }
                    Analyze(element);
                }
            }
            else if (node is FunctionNode functionNode)
            {
                if (_symbolTable.IsFunctionDeclared(functionNode.Name))
                {
                    AddError($"Semantic Error: Function '{functionNode.Name}' is already declared.");
                }
                else
                {
                    _symbolTable.DeclareFunction(functionNode.Name, functionNode.Parameters.Count);
                }

                _symbolTable.EnterScope($"Function-{functionNode.Name}");
                try
                {
                    foreach (var parameter in functionNode.Parameters)
                    {
                        if (parameter is VariableNode variableNode1)
                        {
                            _symbolTable.DeclareVariable(variableNode1.Name, "unknown");
                        }
                    }

                    Analyze(functionNode.Body);
                }
                finally
                {
                    _symbolTable.ExitScope();
                }
            }
            else if (node is FunctionCallNode functionCallNode)
            {
                if (!_symbolTable.IsFunctionDeclared(functionCallNode.FunctionName))
                {
                    AddError($"Semantic Error: Function '{functionCallNode.FunctionName}' is not declared.");
                }
                else
                {
                    int declaredParameterCount = _symbolTable.GetFunctionParameterCount(functionCallNode.FunctionName);
                    int providedArgumentCount = functionCallNode.Arguments.Count;

                    if (declaredParameterCount != providedArgumentCount)
                    {
                        AddError($"Semantic Error: Function '{functionCallNode.FunctionName}' expects {declaredParameterCount} arguments but {providedArgumentCount} were provided.");
                    }

                    foreach (var argument in functionCallNode.Arguments)
                    {
                        Analyze(argument);
                    }
                }
            }
            else if (node is ReturnNode returnNode)
            {
                if (returnNode.Value != null)
                {
                    Analyze(returnNode.Value);
                }
            }
            else if (node is ForeachNode foreachNode)
            {
                Analyze(foreachNode.Array);

                _symbolTable.EnterScope("Foreach");
                try
                {
                    if (foreachNode.VariableDeclaration is VariableDeclarationNode variableDeclaration)
                    {
                        _symbolTable.DeclareVariable(variableDeclaration.Variable, "unknown");
                    }

                    Analyze(foreachNode.Body);
                }
                finally
                {
                    _symbolTable.ExitScope();
                }
            }
            else if (node is ClassNode classNode)
            {
                if (_symbolTable.IsClassDeclared(classNode.ClassName))
                {
                    AddError($"Semantic Error: Class '{classNode.ClassName}' is already declared.");
                }
                else
                {
                    _symbolTable.DeclareClass(classNode.ClassName);
                }

                _symbolTable.EnterScope($"Class-{classNode.ClassName}");
                try
                {
                    foreach (var method in classNode.Methods)
                    {
                        Analyze(method);

                        // Check if the method is a constructor
                        if (method is FunctionNode functionNode1 && functionNode1.Name == "func")
                        {
                            if (!IsConstructor(functionNode1, classNode.ClassName))
                            {
                                AddError($"Semantic Error: Method '{functionNode1.Name}' in class '{classNode.ClassName}' is not a valid constructor.");
                            }
                        }
                    }
                }
                finally
                {
                    _symbolTable.ExitScope();
                }
            }
            else if (node is HashNode hashNode)
            {
                foreach (var element in hashNode.Elements)
                {
                    Analyze(element.Key);
                    Analyze(element.Value);
                }
            }
            else if (node is BlessNode blessNode)
            {
                Analyze(blessNode.Object);
                Analyze(blessNode.Class);

                if (blessNode.Class is VariableNode classVariableNode)
                {
                    // Check if '$class' is declared as a class
                    // if (!_symbolTable.IsClassDeclared(classVariableNode.Name))
                    // {
                    //     AddError($"Semantic Error: Class '{classVariableNode.Name}' is not declared.");
                    // }
                }
                else
                {
                    AddError($"Semantic Error: Invalid class reference in 'bless' statement.");
                }
            }
            else if (node is ObjectNode objectNode)
            {
                foreach (var (key, value) in objectNode.Properties)
                {
                    // Skip declaration checks for keys (they are literals, not variables)
                    if (key is VariableNode keyVariableNode)
                    {
                        // Treat keys as literals, no need to declare them
                        Analyze(value); // Analyze the value
                    }
                    else
                    {
                        Analyze(key);   // Analyze the key
                        Analyze(value); // Analyze the value
                    }
                }
            }
            else
            {
                AddError($"Semantic Error: Unsupported AST node type '{node.GetType().Name}'.");
            }

        }

        private string InferType(ASTNode node)
        {
            try 
            {
                if (node is VariableNode variableNode)
                {
                    if (variableNode.Name == "shift")
                    {
                        // Treat 'shift' as a built-in function and return its type
                        return "function";
                    }
                    if (variableNode.Name == "class")
                    {
                        // Treat '$class' as a variable representing a class name
                        if (_symbolTable.IsVariableDeclared("class"))
                        {
                            return _symbolTable.GetVariableType("class");
                        }
                        AddError($"Semantic Error: Variable '{variableNode.Name}' is not declared.");
                        return "unknown";
                    }
                    if (_symbolTable.IsVariableDeclared(variableNode.Name))
                    {
                        return _symbolTable.GetVariableType(variableNode.Name);
                    }
                    AddError($"Semantic Error: Variable '{variableNode.Name}' is not declared.");
                    return "unknown";
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
                if (node is ParenthesizedExpression parenthesizedExpression)
                {
                    return InferType(parenthesizedExpression.Expression);
                }
                if (node is BinaryOperationNode binaryNode)
                {
                    string leftType = InferType(binaryNode.Left);
                    string rightType = InferType(binaryNode.Right);

                    if (binaryNode.Operator == "&&" || binaryNode.Operator == "||")
                    {
                        if (leftType == "bool" && rightType == "bool")
                        {
                            return "bool";
                        }
                        AddError($"Semantic Error: Logical operator '{binaryNode.Operator}' requires boolean operands.");
                    }

                    if (binaryNode.Operator == "==" || binaryNode.Operator == "!=" ||
                        binaryNode.Operator == "<" || binaryNode.Operator == "<=" ||
                        binaryNode.Operator == ">" || binaryNode.Operator == ">=" ||
                        binaryNode.Operator == "^" || binaryNode.Operator == "||")
                    {
                        if ((leftType == "int" || leftType == "float") &&
                            (rightType == "int" || rightType == "float"))
                        {
                            return "bool";
                        }
                        AddError($"Semantic Error: Incompatible types for comparison operator '{binaryNode.Operator}'.");
                    }

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
                        AddError($"Semantic Error: Incompatible types for arithmetic operator '{binaryNode.Operator}'.");
                    }

                    AddError($"Semantic Error: Unsupported binary operator '{binaryNode.Operator}'.");
                }
                if (node is ObjectNode objectNode)
                {
                    foreach (var (key, value) in objectNode.Properties)
                    {
                        // Skip type inference for keys (they are literals)
                        InferType(value); // Infer the type of the value
                    }
                    return "object";
                }
                if (node is ArrayNode arrayNode)
                {
                    if (arrayNode.Elements.Count == 0)
                    {
                        return "array<unknown>";
                    }

                    string firstElementType = InferType(arrayNode.Elements[0]);
                    // foreach (var element in arrayNode.Elements)
                    // {
                    //     if (InferType(element) != firstElementType)
                    //     {
                    //         AddError("Semantic Error: Array elements must have the same type.");
                    //         return "array<unknown>";
                    //     }
                    // }

                    return $"array<{firstElementType}>";
                }
                if (node is FunctionCallNode functionCallNode)
                {
                    if (!_symbolTable.IsFunctionDeclared(functionCallNode.FunctionName))
                    {
                        AddError($"Semantic Error: Function '{functionCallNode.FunctionName}' is not declared.");
                        return "unknown";
                    }

                    foreach (var argument in functionCallNode.Arguments)
                    {
                        Analyze(argument);
                    }
                    return "function";
                }
                if (node is HashNode hashNode)
                {
                    if (hashNode.Elements.Count == 0)
                    {
                        return "hash<unknown, unknown>";
                    }

                    string keyType = InferType(hashNode.Elements[0].Key);
                    string valueType = InferType(hashNode.Elements[0].Value);

                    // foreach (var element in hashNode.Elements)
                    // {
                    //     if (InferType(element.Key) != keyType || InferType(element.Value) != valueType)
                    //     {
                    //         AddError("Semantic Error: Hash keys and values must have consistent types.");
                    //         return "hash<unknown, unknown>";
                    //     }
                    // }

                    return $"hash<{keyType}, {valueType}>";
                }

                AddError($"Semantic Error: Unable to infer type for node '{node.GetType().Name}'.");
                return "unknown";
            }

            catch (Exception ex)
            {
                AddError(ex.Message);
                return "unknown";
            }
        }

       private bool IsConstructor(FunctionNode functionNode, string className)
        {
            bool initializesSelf = false;
            bool blessesSelf = false;

            // Ensure the body is a BlockNode to iterate over statements
            if (functionNode.Body is BlockNode blockNode)
            {
                foreach (var statement in blockNode.Statements)
                {
                    if (statement is AssignmentNode assignmentNode &&
                        assignmentNode.Variable == "self" &&
                        assignmentNode.Value is ObjectNode)
                    {
                        initializesSelf = true;
                    }

                    if (statement is BlessNode blessNode &&
                        blessNode.Object is VariableNode variableNode &&
                        variableNode.Name == "self" &&
                        blessNode.Class is VariableNode classVariableNode &&
                        classVariableNode.Name == className)
                    {
                        blessesSelf = true;
                    }
                }
            }
            else
            {
                AddError($"Semantic Error: Constructor '{functionNode.Name}' does not have a valid body.");
            }

            return initializesSelf && blessesSelf;
        }

        private bool IsNumeric(ASTNode node)
        {
            string type = InferType(node);
            return type == "int" || type == "float";
        }

        public void PrintSymbolTable()
        {
            _symbolTable.PrintAllScopes();       // Print active scopes
            _symbolTable.PrintReservedScopes(); // Print reserved (exited) scopes
        }

        private void AddError(string errorMessage)
        {
            _errors.Add(errorMessage);
        }

        public void ReportErrors()
        {
            if (_errors.Count > 0)
            {
                foreach (var error in _errors)
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
    }
}