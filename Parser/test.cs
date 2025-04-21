// using System;
// using System.Collections.Generic;

// namespace MTRAN.Parser
// {
//     public class Parser
//     {
//         private readonly PerlLexer _lexer;
//         private Token _currentToken;
//         private int _currentTokenIndex;
//         private Stack<PerlToken> _parenStack = new Stack<PerlToken>();

//         public Parser(PerlLexer lexer)
//         {
//             _lexer = lexer;
//             _currentTokenIndex = 0;
//             _currentToken = _lexer.tokens[_currentTokenIndex];
//         }

//         private void LogError(string message)
//         {
//             string errorFilePath = "/home/shyskov/Documents/libs/sem6/MTRAN/Parser/parse_errors.log";
//             string errorMessage = $"{DateTime.Now}: {message} at line {_currentToken.Line}.";
//             Console.WriteLine(errorMessage);
//             using (StreamWriter writer = new StreamWriter(errorFilePath, append: true))
//             {
//                 writer.WriteLine(errorMessage);
//             }
//         }

//         private ASTNode Eat(PerlToken tokenType)
//         {
//             if (_currentToken.TokenType == tokenType)
//             {
//                 // Track parentheses
//                 if (tokenType == PerlToken.LPAREN)
//                 {
//                     _parenStack.Push(tokenType);
//                 }
//                 else if (tokenType == PerlToken.RPAREN)
//                 {
//                     if (_parenStack.Count == 0 || _parenStack.Peek() != PerlToken.LPAREN)
//                     {
//                         string errorMessage = $"Unmatched closing parenthesis at index {_currentTokenIndex}";
//                         LogError(errorMessage);
//                         return new ErrorNode(errorMessage); // Return an ErrorNode for unmatched closing parenthesis
//                     }
//                     _parenStack.Pop();
//                 }

//                 _currentTokenIndex++;
//                 if (_currentTokenIndex < _lexer.tokens.Count)
//                 {
//                     _currentToken = _lexer.tokens[_currentTokenIndex];
//                 }
//                 return null; // No error
//             }
//             else
//             {
//                 string errorMessage = $"Expected token {tokenType}, got {_currentToken.TokenType} at index {_currentTokenIndex}";
//                 LogError(errorMessage);
//                 return new ErrorNode(errorMessage); // Return an ErrorNode for unexpected token
//             }
//         }

//         private ASTNode Factor()
//         {
//             Token token = _currentToken;

//             if (token.TokenType == PerlToken.NUMBER)
//             {
//                 ASTNode error = Eat(PerlToken.NUMBER);
//                 if (error != null) return error;
//                 return new NumberNode(float.Parse(token.Lexeme));
//             }
//             else if (token.TokenType == PerlToken.INT)
//             {
//                 ASTNode error = Eat(PerlToken.INT);
//                 if (error != null) return error;
//                 return new NumberNode(int.Parse(token.Lexeme));
//             }
//             else if (token.TokenType == PerlToken.IDENT)
//             {
//                 string functionName = token.Lexeme;
//                 ASTNode error = Eat(PerlToken.IDENT);
//                 if (error != null) return error;

//                 if (_currentToken.TokenType == PerlToken.LPAREN)
//                 {
//                     ASTNode error1 = Eat(PerlToken.LPAREN);
//                     if (error1 != null) return error1;
//                     List<ASTNode> arguments = new List<ASTNode>();
//                     while (_currentToken.TokenType != PerlToken.RPAREN)
//                     {
//                         arguments.Add(Expr());
//                         if (_currentToken.TokenType == PerlToken.COMMA)
//                         {
//                             ASTNode error2 = Eat(PerlToken.COMMA);
//                             if (error2 != null) return error2;
//                         }
//                     }
//                     ASTNode error3 = Eat(PerlToken.RPAREN);
//                     if (error3 != null) return error3;
//                     return new FunctionCallNode(functionName, arguments);
//                 }

//                 // Handle compound assignment operators
//                 if (_currentToken.TokenType == PerlToken.ADD_ASSIGN || _currentToken.TokenType == PerlToken.SUB_ASSIGN ||
//                     _currentToken.TokenType == PerlToken.MUL_ASSIGN || _currentToken.TokenType == PerlToken.DIV_ASSIGN)
//                 {
//                     Token operatorToken = _currentToken;
//                     ASTNode error3 = Eat(operatorToken.TokenType);
//                     if (error3 != null) return error3;
//                     ASTNode value = Expr();
//                     return new CompoundAssignmentNode(token.Lexeme, operatorToken.Lexeme, value);
//                 }
//                 else if (_currentToken.TokenType == PerlToken.ASSIGN)
//                 {
//                     ASTNode error3 = Eat(PerlToken.ASSIGN);
//                     if (error3 != null) return error3;
//                     ASTNode value = Expr();
//                     return new AssignmentNode(token.Lexeme, value);
//                 }
//                 else if (_currentToken.TokenType == PerlToken.INC)
//                 {
//                     ASTNode error3 = Eat(PerlToken.INC);
//                     if (error3 != null) return error3;
                    
//                     return new UnaryOperationNode(token.Lexeme, "++");
//                 }
//                 else if (_currentToken.TokenType == PerlToken.DEC)
//                 {
//                     ASTNode error3 = Eat(PerlToken.DEC);
//                     if (error3 != null) return error3;

//                     return new UnaryOperationNode(token.Lexeme, "--");
//                 }

//                 return new VariableNode(token.Lexeme);
//             }
//             else if (token.TokenType == PerlToken.STRING)
//             {
//                 ASTNode error3 = Eat(PerlToken.STRING);
//                 if (error3 != null) return error3;

//                 // Eat(PerlToken.STRING);
//                 return new StringNode(token.Lexeme);
//             }
//             else if (_currentToken.TokenType == PerlToken.SHIFT)
//             {
//                 ASTNode error3 = Eat(PerlToken.SHIFT);
//                 if (error3 != null) return error3;

//                 // Eat(PerlToken.SHIFT);
//                 return new KeywordNode("shift");
//             }
//             else if (_currentToken.TokenType == PerlToken.BLESS)
//             {
//                 ASTNode error3 = Eat(PerlToken.BLESS);
//                 if (error3 != null) return error3;

