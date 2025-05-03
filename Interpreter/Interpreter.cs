namespace MTRAN.Interpreter
{
    public class InterpreterAnalyzer
    {
        private readonly SymbolTable _symbolTable = new SymbolTable();
        private readonly List<string> _errors = new List<string>();
        private readonly Dictionary<string, FunctionNode> _functionTable = new Dictionary<string, FunctionNode>();
        private Dictionary<string, object> _variableValues = new Dictionary<string, object>();

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
                if (assignmentNode.Value is HashAccessNode hashAccessNode)
                {
                    Analyze(hashAccessNode);
                }
                else
                {
                    if (!_symbolTable.IsVariableDeclared(assignmentNode.Variable))
                    {
                        string inferredType = assignmentNode.Value != null ? InferType(assignmentNode.Value) : "unknown";
                        _symbolTable.DeclareVariable(assignmentNode.Variable, inferredType);
                        Console.WriteLine($"[DEBUG] Implicitly declared variable '{assignmentNode.Variable}' with type '{inferredType}'.");
                    }

                    Analyze(assignmentNode.Value);
                }
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
            else if (node is CompoundAssignmentNode compoundAssignmentNode)
            {
                Console.WriteLine($"[DEBUG] Analyzing compound assignment '{compoundAssignmentNode.OperatorSymbol}' for variable '{compoundAssignmentNode.Variable}'.");

                // Check if the variable is declared
                if (!_symbolTable.IsVariableDeclared(compoundAssignmentNode.Variable))
                {
                    AddError($"Semantic Error: Variable '{compoundAssignmentNode.Variable}' is not declared.");
                    return;
                }

                // Infer the type of the variable and the value
                string variableType = _symbolTable.GetVariableType(compoundAssignmentNode.Variable);
                string valueType = InferType(compoundAssignmentNode.Value);

                // Ensure the operator is valid for the types
                if ((variableType == "int" || variableType == "float") && (valueType == "int" || valueType == "float"))
                {
                    // Valid for numeric types
                }
                else
                {
                    AddError($"Semantic Error: Operator '{compoundAssignmentNode.OperatorSymbol}' is not valid for types '{variableType}' and '{valueType}'.");
                }

                // Analyze the value expression
                Analyze(compoundAssignmentNode.Value);
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
                    // Infer the return type of the function
                    string returnType = "void";
                    if (functionNode.Body is BlockNode blockNode1)
                    {
                        foreach (var statement in blockNode1.Statements)
                        {
                            if (statement is ReturnNode returnNode && returnNode.Value != null)
                            {
                                string inferredReturnType = InferType(returnNode.Value);
                                if (returnType == "void")
                                {
                                    returnType = inferredReturnType;
                                }
                                else if (returnType != inferredReturnType)
                                {
                                    AddError($"Semantic Error: Function '{functionNode.Name}' has inconsistent return types: '{returnType}' and '{inferredReturnType}'.");
                                }
                            }
                        }
                    }

                    // Declare the function with its parameter count and return type
                    var parameterTypes = functionNode.Parameters.Select(_ => "unknown").ToList();
                    _symbolTable.DeclareFunction(functionNode.Name, parameterTypes, returnType);
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
            else if (node is UseNode useNode)
            {
                // Analyze the value being assigned to the constant
                Analyze(useNode.Value);

                // Declare the constant in the symbol table
                if (!_symbolTable.IsVariableDeclared(useNode.ConstantName))
                {
                    _symbolTable.DeclareVariable(useNode.ConstantName, InferType(useNode.Value));
                }
                else
                {
                    AddError($"Semantic Error: Constant '{useNode.ConstantName}' is already declared.");
                }
            }
            else if (node is ArrayAccessNode arrayAccessNode)
            {
                // Check if the array is declared
                string arrayName = "@" + arrayAccessNode.ArrayName.Substring(1); // Convert $a to @a
                if (!_symbolTable.IsVariableDeclared(arrayName))
                {
                    AddError($"Semantic Error: Array '{arrayName}' is not declared.");
                }
                else
                {
                    // Analyze the index expression
                    Analyze(arrayAccessNode.Index);

                    // Ensure the index is numeric
                    if (!IsNumeric(arrayAccessNode.Index))
                    {
                        AddError($"Semantic Error: Index for array '{arrayName}' must be numeric.");
                    }
                }
            }
            else if (node is HashNode hashNode)
            {
                string hashName = hashNode.Name; // Assuming HashNode has a Name property

                if (!_symbolTable.IsVariableDeclared(hashName))
                {
                    // Infer the types of the keys and values
                    string keyType = hashNode.Elements.Count > 0 ? InferType(hashNode.Elements[0].Key) : "unknown";
                    string valueType = hashNode.Elements.Count > 0 ? InferType(hashNode.Elements[0].Value) : "unknown";

                    // Declare the hash variable in the symbol table
                    _symbolTable.DeclareVariable(hashName, $"hash<{keyType}, {valueType}>");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] Variable '{hashName}' is already declared. Skipping declaration.");
                }

                // Analyze the elements of the hash
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
            else if (node is HashElementAssignmentNode hashElementAssignmentNode)
            {
                string hashName = hashElementAssignmentNode.HashName;

                if (!_symbolTable.IsVariableDeclared(hashName))
                {
                    AddError($"Semantic Error: Variable '{hashName}' is not declared.");
                }
                else
                {
                    string hashType = _symbolTable.GetVariableType(hashName);
                    if (!hashType.StartsWith("hash<"))
                    {
                        AddError($"Semantic Error: '{hashName}' is not a hash.");
                    }
                    else
                    {
                        // Analyze the key and value
                        Analyze(hashElementAssignmentNode.Key);
                        Analyze(hashElementAssignmentNode.Value);

                        // Ensure the key type matches the hash's key type
                        string expectedKeyType = hashType.Split('<', ',')[1].Trim();
                        string actualKeyType = InferType(hashElementAssignmentNode.Key);
                        if (expectedKeyType != "unknown" && expectedKeyType != actualKeyType)
                        {
                            AddError($"Semantic Error: Key type mismatch for hash '{hashName}'. Expected '{expectedKeyType}', got '{actualKeyType}'.");
                        }

                        // Ensure the value type matches the hash's value type
                        string expectedValueType = hashType.Split(',')[1].TrimEnd('>');
                        string actualValueType = InferType(hashElementAssignmentNode.Value);
                        if (expectedValueType != "unknown" && expectedValueType != actualValueType)
                        {
                            AddError($"Semantic Error: Value type mismatch for hash '{hashName}'. Expected '{expectedValueType}', got '{actualValueType}'.");
                        }
                    }
                }
            }
            else if (node is HashAccessNode hashAccessNode1)
            {
                string hashName = hashAccessNode1.HashName;
                Console.WriteLine($"[DEBUG] Analyzing hash access for '{hashName}'.");
                // Convert scalar reference ($person) to hash reference (%person)
                if (hashName.StartsWith("$"))
                {
                    string potentialHashName = "%" + hashName.Substring(1);
                    if (_symbolTable.IsVariableDeclared(potentialHashName))
                    {
                        hashName = potentialHashName;
                    }
                }

                if (!_symbolTable.IsVariableDeclared(hashName))
                {
                    AddError($"Semantic Error: Hash '{hashName}' is not declared.");
                }
                else
                {
                    string hashType = _symbolTable.GetVariableType(hashName);
                    if (!hashType.StartsWith("hash<"))
                    {
                        AddError($"Semantic Error: '{hashName}' is not a hash.");
                    }
                    else
                    {
                        // Analyze the key
                        Analyze(hashAccessNode1.Key);

                        // Ensure the key type matches the hash's key type
                        string expectedKeyType = hashType.Split('<', ',')[1].Trim();
                        string actualKeyType = InferType(hashAccessNode1.Key);
                        if (expectedKeyType != "unknown" && expectedKeyType != actualKeyType)
                        {
                            AddError($"Semantic Error: Key type mismatch for hash '{hashName}'. Expected '{expectedKeyType}', got '{actualKeyType}'.");
                        }
                    }
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
                if (node is UseNode useNode)
                {
                    // Infer the type of the value being assigned
                    return InferType(useNode.Value);
                }
                if (node is ArrayAccessNode arrayAccessNode)
                {
                    // Infer the type of the array element
                    string arrayName = "@" + arrayAccessNode.ArrayName.Substring(1); // Convert $a to @a
                    string arrayType = InferType(new VariableNode(arrayName));
                    if (arrayType.StartsWith("array<") && arrayType.EndsWith(">"))
                    {
                        return arrayType.Substring(6, arrayType.Length - 7); // Extract the element type
                    }
                    AddError($"Semantic Error: '{arrayName}' is not an array.");
                    return "unknown";
                }
                if (node is HashAccessNode hashAccessNode)
                {
                    string hashName = hashAccessNode.HashName;

                    // Convert scalar reference ($person) to hash reference (%person)
                    if (hashName.StartsWith("$"))
                    {
                        hashName = "%" + hashName.Substring(1);
                    }

                    if (!_symbolTable.IsVariableDeclared(hashName))
                    {
                        AddError($"Semantic Error: Variable '{hashName}' is not declared.");
                        return "unknown";
                    }

                    string hashType = _symbolTable.GetVariableType(hashName);
                    if (!hashType.StartsWith("hash<"))
                    {
                        AddError($"Semantic Error: '{hashName}' is not a hash.");
                        return "unknown";
                    }

                    // Return the value type of the hash
                    return hashType.Split(',')[1].TrimEnd('>');
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

                    var functionInfo = _symbolTable.GetFunctionInfo(functionCallNode.FunctionName);
                    int declaredParameterCount = functionInfo.ParameterTypes.Count;
                    int providedArgumentCount = functionCallNode.Arguments.Count;

                    if (declaredParameterCount != providedArgumentCount)
                    {
                        AddError($"Semantic Error: Function '{functionCallNode.FunctionName}' expects {declaredParameterCount} arguments but {providedArgumentCount} were provided.");
                    }
                    else
                    {
                        for (int i = 0; i < declaredParameterCount; i++)
                        {
                            string expectedType = functionInfo.ParameterTypes[i];
                            string actualType = InferType(functionCallNode.Arguments[i]);

                            if (expectedType != "unknown" && expectedType != actualType)
                            {
                                AddError($"Semantic Error: Argument {i + 1} of function '{functionCallNode.FunctionName}' expects type '{expectedType}' but got '{actualType}'.");
                            }
                        }
                    }

                    return functionInfo.ReturnType; // Return the function's return type
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


        public void Interpret(ASTNode node)
        {
            if (node is HashNode hashNode)
            {
                string hashName = hashNode.Name;

                if (_variableValues.ContainsKey(hashName))
                {
                    AddError($"Interpretation Error: Hash '{hashName}' is already declared.");
                }
                else
                {
                    var hash = new Dictionary<string, object>();
                    foreach (var (key, value) in hashNode.Elements)
                    {
                        object evaluatedKey = Evaluate(key);
                        object evaluatedValue = Evaluate(value);

                        if (evaluatedKey is string keyString)
                        {
                            hash[keyString] = evaluatedValue;
                        }
                        else
                        {
                            AddError($"Interpretation Error: Hash key '{evaluatedKey}' must be a string.");
                        }
                    }

                    _variableValues[hashName] = hash;
                    Console.WriteLine($"[INTERPRET] {hashName} = {string.Join(", ", hash.Select(kv => $"{kv.Key} => {kv.Value}"))}");
                }
            }
            if (node is IfNode ifNode)
            {
                Console.WriteLine($"[DEBUG] Interpreting IfNode... {ifNode.Condition}");
                object conditionValue = Evaluate(ifNode.Condition);
                Console.WriteLine($"[DEBUG] Condition evaluated to: {conditionValue}");

                if (conditionValue is bool condition && condition)
                {
                    Console.WriteLine("[DEBUG] Executing ThenBranch...");
                    Interpret(ifNode.ThenBranch);
                }
                else
                {
                    bool executed = false;

                    foreach (var elseIfBranch in ifNode.ElseIfBranches)
                    {
                        object elseIfConditionValue = Evaluate(elseIfBranch.Condition);
                        Console.WriteLine($"[DEBUG] ElseIf condition evaluated to: {elseIfConditionValue}, {elseIfBranch.Condition}");

                        if (elseIfConditionValue is bool elseIfCondition && elseIfCondition)
                        {
                            Console.WriteLine("[DEBUG] Executing ElseIfBranch...");
                            Interpret(elseIfBranch.ThenBranch);
                            executed = true;
                            break;
                        }
                    }

                    if (!executed && ifNode.ElseBranch != null)
                    {
                        Console.WriteLine("[DEBUG] Executing ElseBranch...");
                        Interpret(ifNode.ElseBranch);
                    }
                }
            }
            else if (node is CompoundAssignmentNode compoundAssignmentNode)
            {
                Console.WriteLine($"[DEBUG] Interpreting compound assignment '{compoundAssignmentNode.OperatorSymbol}' for variable '{compoundAssignmentNode.Variable}'.");

                // Check if the variable exists
                if (!_variableValues.ContainsKey(compoundAssignmentNode.Variable))
                {
                    AddError($"Interpretation Error: Variable '{compoundAssignmentNode.Variable}' is not initialized.");
                    return;
                }

                // Get the current value of the variable
                object leftValue = _variableValues[compoundAssignmentNode.Variable];

                // Evaluate the right-hand side value
                object rightValue = Evaluate(compoundAssignmentNode.Value);

                // Perform the compound assignment operation
                object result = PerformBinaryOperation(compoundAssignmentNode.OperatorSymbol, leftValue, rightValue, compoundAssignmentNode.Variable);

                // Update the variable's value
                _variableValues[compoundAssignmentNode.Variable] = result;
                Console.WriteLine($"[INTERPRET] {compoundAssignmentNode.Variable} {compoundAssignmentNode.OperatorSymbol} {rightValue} = {result}");
            }
            else if (node is VariableDeclarationNode declarationNode && declarationNode.Variable.StartsWith("%"))
            {
                string hashName = declarationNode.Variable;

                Console.WriteLine($"[DEBUG] Declaring hash '{hashName}' with keyword '{declarationNode.Keyword}'.");

                var hashValues = new Dictionary<string, object>();
                if (declarationNode.Value is HashNode hashNod1)
                {
                    foreach (var (key, value) in hashNod1.Elements)
                    {
                        object evaluatedKey = Evaluate(key);
                        object evaluatedValue = Evaluate(value);

                        if (evaluatedKey is string keyString)
                        {
                            hashValues[keyString] = evaluatedValue;
                        }
                        else
                        {
                            AddError($"Interpretation Error: Hash key '{evaluatedKey}' must be a string.");
                        }
                    }
                }

                if (_variableValues.ContainsKey(hashName))
                {
                    AddError($"Interpretation Error: Hash '{hashName}' is already declared.");
                }
                else
                {
                    _variableValues[hashName] = hashValues;
                    Console.WriteLine($"[INTERPRET] {hashName} = {string.Join(", ", hashValues.Select(kv => $"{kv.Key} => {kv.Value}"))}");
                }
            }
            else if (node is VariableDeclarationNode declarationNode1 && declarationNode1.Variable.StartsWith("@"))
            {
                string arrayName = declarationNode1.Variable;

                Console.WriteLine($"[DEBUG] Declaring array '{arrayName}' with keyword '{declarationNode1.Keyword}'.");

                List<object> arrayValues = new List<object>();
                if (declarationNode1.Value is ArrayNode arrayNode)
                {
                    foreach (var element in arrayNode.Elements)
                    {
                        object evaluatedElement = Evaluate(element);
                        if (evaluatedElement != null) // Filter out null or invalid elements
                        {
                            arrayValues.Add(evaluatedElement);
                        }
                    }
                }

                if (_variableValues.ContainsKey(arrayName))
                {
                    AddError($"Interpretation Error: Array '{arrayName}' is already declared.");
                }
                else
                {
                    _variableValues[arrayName] = arrayValues;
                    Console.WriteLine($"[INTERPRET] {arrayName} = {string.Join(", ", arrayValues)}");
                }
            }
            else if (node is VariableDeclarationNode declarationNode2 && declarationNode2.Variable.StartsWith("$"))
            {
                string variableName = declarationNode2.Variable;

                Console.WriteLine($"[DEBUG] Declaring variable '{variableName}' with keyword '{declarationNode2.Keyword}'.");

                object value = null;
                if (declarationNode2.Value != null)
                {
                    value = Evaluate(declarationNode2.Value);
                }

                if (_variableValues.ContainsKey(variableName))
                {
                    AddError($"Interpretation Error: Variable '{variableName}' is already declared.");
                }
                else
                {
                    _variableValues[variableName] = value;
                    Console.WriteLine($"[INTERPRET] {variableName} = {value}");
                }
            }
            if (node is AssignmentNode assignmentNode)
            {
                Console.WriteLine($"[DEBUG] Interpreting assignment for variable '{assignmentNode.Variable}'.");

                object value = Evaluate(assignmentNode.Value);
                Console.WriteLine($"[DEBUG] Evaluated value for '{assignmentNode.Variable}' is {value}.");

                _variableValues[assignmentNode.Variable] = value;
                Console.WriteLine($"[INTERPRET] {assignmentNode.Variable} = {value}");
            }
            else if (node is ParenthesizedExpression parenthesizedExpression)
            {
                Console.WriteLine("[DEBUG] Interpreting ParenthesizedExpression...");
                Interpret(parenthesizedExpression.Expression); // Interpret the inner expression
            }
            else if (node is FunctionNode functionNode)
            {
                Console.WriteLine($"[DEBUG] Declaring function '{functionNode.Name}'.");

                if (_functionTable.ContainsKey(functionNode.Name))
                {
                    AddError($"Interpretation Error: Function '{functionNode.Name}' is already declared.");
                }
                else
                {
                    _functionTable[functionNode.Name] = functionNode;
                }
            }
            else if (node is FunctionCallNode functionCallNode)
            {
                Console.WriteLine($"[DEBUG] Calling function '{functionCallNode.FunctionName}'.");

                if (!_functionTable.ContainsKey(functionCallNode.FunctionName))
                {
                    AddError($"Interpretation Error: Function '{functionCallNode.FunctionName}' is not declared.");
                    return;
                }

                var function = _functionTable[functionCallNode.FunctionName];
                if (function.Parameters.Count != functionCallNode.Arguments.Count)
                {
                    AddError($"Interpretation Error: Function '{functionCallNode.FunctionName}' expects {function.Parameters.Count} arguments but got {functionCallNode.Arguments.Count}.");
                    return;
                }

                // Save the current variable values to restore later
                var previousVariableValues = new Dictionary<string, object>(_variableValues);

                // Assign arguments to parameters
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    if (function.Parameters[i] is VariableNode parameterNode)
                    {
                        object argumentValue = Evaluate(functionCallNode.Arguments[i]);
                        _variableValues[parameterNode.Name] = argumentValue;
                        Console.WriteLine($"[DEBUG] Parameter '{parameterNode.Name}' = {argumentValue}");
                    }
                }

                // Interpret the function body
                object returnValue = null;
                try
                {
                    returnValue = InterpretFunctionBody(function.Body);
                }
                finally
                {
                    // Restore the previous variable values
                    _variableValues = previousVariableValues;
                }

                Console.WriteLine($"[DEBUG] Function '{functionCallNode.FunctionName}' returned: {returnValue}");
            }
            else if (node is ForNode forNode)
            {
                Console.WriteLine("[DEBUG] Interpreting ForNode...");

                if (forNode.Initialization != null)
                {
                    Interpret(forNode.Initialization);
                }

                while (true)
                {
                    object conditionValue = Evaluate(forNode.Condition);
                    Console.WriteLine($"[DEBUG] ForNode condition evaluated to: {conditionValue}");

                    if (conditionValue is bool condition && !condition)
                    {
                        break;
                    }

                    Interpret(forNode.Body);

                    if (forNode.Increment != null)
                    {
                        Interpret(forNode.Increment);
                    }
                }
            }
            else if (node is WhileNode whileNode)
            {
                Console.WriteLine("[DEBUG] Interpreting WhileNode...");

                while (true)
                {
                    object conditionValue = Evaluate(whileNode.Condition);
                    Console.WriteLine($"[DEBUG] WhileNode condition evaluated to: {conditionValue}");

                    if (conditionValue is bool condition && !condition)
                    {
                        break;
                    }

                    Interpret(whileNode.Body);
                }
            }
            else if (node is PrintStatementNode printStatementNode1)
            {
                foreach (var argument in printStatementNode1.Arguments)
                {
                    object value = Evaluate(argument);
                    Console.WriteLine(value);
                }
            }
            else if (node is PrintStatementNode printStatementNode)
            {
                foreach (var argument in printStatementNode.Arguments)
                {
                    object value = Evaluate(argument);
                    if (value != null)
                    {
                        Console.WriteLine(value);
                    }
                    else
                    {
                        AddError("Interpretation Error: Unable to evaluate argument in print statement.");
                    }
                }
            }
            else if (node is BinaryOperationNode binaryNode)
            {
                object leftValue = Evaluate(binaryNode.Left);
                object rightValue = Evaluate(binaryNode.Right);

                object result = PerformBinaryOperation(binaryNode.Operator, leftValue, rightValue);
                Console.WriteLine($"[INTERPRET] {leftValue} {binaryNode.Operator} {rightValue} = {result}");
            }
            else if (node is UnaryOperationNode unaryNode)
            {
                object result = Evaluate(unaryNode); // Use Evaluate to handle the UnaryOperationNode
                Console.WriteLine($"[INTERPRET] {unaryNode.Variable} {unaryNode.Operator} = {result}");
            }
            else if (node is StatementListNode statementListNode)
            {
                foreach (var statement in statementListNode.Statements)
                {
                    Interpret(statement);
                }
            }
            else if (node is BlockNode blockNode)
            {
                foreach (var statement in blockNode.Statements)
                {
                    Interpret(statement);
                }
            }
            else if (node is ForeachNode foreachNode)
            {
                Console.WriteLine("[DEBUG] Interpreting ForeachNode...");

                object arrayValue = Evaluate(foreachNode.Array);
                if (arrayValue is List<object> array)
                {
                    foreach (var element in array)
                    {
                        if (foreachNode.VariableDeclaration is VariableDeclarationNode variableDeclaration)
                        {
                            string variableName = variableDeclaration.Variable;
                            _variableValues[variableName] = element;
                            Console.WriteLine($"[DEBUG] Iterating with {variableName} = {element}");
                        }

                        Interpret(foreachNode.Body);
                    }
                }
                else
                {
                    AddError("Interpretation Error: The foreach loop requires an array to iterate over.");
                }
            }
            else if (node is WhileNode whileNode1)
            {
                Console.WriteLine("[DEBUG] Interpreting WhileNode...");

                while (true)
                {
                    object conditionValue = Evaluate(whileNode1.Condition);
                    Console.WriteLine($"[DEBUG] WhileNode condition evaluated to: {conditionValue}");

                    if (conditionValue is bool condition && condition)
                    {
                        Interpret(whileNode1.Body);
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] WhileNode condition is false, exiting loop...");
                        break;
                    }
                }
            }
            else if (node is ForNode forNode1)
            {
                Console.WriteLine("[DEBUG] Interpreting ForNode...");

                // Execute the initialization
                if (forNode1.Initialization != null)
                {
                    Interpret(forNode1.Initialization);
                }

                while (true)
                {
                    // Evaluate the condition
                    if (forNode1.Condition == null)
                    {
                        AddError("Interpretation Error: ForNode condition is null.");
                        break;
                    }

                    object conditionValue = Evaluate(forNode1.Condition);
                    Console.WriteLine($"[DEBUG] ForNode condition evaluated to: {conditionValue}");

                    if (conditionValue is bool condition && !condition)
                    {
                        break;
                    }

                    // Execute the body of the loop
                    if (forNode1.Body != null)
                    {
                        Interpret(forNode1.Body);
                    }
                    else
                    {
                        AddError("Interpretation Error: ForNode body is null.");
                        break;
                    }

                    // Execute the increment
                    if (forNode1.Increment != null)
                    {
                        Interpret(forNode1.Increment);
                    }
                    else
                    {
                        AddError("Interpretation Error: ForNode increment is null.");
                        break;
                    }
                }
            }
            else
            {
                AddError($"Interpretation Error: Unsupported AST node type '{node.GetType().Name}'.");
            }
        }


        private object InterpretFunctionBody(ASTNode body)
        {
            if (body is BlockNode blockNode)
            {
                foreach (var statement in blockNode.Statements)
                {
                    if (statement is ReturnNode returnNode)
                    {
                        return Evaluate(returnNode.Value);
                    }
                    Interpret(statement);
                }
            }
            else if (body is ReturnNode returnNode)
            {
                return Evaluate(returnNode.Value);
            }
            else
            {
                Interpret(body);
            }

            return null; // Default return value if no ReturnNode is encountered
        }

        private string InterpolateString(string input)
        {
            var result = new System.Text.StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                if (input[i] == '$' && i + 1 < input.Length && (char.IsLetter(input[i + 1]) || input[i + 1] == '_'))
                {
                    int start = i;
                    i++;
                    while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                    {
                        i++;
                    }
                    string variableName = input.Substring(start, i - start);

                    if (_variableValues.ContainsKey(variableName))
                    {
                        result.Append(_variableValues[variableName]?.ToString() ?? "null");
                    }
                    else
                    {
                        AddError($"Interpretation Error: Variable '{variableName}' is not declared.");
                        result.Append($"${variableName}");
                    }
                }
                else if (input[i] == '\\' && i + 1 < input.Length)
                {
                    i++;
                    result.Append(input[i] switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        '\\' => '\\',
                        '"' => '"',
                        _ => input[i]
                    });
                }
                else
                {
                    result.Append(input[i]);
                }
                i++;
            }

            return result.ToString() + "\"";
        }

        private object Evaluate(ASTNode node)
        {
            if (node is ParenthesizedExpression parenthesizedExpression)
            {
                Console.WriteLine("[DEBUG] Evaluating ParenthesizedExpression...");
                return Evaluate(parenthesizedExpression.Expression);
            }
            if (node is IntNode intNode)
            {
                return intNode.Value;
            }
            else if (node is HashAccessNode hashAccessNode)
            {
                string hashName = hashAccessNode.HashName;

                // Convert scalar reference ($person) to hash reference (%person)
                if (hashName.StartsWith("$"))
                {
                    hashName = "%" + hashName.Substring(1);
                }

                if (_variableValues.ContainsKey(hashName) && _variableValues[hashName] is Dictionary<string, object> hash)
                {
                    object key = Evaluate(hashAccessNode.Key);
                    if (key is string keyString && hash.ContainsKey(keyString))
                    {
                        return hash[keyString];
                    }
                    else
                    {
                        AddError($"Interpretation Error: Key '{key}' not found in hash '{hashName}'.");
                        return null;
                    }
                }
                else
                {
                    AddError($"Interpretation Error: Hash '{hashName}' is not declared or is not a hash.");
                    return null;
                }
            }
            else if (node is NumberNode numberNode)
            {
                return numberNode.Value;
            }
            else if (node is ArrayAccessNode arrayAccessNode)
            {
                // Convert $a to @a for array access
                string arrayName = "@" + arrayAccessNode.ArrayName.Substring(1); 
                if (_variableValues.ContainsKey(arrayName) && _variableValues[arrayName] is List<object> array)
                {
                    object indexValue = Evaluate(arrayAccessNode.Index);
                    if (indexValue is float index && index >= 0 && index < array.Count)
                    {
                        return array[(int)index]; // Return the accessed element
                    }
                    else
                    {
                        AddError($"Interpretation Error: Index '{indexValue}' is out of bounds for array '{arrayName}'.");
                        return null;
                    }
                }
                else
                {
                    AddError($"Interpretation Error: Array '{arrayName}' is not declared or is not an array.");
                    return null;
                }
            }
            else if (node is UnaryOperationNode unaryOperationNode)
            {
                Console.WriteLine($"[DEBUG] Evaluating unary operation '{unaryOperationNode.Operator}' on variable '{unaryOperationNode.Variable}'.");
                return PerformUnaryOperation(unaryOperationNode.Operator, unaryOperationNode.Variable);
            }
            else if (node is FunctionCallNode functionCallNode)
            {
                Console.WriteLine($"[DEBUG] Evaluating function call '{functionCallNode.FunctionName}'.");

                if (!_functionTable.ContainsKey(functionCallNode.FunctionName))
                {
                    AddError($"Interpretation Error: Function '{functionCallNode.FunctionName}' is not declared.");
                    return null;
                }

                var function = _functionTable[functionCallNode.FunctionName];
                if (function.Parameters.Count != functionCallNode.Arguments.Count)
                {
                    AddError($"Interpretation Error: Function '{functionCallNode.FunctionName}' expects {function.Parameters.Count} arguments but got {functionCallNode.Arguments.Count}.");
                    return null;
                }

                // Save the current variable values to restore later
                var previousVariableValues = new Dictionary<string, object>(_variableValues);

                // Assign arguments to parameters
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    if (function.Parameters[i] is VariableNode parameterNode)
                    {
                        object argumentValue = Evaluate(functionCallNode.Arguments[i]);
                        _variableValues[parameterNode.Name] = argumentValue;
                        Console.WriteLine($"[DEBUG] Parameter '{parameterNode.Name}' = {argumentValue}");
                    }
                }

                // Interpret the function body
                object returnValue = null;
                try
                {
                    returnValue = InterpretFunctionBody(function.Body);
                }
                finally
                {
                    // Restore the previous variable values
                    _variableValues = previousVariableValues;
                }

                return returnValue;
            }
            else if (node is VariableNode variableNode)
            {
                if (_variableValues.ContainsKey(variableNode.Name))
                {
                    return _variableValues[variableNode.Name];
                }
                else
                {
                    AddError($"Interpretation Error: Variable '{variableNode.Name}' is not initialized.");
                    return null;
                }
            }
            else if (node is BinaryOperationNode binaryNode)
            {
                object leftValue = Evaluate(binaryNode.Left);
                object rightValue = Evaluate(binaryNode.Right);
                return PerformBinaryOperation(binaryNode.Operator, leftValue, rightValue);
            }
            else if (node is StringNode stringNode)
            {
                return InterpolateString(stringNode.Value);
            }
            else if (node is IfNode ifNode)
            {
                object conditionValue = Evaluate(ifNode.Condition);
                Console.WriteLine($"[DEBUG] IfNode condition evaluated to: {conditionValue}");

                if (conditionValue is bool condition && condition)
                {
                    Console.WriteLine("[DEBUG] Executing IfNode ThenBranch...");
                    return Evaluate(ifNode.ThenBranch);
                }
                else
                {
                    foreach (var elseIfBranch in ifNode.ElseIfBranches)
                    {
                        object elseIfConditionValue = Evaluate(elseIfBranch.Condition);
                        Console.WriteLine($"[DEBUG] ElseIfNode condition evaluated to: {elseIfConditionValue}");

                        if (elseIfConditionValue is bool elseIfCondition && elseIfCondition)
                        {
                            Console.WriteLine("[DEBUG] Executing ElseIfNode ThenBranch...");
                            return Evaluate(elseIfBranch.ThenBranch);
                        }
                    }

                    if (ifNode.ElseBranch != null)
                    {
                        Console.WriteLine("[DEBUG] Executing IfNode ElseBranch...");
                        return Evaluate(ifNode.ElseBranch);
                    }
                }

                return null;
            }
            else if (node is ElseIfNode elseIfNode)
            {
                object conditionValue = Evaluate(elseIfNode.Condition);
                if (conditionValue is bool condition && condition)
                {
                    return Evaluate(elseIfNode.ThenBranch);
                }
                return null;
            }
            else if (node is ElseNode elseNode)
            {
                return Evaluate(elseNode.ThenBranch);
            }
            else if (node is ArrayNode arrayNode)
            {
                var array = new List<object>();
                foreach (var element in arrayNode.Elements)
                {
                    array.Add(Evaluate(element));
                }
                return array;
            }
            else
            {
                AddError($"Interpretation Error: Unable to evaluate node '{node.GetType().Name}'.");
                return null;
            }
        }


        private object PerformUnaryOperation(string operatorSymbol, string variableName)
        {
            if (!_variableValues.ContainsKey(variableName))
            {
                AddError($"Interpretation Error: Variable '{variableName}' is not initialized.");
                return null;
            }

            object currentValue = _variableValues[variableName];

            if (currentValue == null)
            {
                AddError($"Interpretation Error: Variable '{variableName}' has a null value.");
                return null;
            }

            if (currentValue is int intValue)
            {
                switch (operatorSymbol)
                {
                    case "++":
                        _variableValues[variableName] = intValue + 1;
                        return intValue + 1;
                    case "--":
                        _variableValues[variableName] = intValue - 1;
                        return intValue - 1;
                    default:
                        AddError($"Interpretation Error: Unsupported unary operator '{operatorSymbol}'.");
                        return null;
                }
            }
            else if (currentValue is float floatValue)
            {
                switch (operatorSymbol)
                {
                    case "++":
                        _variableValues[variableName] = floatValue + 1;
                        return floatValue + 1;
                    case "--":
                        _variableValues[variableName] = floatValue - 1;
                        return floatValue - 1;
                    default:
                        AddError($"Interpretation Error: Unsupported unary operator '{operatorSymbol}'.");
                        return null;
                }
            }
            else
            {
                AddError($"Interpretation Error: Unary operator '{operatorSymbol}' can only be applied to numeric types.");
                return null;
            }
        }

        private object PerformBinaryOperation(string operatorSymbol, object left, object right, string variableName = null)
        {
            if (left is int leftInt && right is int rightInt)
            {
                switch (operatorSymbol)
                {
                    case "+":
                        return leftInt + rightInt;
                    case "-":
                        return leftInt - rightInt;
                    case "*":
                        return leftInt * rightInt;
                    case "/":
                        if (rightInt == 0) throw new DivideByZeroException();
                        return leftInt / rightInt;
                    case "==":
                        return leftInt == rightInt;
                    case "!=":
                        return leftInt != rightInt;
                    case "<":
                        return leftInt < rightInt;
                    case ">":
                        return leftInt > rightInt;
                    case "<=":
                        return leftInt <= rightInt;
                    case ">=":
                        return leftInt >= rightInt;
                    case "+=":
                        if (variableName != null) _variableValues[variableName] = leftInt + rightInt;
                        return leftInt + rightInt;
                    case "-=":
                        if (variableName != null) _variableValues[variableName] = leftInt - rightInt;
                        return leftInt - rightInt;
                    case "*=":
                        if (variableName != null) _variableValues[variableName] = leftInt * rightInt;
                        return leftInt * rightInt;
                    case "/=":
                        if (rightInt == 0) throw new DivideByZeroException();
                        if (variableName != null) _variableValues[variableName] = leftInt / rightInt;
                        return leftInt / rightInt;
                    default:
                        throw new InvalidOperationException($"Unsupported operator '{operatorSymbol}' for integers.");
                }
            }
            else if (left is float leftFloat && right is float rightFloat)
            {
                switch (operatorSymbol)
                {
                    case "+":
                        return leftFloat + rightFloat;
                    case "-":
                        return leftFloat - rightFloat;
                    case "*":
                        return leftFloat * rightFloat;
                    case "/":
                        if (rightFloat == 0) throw new DivideByZeroException();
                        return leftFloat / rightFloat;
                    case "==":
                        return Math.Abs(leftFloat - rightFloat) < 1e-6; // Handle floating-point equality
                    case "!=":
                        return Math.Abs(leftFloat - rightFloat) >= 1e-6;
                    case "<":
                        return leftFloat < rightFloat;
                    case ">":
                        return leftFloat > rightFloat;
                    case "<=":
                        return leftFloat <= rightFloat;
                    case ">=":
                        return leftFloat >= rightFloat;
                    case "+=":
                        if (variableName != null) _variableValues[variableName] = leftFloat + rightFloat;
                        return leftFloat + rightFloat;
                    case "-=":
                        if (variableName != null) _variableValues[variableName] = leftFloat - rightFloat;
                        return leftFloat - rightFloat;
                    case "*=":
                        if (variableName != null) _variableValues[variableName] = leftFloat * rightFloat;
                        return leftFloat * rightFloat;
                    case "/=":
                        if (rightFloat == 0) throw new DivideByZeroException();
                        if (variableName != null) _variableValues[variableName] = leftFloat / rightFloat;
                        return leftFloat / rightFloat;
                    default:
                        throw new InvalidOperationException($"Unsupported operator '{operatorSymbol}' for floats.");
                }
            }
            else
            {
                AddError($"Interpretation Error: Unsupported operand types for operator '{operatorSymbol}'.");
                return null;
            }
        }

        // public void PrintVariableValues()
        // {
        //     Console.WriteLine("[INTERPRET] Variable Values:");
        //     foreach (var (variable, value) in _variableValues)
        //     {
        //         Console.WriteLine($"  {variable} = {value}");
        //     }
        // }
    }
}