//                 // Eat(PerlToken.BLESS);

//                 // Parse the object being blessed
//                 ASTNode obj = Factor(); // This will now handle ObjectNode

//                 // Optionally, parse the class name (if provided)
//                 ASTNode cls = null;
//                 if (_currentToken.TokenType == PerlToken.COMMA)
//                 {
//                     ASTNode error33 = Eat(PerlToken.COMMA);
//                     if (error33 != null) return error33;

//                     // Eat(PerlToken.COMMA);
//                     cls = Expr();
//                 }

//                 return new BlessNode(obj, cls);
//             }
//             else if (_currentToken.TokenType == PerlToken.NEW)
//             {
//                 ASTNode error33 = Eat(PerlToken.NEW);
//                 if (error33 != null) return error33;

//                 // Eat(PerlToken.NEW);
//                 return new KeywordNode("new");
//             }
//             else if (token.TokenType == PerlToken.LPAREN)
//             {
//                 ASTNode error33 = Eat(PerlToken.LPAREN);
//                 if (error33 != null) return error33;

//                 // Eat(PerlToken.LPAREN);
//                 ASTNode node = Expr();
//                 ASTNode error333 = Eat(PerlToken.RPAREN);
//                 if (error333 != null) return error333;
//                 // Eat(PerlToken.RPAREN);
//                 return node;
//             }
//             else if (token.TokenType == PerlToken.SUB)
//             {
//                 return FunctionDeclaration();
//             }
//             else if (token.TokenType == PerlToken.IDENT && token.Lexeme.StartsWith("@"))
//             {
//                 ASTNode error333 = Eat(PerlToken.IDENT);
//                 if (error333 != null) return error333;

//                 // Eat(PerlToken.IDENT);
//                 return new VariableNode(token.Lexeme);
//             }
//             else if (token.TokenType == PerlToken.MY)
//             {
//                 return Assignment();
//             }
//             else if (_currentToken.TokenType == PerlToken.LBRACE)
//             {
//                 ASTNode error333 = Eat(PerlToken.LBRACE);
//                 if (error333 != null) return error333;

//                 // Eat(PerlToken.LBRACE);
//                 List<(ASTNode Key, ASTNode Value)> properties = new List<(ASTNode Key, ASTNode Value)>();

//                 while (_currentToken.TokenType != PerlToken.RBRACE)
//                 {
//                     // Parse the key
//                     ASTNode key = Expr();

//                     // Expect '=>'
//                     if (_currentToken.TokenType == PerlToken.HASH_ASSIGN)
//                     {
//                         ASTNode error3333 = Eat(PerlToken.HASH_ASSIGN);
//                         if (error3333 != null) return error3333;

//                         // Eat(PerlToken.HASH_ASSIGN);
//                     }
//                     else
//                     {
//                         throw new Exception($"Expected '=>' for object property assignment, got {_currentToken.TokenType} at line {_currentToken.Line}.");
//                     }

//                     // Parse the value
//                     ASTNode value = Expr();
//                     properties.Add((key, value));

//                     // Handle commas between properties
//                     if (_currentToken.TokenType == PerlToken.COMMA)
//                     {
//                         ASTNode error3333 = Eat(PerlToken.COMMA);
//                         if (error3333 != null) return error3333;

//                         // Eat(PerlToken.COMMA);
//                     }
//                 }

//                 ASTNode error33333 = Eat(PerlToken.RBRACE); 
//                 if (error33333 != null) return error33333;

//                 // Eat(PerlToken.RBRACE); // Consume the closing brace
//                 return new ObjectNode("self", properties); // Return an ObjectNode
//             }
//             else
//             {
//                 string errorMessage = $"Invalid factor: unexpected token '{token.TokenType}' at line {_currentToken.Line}.";
//                 LogError(errorMessage);
//                 throw new Exception(errorMessage);
//             }
//         }

//         private ASTNode Term()
//         {
//             ASTNode node = Factor();

//             while (_currentToken.TokenType == PerlToken.MUL || _currentToken.TokenType == PerlToken.DIV)
//             {
//                 Token token = _currentToken;
//                 if (token.TokenType == PerlToken.MUL)
//                 {
//                     Eat(PerlToken.MUL);
//                 }
//                 else if (token.TokenType == PerlToken.DIV)
//                 {
//                     Eat(PerlToken.DIV);
//                 }

//                 node = new BinaryOperationNode(node, token.Lexeme, Factor());
//             }

//             return node;
//         }

//         private ASTNode Expr()
//         {
//             return ParseLogicalOr();
//         }

//         private ASTNode ParseLogicalOr()
//         {
//             ASTNode node = ParseLogicalAnd();

//             while (_currentToken.TokenType == PerlToken.LOR || _currentToken.TokenType == PerlToken.OR)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, ParseLogicalAnd());
//             }

//             return node;
//         }

//         private ASTNode ParseLogicalAnd()
//         {
//             ASTNode node = ParseEquality();

//             while (_currentToken.TokenType == PerlToken.LAND || _currentToken.TokenType == PerlToken.AND)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, ParseEquality());
//             }

//             return node;
//         }

//         private ASTNode ParseEquality()
//         {
//             ASTNode node = ParseBitwiseXor();

//             while (_currentToken.TokenType == PerlToken.EQUAL || _currentToken.TokenType == PerlToken.NOT_EQUAL)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, ParseBitwiseXor());
//             }

//             return node;
//         }

//         private ASTNode ParseBitwiseXor()
//         {
//             ASTNode node = ParseRelational();

//             while (_currentToken.TokenType == PerlToken.BITWISE_XOR || _currentToken.TokenType == PerlToken.XOR)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, ParseRelational());
//             }

//             return node;
//         }

//         private ASTNode ParseRelational()
//         {
//             ASTNode node = ParseAdditive();

//             while (_currentToken.TokenType == PerlToken.LESS || _currentToken.TokenType == PerlToken.GRT ||
//                 _currentToken.TokenType == PerlToken.LESS_OR_EQUAL || _currentToken.TokenType == PerlToken.GRT_OR_EQUAL)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, ParseAdditive());
//             }

//             return node;
//         }

//         private ASTNode ParseAdditive()
//         {
//             ASTNode node = ParseMultiplicative();

//             while (_currentToken.TokenType == PerlToken.ADD || _currentToken.TokenType == PerlToken.SUB)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, ParseMultiplicative());
//             }

//             return node;
//         }

//         private ASTNode ParseMultiplicative()
//         {
//             ASTNode node = Factor();

//             while (_currentToken.TokenType == PerlToken.MUL || _currentToken.TokenType == PerlToken.DIV || _currentToken.TokenType == PerlToken.MOD)
//             {
//                 Token token = _currentToken;
//                 ASTNode error33333 = Eat(token.TokenType);
//                 if (error33333 != null) return error33333;

//                 // Eat(token.TokenType);
//                 node = new BinaryOperationNode(node, token.Lexeme, Factor());
//             }

//             return node;
//         }

//         private ASTNode IfStatement()
//         {
//             ASTNode error = Eat(PerlToken.IF);
//             if (error != null) return error;

//             // Eat(PerlToken.IF);

//             ASTNode error1 = Eat(PerlToken.LPAREN);
//             if (error1 != null) return error1;

//             // Eat(PerlToken.LPAREN);
//             ASTNode condition = Expr();
//             ASTNode error2 = Eat(PerlToken.RPAREN);
//             if (error2 != null) return error2;

//             // Eat(PerlToken.RPAREN);
//             ASTNode error3 = Eat(PerlToken.LBRACE);
//             if (error3 != null) return error3;

//             // Eat(PerlToken.LBRACE);
//             ASTNode thenBranch = ParseBlock();
//             ASTNode error4 = Eat(PerlToken.RBRACE);
//             if (error4 != null) return error4;

//             // Eat(PerlToken.RBRACE);

//             List<ElseIfNode> elseIfBranches = new List<ElseIfNode>();
//             while (_currentToken.TokenType == PerlToken.ELSIF)
//             {
//                 ASTNode error5 = Eat(PerlToken.ELSIF);
//                 if (error5 != null) return error5;

//                 // Eat(PerlToken.ELSIF);
//                 ASTNode error6 = Eat(PerlToken.LPAREN);
//                 if (error6 != null) return error6;

//                 // Eat(PerlToken.LPAREN);
//                 ASTNode elseIfCondition = Expr();
//                 ASTNode error7 = Eat(PerlToken.RPAREN);
//                 if (error7 != null) return error7;

//                 // Eat(PerlToken.RPAREN);
//                 ASTNode error8 = Eat(PerlToken.LBRACE);
//                 if (error8 != null) return error8;

//                 // Eat(PerlToken.LBRACE);
//                 ASTNode elseIfThenBranch = ParseBlock();
//                 ASTNode error9 = Eat(PerlToken.RBRACE);
//                 if (error9 != null) return error9;

//                 // Eat(PerlToken.RBRACE);
//                 elseIfBranches.Add(new ElseIfNode(elseIfCondition, elseIfThenBranch));
//             }

//             ASTNode elseBranch = null;
//             if (_currentToken.TokenType == PerlToken.ELSE)
//             {
//                 ASTNode error9 = Eat(PerlToken.ELSE);
//                 if (error9 != null) return error9;

//                 // Eat(PerlToken.ELSE);
//                 ASTNode error10 = Eat(PerlToken.LBRACE);
//                 if (error10 != null) return error10;

//                 // Eat(PerlToken.LBRACE);
//                 elseBranch = ParseBlock();
//                 ASTNode error11 = Eat(PerlToken.RBRACE);
//                 if (error11 != null) return error11;

//                 // Eat(PerlToken.RBRACE);
//             }

//             return new IfNode(new ParenthesizedExpression(condition), thenBranch, elseIfBranches, elseBranch);
//         }

//         private ASTNode ParseBlock()
//         {
//             List<ASTNode> statements = new List<ASTNode>();

//             while (_currentToken.TokenType != PerlToken.RBRACE && _currentToken.TokenType != PerlToken.EOF_)
//             {
//                 ASTNode node = ParseStatement();
//                 if (node != null)
//                 {
//                     statements.Add(node);
//                 }
//                 Console.WriteLine(_currentToken.TokenType);
//                 if (_currentToken.TokenType == PerlToken.SEMICOLON)
//                 {
//                     statements.Add(new PunctuationNode(";"));
//                     ASTNode error11 = Eat(PerlToken.SEMICOLON);
//                     if (error11 != null) return error11;

//                     // Eat(PerlToken.SEMICOLON);
//                 }
//                 else if (_currentToken.TokenType == PerlToken.LPAREN)
//                 {
//                     statements.Add(new PunctuationNode("("));
//                     ASTNode error11 = Eat(PerlToken.LPAREN);
//                     if (error11 != null) return error11;

//                     // Eat(PerlToken.LPAREN);
//                 }
//                 else if (_currentToken.TokenType == PerlToken.RPAREN)
//                 {
//                     statements.Add(new PunctuationNode(")"));
//                     ASTNode error11 = Eat(PerlToken.RPAREN);
//                     if (error11 != null) return error11;

//                     // Eat(PerlToken.RPAREN);
//                 }
//                 else if (_currentToken.TokenType != PerlToken.RBRACE && _currentToken.TokenType != PerlToken.EOF_)
//                 {
//                     // Allow certain tokens to be present after a statement
//                     if (_currentToken.TokenType == PerlToken.MY || _currentToken.TokenType == PerlToken.IF || 
//                         _currentToken.TokenType == PerlToken.FOR || _currentToken.TokenType == PerlToken.FOREACH || 
//                         _currentToken.TokenType == PerlToken.WHILE || _currentToken.TokenType == PerlToken.ELSIF || 
//                         _currentToken.TokenType == PerlToken.ELSE)
//                     {
//                         continue;
//                     }
//                     // throw new Exception("Unexpected token after statement");
//                 }
//                 // else
//                 // {
//                 //     LogError("Unclosed block: missing '}'");
//                 // }
//             }

//             return new BlockNode(statements);
//         }

//         private ASTNode ParseStatement()
//         {
//             if (_currentToken.TokenType == PerlToken.IF)
//             {
//                 return IfStatement();
//             }
//             else if (_currentToken.TokenType == PerlToken.FOR)
//             {
//                 return ForStatement();
//             }
//             else if (_currentToken.TokenType == PerlToken.FOREACH)
//             {
//                 return ForeachStatement();
//             }
//             else if (_currentToken.TokenType == PerlToken.WHILE)
//             {
//                 return WhileStatement();
//             }
//             else if (_currentToken.TokenType == PerlToken.MY)
//             {
//                 return Assignment();
//             }
//             else if (_currentToken.TokenType == PerlToken.SUB)
//             {
//                 return FunctionDeclaration();
//             }
//             else if (_currentToken.TokenType == PerlToken.RETURN)
//             {
//                 return ReturnStatement();
//             }
//             else if (_currentToken.TokenType == PerlToken.PRINT)
//             {
//                 return PrintStatement();
//             }
//             else if (_currentToken.TokenType == PerlToken.PACKAGE)
//             {
//                 return ClassDeclaration();
//             }
//             else
//             {
//                 return Expr();
//             }
//         }

//         private ASTNode ClassDeclaration()
//         {
//             ASTNode error11 = Eat(PerlToken.PACKAGE);
//             if (error11 != null) return error11;
//             // Eat(PerlToken.PACKAGE);

//             // Parse class name
//             if (_currentToken.TokenType != PerlToken.IDENT)
//             {
//                 string errorMessage = $"Expected class name, got '{_currentToken.TokenType}' at line {_currentToken.Line}.";
//                 LogError(errorMessage);
//                 throw new Exception($"Expected class name, got {_currentToken.TokenType}");
//             }
//             string className = _currentToken.Lexeme;
//             ASTNode error111 = Eat(PerlToken.IDENT);
//             if (error111 != null) return error111;

//             // Eat(PerlToken.IDENT);

//             List<ASTNode> methods = new List<ASTNode>();

//             // Parse methods inside the class
//             while (_currentToken.TokenType == PerlToken.SUB)
//             {
//                 methods.Add(ClassFunctionDeclaration());
//             }

//             return new ClassNode(className, methods);
//         }

//         private ASTNode PrintStatement()
//         {
//             ASTNode error111 = Eat(PerlToken.PRINT);
//             if (error111 != null) return error111;
//             // Eat(PerlToken.PRINT);

//             List<ASTNode> arguments = new List<ASTNode>();
//             while (_currentToken.TokenType != PerlToken.SEMICOLON && _currentToken.TokenType != PerlToken.EOF_)
//             {
//                 arguments.Add(Expr());
//                 if (_currentToken.TokenType == PerlToken.COMMA)
//                 {
//                     ASTNode error1111 = Eat(PerlToken.COMMA);
//                     if (error1111 != null) return error1111;

//                     // Eat(PerlToken.COMMA);
//                 }
//             }

//             return new PrintStatementNode(arguments);
//         }

//         private ASTNode ReturnStatement()
//         {
//             ASTNode error1111 = Eat(PerlToken.RETURN);
//             if (error1111 != null) return error1111;

//             // Eat(PerlToken.RETURN);
//             ASTNode value = Expr();
//             return new ReturnNode(value);
//         }

//         public ASTNode Parse()
//         {
//             ASTNode root = ParseBlock();

//             if (_parenStack.Count > 0)
//             {
//                 // throw new Exception("Unclosed parenthesis detected.");
//             }

//             return root;
//         }

//         private ASTNode Assignment()
//         {
//             bool isDeclaration = false;
//             string? keyword = null;

//             // Check if the statement starts with "my"
//             if (_currentToken.TokenType == PerlToken.MY)
//             {
//                 ASTNode error1111 = Eat(PerlToken.MY);
//                 if (error1111 != null) return error1111;

//                 // Eat(PerlToken.MY);
//                 isDeclaration = true;
//                 keyword = "my";
//             }

//             if (_currentToken.TokenType == PerlToken.LPAREN)
//             {
//                 // Handle parentheses for grouped assignments (e.g., my ($a, $b) = @_;)
//                 ASTNode error1111 = Eat(PerlToken.LPAREN);
//                 if (error1111 != null) return error1111;
//                 // Eat(PerlToken.LPAREN);
//                 List<ASTNode> variables = new List<ASTNode>();
//                 while (_currentToken.TokenType != PerlToken.RPAREN)
//                 {
//                     if (_currentToken.TokenType == PerlToken.IDENT)
//                     {
//                         string variableName = _currentToken.Lexeme;
//                         ASTNode error11111 = Eat(PerlToken.IDENT);
//                         if (error11111 != null) return error11111;

//                         // Eat(PerlToken.IDENT);
//                         variables.Add(new VariableNode(variableName));
//                     }
//                     if (_currentToken.TokenType == PerlToken.COMMA)
//                     {
//                         ASTNode error11111 = Eat(PerlToken.COMMA);
//                         if (error11111 != null) return error11111;

//                         // Eat(PerlToken.COMMA);
//                     }
//                 }
//                 ASTNode error111111 = Eat(PerlToken.RPAREN);
//                 if (error111111 != null) return error111111;

//                 // Eat(PerlToken.RPAREN);

//                 if (_currentToken.TokenType == PerlToken.ASSIGN)
//                 {
//                     ASTNode error1111111 = Eat(PerlToken.ASSIGN);
//                     if (error1111111 != null) return error1111111;

//                     // Eat(PerlToken.ASSIGN);
//                     ASTNode value = Expr();
//                     return new GroupedAssignmentNode(variables, value);
//                 }
//                 else
//                 {
//                     throw new Exception($"Expected token ASSIGN, got {_currentToken.TokenType}");
//                 }
//             }

//             if (_currentToken.TokenType == PerlToken.ILLEGAL)
//             {
//                 string errorMessage = $"Invalid variable name '{_currentToken.Lexeme}' at line {_currentToken.Line}. Variable name must not start from number";
//                 LogError(errorMessage);
//                 throw new Exception(errorMessage);
//             }

//             if (_currentToken.TokenType == PerlToken.IDENT)
//             {
//                 string variableName = _currentToken.Lexeme;

//                 // Validate variable name starts with $, @, or %
//                 if (!variableName.StartsWith("$") && !variableName.StartsWith("@") && !variableName.StartsWith("%"))
//                 {
//                     string errorMessage = $"Invalid variable name '{variableName}' at line {_currentToken.Line}. Variable names must start with $, @, or %.";
//                     LogError(errorMessage);
//                     throw new Exception(errorMessage);
//                 }
//             }

            
//             if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme.StartsWith("@"))
//             {
//                 Token arrayToken = _currentToken;
//                 ASTNode error1111111 = Eat(PerlToken.IDENT);
//                 if (error1111111 != null) return error1111111;

//                 // Eat(PerlToken.IDENT);

//                 if (_currentToken.TokenType == PerlToken.ASSIGN)
//                 {
//                     ASTNode error11111111 = Eat(PerlToken.ASSIGN);
//                     if (error11111111 != null) return error11111111;

//                     // Eat(PerlToken.ASSIGN);
//                     ASTNode error1111111112 = Eat(PerlToken.LPAREN);
//                     if (error1111111112 != null) return error1111111112;

//                     // Eat(PerlToken.LPAREN);
//                     List<ASTNode> elements = new List<ASTNode>();
//                     while (_currentToken.TokenType != PerlToken.RPAREN)
//                     {
//                         elements.Add(Expr());
//                         if (_currentToken.TokenType == PerlToken.COMMA)
//                         {
//                             elements.Add(new PunctuationNode(","));
//                             ASTNode error11111111122 = Eat(PerlToken.COMMA);
//                             if (error11111111122 != null) return error11111111122;

//                             // Eat(PerlToken.COMMA);
//                         }
//                     }
//                     ASTNode error111111111222 = Eat(PerlToken.RPAREN);
//                     if (error111111111222 != null) return error111111111222;

//                     // Eat(PerlToken.RPAREN);
//                     if (isDeclaration)
//                     {
//                         return new VariableDeclarationNode(keyword, arrayToken.Lexeme, new ArrayNode(arrayToken.Lexeme, elements));
//                     }
//                     else
//                     {
//                         return new AssignmentNode(arrayToken.Lexeme, new ArrayNode(arrayToken.Lexeme, elements));
//                     }
//                 }
//                 else
//                 {
//                     throw new Exception($"Expected token ASSIGN, got {_currentToken.TokenType}");
//                 }
//             }
//             else if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme.StartsWith("%"))
//             {
//                 Token hashToken = _currentToken;
//                 ASTNode error111111111222 = Eat(PerlToken.IDENT);
//                 if (error111111111222 != null) return error111111111222;

//                 // Eat(PerlToken.IDENT);

//                 if (_currentToken.TokenType == PerlToken.ASSIGN)
//                 {
//                     ASTNode error1111111112222 = Eat(PerlToken.ASSIGN);
//                     if (error1111111112222 != null) return error1111111112222;

//                     // Eat(PerlToken.ASSIGN);
//                     ASTNode error11111111122222 = Eat(PerlToken.LPAREN);
//                     if (error11111111122222 != null) return error11111111122222;

//                     // Eat(PerlToken.LPAREN);
//                     List<(ASTNode Key, ASTNode Value)> elements = new List<(ASTNode Key, ASTNode Value)>();
//                     while (_currentToken.TokenType != PerlToken.RPAREN)
//                     {
//                         ASTNode key = Expr();
//                         if (_currentToken.TokenType == PerlToken.HASH_ASSIGN)
//                         {
//                             ASTNode error1111111112222224 = Eat(PerlToken.HASH_ASSIGN);
//                             if (error1111111112222224 != null) return error1111111112222224;

//                             // Eat(PerlToken.HASH_ASSIGN);
//                         }
//                         else
//                         {
//                             throw new Exception($"Expected token HASH_ASSIGN, got {_currentToken.TokenType}");
//                         }
//                         ASTNode value = Expr();
//                         elements.Add((key, value));
//                         if (_currentToken.TokenType == PerlToken.COMMA)
//                         {
//                             ASTNode error1111111112222223 = Eat(PerlToken.COMMA);
//                             if (error1111111112222223 != null) return error1111111112222223;

//                             // Eat(PerlToken.COMMA);
//                         }
//                     }
//                     ASTNode error111111111222222 = Eat(PerlToken.RPAREN);
//                     if (error111111111222222 != null) return error111111111222222;

//                     // Eat(PerlToken.RPAREN);
//                     if (isDeclaration)
//                     {
//                         return new VariableDeclarationNode(keyword, hashToken.Lexeme, new HashNode(hashToken.Lexeme, elements));
//                     }
//                     else
//                     {
//                         return new AssignmentNode(hashToken.Lexeme, new HashNode(hashToken.Lexeme, elements));
//                     }
//                 }
//                 else
//                 {
//                     throw new Exception($"Expected token ASSIGN, got {_currentToken.TokenType}");
//                 }
//             }
//             else if (_currentToken.TokenType == PerlToken.IDENT)
//             {
//                 Token variableToken = _currentToken;
//                 ASTNode error111111111222222 = Eat(PerlToken.IDENT);
//                 if (error111111111222222 != null) return error111111111222222;

//                 // Eat(PerlToken.IDENT);

//                 if (_currentToken.TokenType == PerlToken.ASSIGN)
//                 {
//                     ASTNode error1111111112222222 = Eat(PerlToken.ASSIGN);
//                     if (error1111111112222222 != null) return error1111111112222222;

//                     // Eat(PerlToken.ASSIGN);

//                     if (_currentToken.TokenType == PerlToken.LPAREN)
//                     {
//                         // Handle hash declarations
//                         ASTNode hash = ParseHash();
//                         if (isDeclaration)
//                         {
//                             return new VariableDeclarationNode(keyword, variableToken.Lexeme, hash);
//                         }
//                         else
//                         {
//                             return new AssignmentNode(variableToken.Lexeme, hash);
//                         }
//                     }
//                     else
//                     {
//                         // Handle regular assignments
//                         ASTNode value = Expr();
//                         if (isDeclaration)
//                         {
//                             return new VariableDeclarationNode(keyword, variableToken.Lexeme, value);
//                         }
//                         else
//                         {
//                             return new AssignmentNode(variableToken.Lexeme, value);
//                         }
//                     }
//                 }
//                 else if (_currentToken.TokenType == PerlToken.ADD_ASSIGN || _currentToken.TokenType == PerlToken.SUB_ASSIGN ||
//                         _currentToken.TokenType == PerlToken.MUL_ASSIGN || _currentToken.TokenType == PerlToken.DIV_ASSIGN)
//                 {
//                     // Handle compound assignment operators
//                     Token operatorToken = _currentToken;
//                     ASTNode error1111111112222222 = Eat(operatorToken.TokenType);
//                     if (error1111111112222222 != null) return error1111111112222222;

//                     // Eat(operatorToken.TokenType);
//                     ASTNode value = Expr();

//                     if (isDeclaration)
//                     {
//                         // Return a declaration node for "my $a += 1;"
//                         return new VariableDeclarationNode(keyword, variableToken.Lexeme, new CompoundAssignmentNode(variableToken.Lexeme, operatorToken.Lexeme, value));
//                     }
//                     else
//                     {
//                         // Return a compound assignment node for "$a += 1;"
//                         return new CompoundAssignmentNode(variableToken.Lexeme, operatorToken.Lexeme, value);
//                     }
//                 }
//                 else if (_currentToken.TokenType == PerlToken.INC)
//                 {
//                     // Handle increment (++)
//                     ASTNode error1111111112222222 = Eat(PerlToken.INC);
//                     if (error1111111112222222 != null) return error1111111112222222;

//                     // Eat(PerlToken.INC);
//                     return new UnaryOperationNode(variableToken.Lexeme, "++");
//                 }
//                 else if (_currentToken.TokenType == PerlToken.DEC)
//                 {
//                     // Handle decrement (--)
//                     ASTNode error1111111112222222 = Eat(PerlToken.DEC);
//                     if (error1111111112222222 != null) return error1111111112222222;

//                     // Eat(PerlToken.DEC);
//                     return new UnaryOperationNode(variableToken.Lexeme, "--");
//                 }
//                 else
//                 {
//                     throw new Exception($"Expected token ASSIGN, INC, DEC, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, or MOD_ASSIGN, got {_currentToken.TokenType}");
//                 }
//             }
//             else
//             {
//                 throw new Exception($"Expected identifier, got {_currentToken.TokenType}");
//             }
//         }


//         private ASTNode ParseHash()
//         {
//             ASTNode error1111111112222222 = Eat(PerlToken.LPAREN);
//             if (error1111111112222222 != null) return error1111111112222222;

//             // Eat(PerlToken.LPAREN); // Consume the opening parenthesis

//             List<(ASTNode Key, ASTNode Value)> elements = new List<(ASTNode Key, ASTNode Value)>();

//             while (_currentToken.TokenType != PerlToken.RPAREN)
//             {
//                 // Parse the key (must be a string or identifier)
//                 ASTNode key = null;
//                 if (_currentToken.TokenType == PerlToken.STRING || _currentToken.TokenType == PerlToken.IDENT)
//                 {
//                     key = new StringNode(_currentToken.Lexeme);
//                     ASTNode error11111111122222227 = Eat(_currentToken.TokenType);
//                     if (error11111111122222227 != null) return error11111111122222227;

//                     // Eat(_currentToken.TokenType);
//                 }
//                 else
//                 {
//                     throw new Exception($"Expected string or identifier as hash key, got {_currentToken.TokenType}");
//                 }

//                 // Expect the hash assignment operator (=>)
//                 if (_currentToken.TokenType == PerlToken.HASH_ASSIGN)
//                 {
//                     ASTNode error11111111122222227 = Eat(PerlToken.HASH_ASSIGN);
//                     if (error11111111122222227 != null) return error11111111122222227;

//                     // Eat(PerlToken.HASH_ASSIGN);
//                 }
//                 else
//                 {
//                     throw new Exception($"Expected token HASH_ASSIGN (=>), got {_currentToken.TokenType}");
//                 }

//                 // Parse the value (expression)
//                 ASTNode value = Expr();
//                 elements.Add((key, value));

//                 // Handle commas between hash elements
//                 if (_currentToken.TokenType == PerlToken.COMMA)
//                 {
//                     ASTNode error11111111122222227 = Eat(PerlToken.COMMA);
//                     if (error11111111122222227 != null) return error11111111122222227;

//                     // Eat(PerlToken.COMMA);
//                 }
//             }

//             ASTNode error111111111222222272= Eat(PerlToken.RPAREN);
//             if (error111111111222222272 != null) return error111111111222222272;

//             // Eat(PerlToken.RPAREN); // Consume the closing parenthesis

//             return new HashNode("hash", elements);
//         }

//         private ASTNode ForStatement()
//         {
//             ASTNode error= Eat(PerlToken.FOR);
//             if (error != null) return error;

//             // Eat(PerlToken.FOR);
//             ASTNode error1 = Eat(PerlToken.LPAREN);
//             if (error1 != null) return error1;

//             // Eat(PerlToken.LPAREN);

//             ASTNode initialization = Assignment();
//             ASTNode initSemicolon = null;
//             if (_currentToken.TokenType == PerlToken.SEMICOLON)
//             {
//                 initSemicolon = new PunctuationNode(";");
//                 ASTNode error2 = Eat(PerlToken.SEMICOLON);
//                 if (error2 != null) return error2;

//                 // Eat(PerlToken.SEMICOLON);
//             }

//             ASTNode condition = Expr();
//             ASTNode conditionSemicolon = null;
//             if (_currentToken.TokenType == PerlToken.SEMICOLON)
//             {
//                 conditionSemicolon = new PunctuationNode(";");
//                 ASTNode error2 = Eat(PerlToken.SEMICOLON);
//                 if (error2 != null) return error2;

//                 // Eat(PerlToken.SEMICOLON);
//             }

//             ASTNode increment = Assignment();
//             ASTNode error3 = Eat(PerlToken.RPAREN);
//             if (error3 != null) return error3;

//             // Eat(PerlToken.RPAREN);

//             ASTNode forExpression = new ParenthesizedExpression(
//                 new StatementListNode(new List<ASTNode>
//                 {
//                     initialization,
//                     initSemicolon,
//                     condition,
//                     conditionSemicolon,
//                     increment
//                 })
//             );

//             ASTNode error4 = Eat(PerlToken.LBRACE);
//             if (error4 != null) return error4;

//             // Eat(PerlToken.LBRACE);
//             ASTNode body = ParseBlock();
//             ASTNode error5 = Eat(PerlToken.RBRACE);
//             if (error5 != null) return error5;

//             // Eat(PerlToken.RBRACE);

//             return new ForNode(forExpression, null, null, body);
//         }

        
//         private ASTNode ForeachStatement()
//         {
//             ASTNode error5 = Eat(PerlToken.FOREACH);
//             if (error5 != null) return error5;

//             // Eat(PerlToken.FOREACH);
//             ASTNode error6 = Eat(PerlToken.MY);
//             if (error6 != null) return error6;

//             // Eat(PerlToken.MY);
//             string variable = _currentToken.Lexeme;
//             ASTNode error7 = Eat(PerlToken.IDENT);
//             if (error7 != null) return error7;

//             // Eat(PerlToken.IDENT); // Variable

//             ASTNode variableDeclaration = new VariableDeclarationNode("my", variable, null);
//             ASTNode error8 = Eat(PerlToken.LPAREN);
//             if (error8 != null) return error8;

//             // Eat(PerlToken.LPAREN);
//             ASTNode array = new VariableNode(_currentToken.Lexeme);
//             ASTNode error9 = Eat(PerlToken.IDENT); 
//             if (error9 != null) return error9;

//             // Eat(PerlToken.IDENT); // Array
//             ASTNode error10 = Eat(PerlToken.RPAREN);
//             if (error10 != null) return error10;

//             // Eat(PerlToken.RPAREN);
//             ASTNode error11 = Eat(PerlToken.LBRACE);
//             if (error11 != null) return error11;

//             // Eat(PerlToken.LBRACE);
//             ASTNode body = ParseBlock();
//             ASTNode error12 = Eat(PerlToken.RBRACE);
//             if (error12 != null) return error12;

//             // Eat(PerlToken.RBRACE);

//             return new ForeachNode(variableDeclaration, new ParenthesizedExpression(array), body);
//         }
                            
        
//         private ASTNode WhileStatement()
//         {
//             ASTNode error = Eat(PerlToken.WHILE);
//             if (error != null) return error;

//             // Eat(PerlToken.WHILE);
//             ASTNode error1 = Eat(PerlToken.LPAREN);
//             if (error1 != null) return error1;

//             // Eat(PerlToken.LPAREN);
//             ASTNode condition = Expr();
//             ASTNode error2 = Eat(PerlToken.RPAREN);
//             if (error2 != null) return error2;

//             // Eat(PerlToken.RPAREN);
//             ASTNode error3 = Eat(PerlToken.LBRACE);
//             if (error3 != null) return error3;

//             // Eat(PerlToken.LBRACE);
//             ASTNode body = ParseBlock();
//             ASTNode error4 = Eat(PerlToken.RBRACE);
//             if (error4 != null) return error4;

//             // Eat(PerlToken.RBRACE);

//             return new WhileNode(new ParenthesizedExpression(condition), body);
//         }
        
//         private ASTNode FunctionDeclaration()
//         {
//             ASTNode error = Eat(PerlToken.SUB);
//             if (error != null) return error;

//             // Eat(PerlToken.SUB);
//             if (_currentToken.TokenType != PerlToken.IDENT)
//             {
//                 throw new Exception($"Expected function name, got {_currentToken.TokenType}");
//             }

//             string functionName = _currentToken.Lexeme;
//             ASTNode error123 = Eat(PerlToken.IDENT);
//             if (error123 != null) return error123;

//             // Eat(PerlToken.IDENT);

//             List<ASTNode> parameters = new List<ASTNode>();
//             if (_currentToken.TokenType == PerlToken.LPAREN)
//             {
//                 ASTNode error1234 = Eat(PerlToken.LPAREN);
//                 if (error1234 != null) return error1234;

//                 // Eat(PerlToken.LPAREN);
//                 while (_currentToken.TokenType != PerlToken.RPAREN)
//                 {
//                     if (_currentToken.TokenType == PerlToken.MY)
//                     {
//                         ASTNode error12345 = Eat(PerlToken.MY);
//                         if (error12345 != null) return error12345;

//                         // Eat(PerlToken.MY);
//                         if (_currentToken.TokenType == PerlToken.LPAREN)
//                         {
//                             ASTNode error123456 = Eat(PerlToken.LPAREN);
//                             if (error123456 != null) return error123456;

//                             // Eat(PerlToken.LPAREN);
//                             while (_currentToken.TokenType != PerlToken.RPAREN)
//                             {
//                                 string paramName = _currentToken.Lexeme;
//                                 ASTNode error1234567 = Eat(PerlToken.IDENT);
//                                 if (error1234567 != null) return error1234567;

//                                 // Eat(PerlToken.IDENT);
//                                 parameters.Add(new VariableDeclarationNode("my", paramName, null));

//                                 if (_currentToken.TokenType == PerlToken.COMMA)
//                                 {
//                                     ASTNode error12345678 = Eat(PerlToken.COMMA);
//                                     if (error12345678 != null) return error12345678;

//                                     // Eat(PerlToken.COMMA);
//                                 }
//                             }
//                             ASTNode error123456789 = Eat(PerlToken.RPAREN);
//                             if (error123456789 != null) return error123456789;

//                             // Eat(PerlToken.RPAREN);
//                         }
//                         else
//                         {
//                             string paramName = _currentToken.Lexeme;
//                             ASTNode error123456789 = Eat(PerlToken.IDENT);
//                             if (error123456789 != null) return error123456789;

//                             // Eat(PerlToken.IDENT);
//                             parameters.Add(new VariableDeclarationNode("my", paramName, null));
//                         }
//                     }
//                     else if (_currentToken.TokenType == PerlToken.IDENT)
//                     {
//                         parameters.Add(new VariableNode(_currentToken.Lexeme));
//                         ASTNode error1234567895 = Eat(PerlToken.IDENT);
//                         if (error1234567895 != null) return error1234567895;

//                         // Eat(PerlToken.IDENT);
//                     }
//                     if (_currentToken.TokenType == PerlToken.COMMA)
//                     {
//                         ASTNode error1234567894 = Eat(PerlToken.COMMA);
//                         if (error1234567894 != null) return error1234567894;

//                         // Eat(PerlToken.COMMA);
//                     }
//                 }
//                 ASTNode error1234567893 = Eat(PerlToken.RPAREN);
//                 if (error1234567893 != null) return error1234567893;

//                 // Eat(PerlToken.RPAREN);
//             }

//             // Handle assignment to parameters (e.g., my ($age) = @_;)
//             if (_currentToken.TokenType == PerlToken.ASSIGN)
//             {
//                 ASTNode error1234567893 = Eat(PerlToken.ASSIGN);
//                 if (error1234567893 != null) return error1234567893;

//                 // Eat(PerlToken.ASSIGN);
//                 ASTNode value = Expr();
//                 foreach (var param in parameters)
//                 {
//                     if (param is VariableDeclarationNode declaration)
//                     {
//                         declaration.Value = value;
//                     }
//                 }
//             }
//             ASTNode error12345678933 = Eat(PerlToken.LBRACE);
//             if (error12345678933 != null) return error12345678933;

//             // Eat(PerlToken.LBRACE);
//             ASTNode body = ParseBlock();
//             ASTNode error123456789331 = Eat(PerlToken.RBRACE);
//             if (error123456789331 != null) return error123456789331;

//             // Eat(PerlToken.RBRACE);

//             return new FunctionNode(functionName, parameters, body);
//         }


//         private ASTNode ClassFunctionDeclaration()
//         {
//             ASTNode error = Eat(PerlToken.SUB);
//             if (error != null) return error;

//             // Eat(PerlToken.SUB);

//             // Parse function name
//             if (_currentToken.TokenType != PerlToken.IDENT)
//             {
//                 throw new Exception($"Expected function name, got {_currentToken.TokenType}");
//             }
//             string functionName = _currentToken.Lexeme;
//             ASTNode error1 = Eat(PerlToken.IDENT);
//             if (error1 != null) return error1;

//             // Eat(PerlToken.IDENT);

//             // Parse parameters
//             List<ASTNode> parameters = new List<ASTNode>();
//             if (_currentToken.TokenType == PerlToken.LPAREN)
//             {
//                 ASTNode error2 = Eat(PerlToken.LPAREN);
//                 if (error2 != null) return error2;

//                 // Eat(PerlToken.LPAREN);
//                 while (_currentToken.TokenType != PerlToken.RPAREN)
//                 {
//                     if (_currentToken.TokenType == PerlToken.MY)
//                     {
//                         ASTNode error3 = Eat(PerlToken.MY);
//                         if (error3 != null) return error3;

//                         // Eat(PerlToken.MY);
//                         string paramName = _currentToken.Lexeme;
//                         ASTNode error4 = Eat(PerlToken.IDENT);
//                         if (error4 != null) return error4;

//                         // Eat(PerlToken.IDENT);
//                         parameters.Add(new VariableDeclarationNode("my", paramName, null));
//                     }
//                     else if (_currentToken.TokenType == PerlToken.IDENT)
//                     {
//                         parameters.Add(new VariableNode(_currentToken.Lexeme));
//                         ASTNode error4 = Eat(PerlToken.IDENT);
//                         if (error4 != null) return error4;

//                         // Eat(PerlToken.IDENT);
//                     }
//                     if (_currentToken.TokenType == PerlToken.COMMA)
//                     {
//                         ASTNode error4 = Eat(PerlToken.COMMA);
//                         if (error4 != null) return error4;

//                         // Eat(PerlToken.COMMA);
//                     }
//                 }
//                 ASTNode error51 = Eat(PerlToken.RPAREN);
//                 if (error51 != null) return error51;

//                 // Eat(PerlToken.RPAREN);
//             }

//             // Parse function body
//             ASTNode error5 = Eat(PerlToken.LBRACE);
//             if (error5 != null) return error5;

//             // Eat(PerlToken.LBRACE);
//             List<ASTNode> body = new List<ASTNode>();

//             while (_currentToken.TokenType != PerlToken.RBRACE)
//             {
//                 if (_currentToken.TokenType == PerlToken.MY)
//                 {
//                     // Handle variable declarations
//                     body.Add(Assignment());
//                 }
//                 else if (_currentToken.TokenType == PerlToken.PRINT)
//                 {
//                     // Handle print statements
//                     body.Add(PrintStatement());
//                 }
//                 else if (_currentToken.TokenType == PerlToken.BLESS)
//                 {
//                     // Handle bless statements
//                     ASTNode error6 = Eat(PerlToken.BLESS);
//                     if (error6 != null) return error6;

//                     // Eat(PerlToken.BLESS);
//                     ASTNode objectNode = Expr();
//                     ASTNode error7 = Eat(PerlToken.COMMA);
//                     if (error7 != null) return error7;

//                     // Eat(PerlToken.COMMA);
//                     ASTNode classNode = Expr();
//                     body.Add(new BlessNode(objectNode, classNode));
//                 }
//                 else if (_currentToken.TokenType == PerlToken.RETURN)
//                 {
//                     // Handle return statements
//                     body.Add(ReturnStatement());
//                 }
//                 else
//                 {
//                     // Handle other statements
//                     body.Add(ParseStatement());
//                 }
//             }
//             ASTNode error8 = Eat(PerlToken.RBRACE);
//             if (error8 != null) return error8;

//             // Eat(PerlToken.RBRACE);

//             return new FunctionNode(functionName, parameters, new BlockNode(body));
//         }
//     }
// }