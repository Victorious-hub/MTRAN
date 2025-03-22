using System;
using System.Collections.Generic;

namespace MTRAN.Parser
{
    public class Parser
    {
        private readonly PerlLexer _lexer;
        private Token _currentToken;
        private int _currentTokenIndex;
        private Stack<PerlToken> _parenStack = new Stack<PerlToken>();

        public Parser(PerlLexer lexer)
        {
            _lexer = lexer;
            _currentTokenIndex = 0;
            _currentToken = _lexer.tokens[_currentTokenIndex];
        }

        private void LogError(string message)
        {
            string errorFilePath = "/home/shyskov/Documents/libs/sem6/MTRAN/Parser/parse_errors.log";
            string errorMessage = $"{DateTime.Now}: {message} at line {_currentToken.Line}.";
            Console.WriteLine(errorMessage);
            using (StreamWriter writer = new StreamWriter(errorFilePath, append: true))
            {
                writer.WriteLine(errorMessage);
            }
        }

        private void Eat(PerlToken tokenType)
        {
            if (_currentToken.TokenType == tokenType)
            {
                // Track parentheses
                if (tokenType == PerlToken.LPAREN)
                {
                    _parenStack.Push(tokenType);
                }
                else if (tokenType == PerlToken.RPAREN)
                {
                    if (_parenStack.Count == 0 || _parenStack.Peek() != PerlToken.LPAREN)
                    {
                        LogError($"Unmatched closing parenthesis at index {_currentTokenIndex}");
                        throw new Exception($"Unmatched closing parenthesis at index {_currentTokenIndex}");
                    }
                    _parenStack.Pop();
                }

                _currentTokenIndex++;
                if (_currentTokenIndex < _lexer.tokens.Count)
                {
                    _currentToken = _lexer.tokens[_currentTokenIndex];
                }
            }
            else
            {
                LogError($"Expected token {tokenType}, got {_currentToken.TokenType} at index {_currentTokenIndex}");
                throw new Exception($"Expected token {tokenType}, got {_currentToken.TokenType} at index {_currentTokenIndex}");
            }
        }

        private ASTNode Factor()
        {
            Token token = _currentToken;

            if (token.TokenType == PerlToken.NUMBER)
            {
                Eat(PerlToken.NUMBER);
                return new NumberNode(float.Parse(token.Lexeme));
            }
            else if (token.TokenType == PerlToken.INT)
            {
                Eat(PerlToken.INT);
                return new NumberNode(int.Parse(token.Lexeme));
            }
            else if (token.TokenType == PerlToken.IDENT)
            {
                string functionName = token.Lexeme;
                Eat(PerlToken.IDENT);

                // Check if this is a function call
                if (_currentToken.TokenType == PerlToken.LPAREN)
                {
                    Eat(PerlToken.LPAREN);
                    List<ASTNode> arguments = new List<ASTNode>();
                    while (_currentToken.TokenType != PerlToken.RPAREN)
                    {
                        arguments.Add(Expr());
                        if (_currentToken.TokenType == PerlToken.COMMA)
                        {
                            Eat(PerlToken.COMMA);
                        }
                    }
                    Eat(PerlToken.RPAREN);
                    return new FunctionCallNode(functionName, arguments);
                }

                // Handle compound assignment operators
                if (_currentToken.TokenType == PerlToken.ADD_ASSIGN || _currentToken.TokenType == PerlToken.SUB_ASSIGN ||
                    _currentToken.TokenType == PerlToken.MUL_ASSIGN || _currentToken.TokenType == PerlToken.DIV_ASSIGN)
                {
                    Token operatorToken = _currentToken;
                    Eat(operatorToken.TokenType);
                    ASTNode value = Expr();
                    return new CompoundAssignmentNode(token.Lexeme, operatorToken.Lexeme, value);
                }
                else if (_currentToken.TokenType == PerlToken.ASSIGN)
                {
                    Eat(PerlToken.ASSIGN);
                    ASTNode value = Expr();
                    return new AssignmentNode(token.Lexeme, value);
                }
                else if (_currentToken.TokenType == PerlToken.INC)
                {
                    Eat(PerlToken.INC);
                    return new UnaryOperationNode(token.Lexeme, "++");
                }
                else if (_currentToken.TokenType == PerlToken.DEC)
                {
                    Eat(PerlToken.DEC);
                    return new UnaryOperationNode(token.Lexeme, "--");
                }

                return new VariableNode(token.Lexeme);
            }
            else if (token.TokenType == PerlToken.STRING)
            {
                Eat(PerlToken.STRING);
                return new StringNode(token.Lexeme);
            }
            else if (_currentToken.TokenType == PerlToken.SHIFT)
            {
                Eat(PerlToken.SHIFT);
                return new KeywordNode("shift");
            }
            else if (_currentToken.TokenType == PerlToken.BLESS)
            {
                Eat(PerlToken.BLESS);

                // Parse the object being blessed
                ASTNode obj = Expr();

                // Optionally, parse the class name (if provided)
                ASTNode cls = null;
                if (_currentToken.TokenType == PerlToken.COMMA)
                {
                    Eat(PerlToken.COMMA);
                    cls = Expr();
                }

                return new BlessNode(obj, cls);
            }
            else if (_currentToken.TokenType == PerlToken.NEW)
            {
                Eat(PerlToken.NEW);
                return new KeywordNode("new");
            }
            else if (token.TokenType == PerlToken.LPAREN)
            {
                Eat(PerlToken.LPAREN);
                ASTNode node = Expr();
                Eat(PerlToken.RPAREN);
                return node;
            }
            else if (token.TokenType == PerlToken.SUB)
            {
                return FunctionDeclaration();
            }
            else if (token.TokenType == PerlToken.IDENT && token.Lexeme.StartsWith("@"))
            {
                Eat(PerlToken.IDENT);
                return new VariableNode(token.Lexeme);
            }
            else if (token.TokenType == PerlToken.MY)
            {
                return Assignment();
            }
            else
            {
                string errorMessage = $"Invalid factor: unexpected token '{token.TokenType}' at line {_currentToken.Line}.";
                LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        private ASTNode Term()
        {
            ASTNode node = Factor();

            while (_currentToken.TokenType == PerlToken.MUL || _currentToken.TokenType == PerlToken.DIV)
            {
                Token token = _currentToken;
                if (token.TokenType == PerlToken.MUL)
                {
                    Eat(PerlToken.MUL);
                }
                else if (token.TokenType == PerlToken.DIV)
                {
                    Eat(PerlToken.DIV);
                }

                node = new BinaryOperationNode(node, token.Lexeme, Factor());
            }

            return node;
        }

        private ASTNode Expr()
        {
            return ParseLogicalOr();
        }

        private ASTNode ParseLogicalOr()
        {
            ASTNode node = ParseLogicalAnd();

            while (_currentToken.TokenType == PerlToken.LOR || _currentToken.TokenType == PerlToken.OR)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, ParseLogicalAnd());
            }

            return node;
        }

        private ASTNode ParseLogicalAnd()
        {
            ASTNode node = ParseEquality();

            while (_currentToken.TokenType == PerlToken.LAND || _currentToken.TokenType == PerlToken.AND)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, ParseEquality());
            }

            return node;
        }

        private ASTNode ParseEquality()
        {
            ASTNode node = ParseBitwiseXor();

            while (_currentToken.TokenType == PerlToken.EQUAL || _currentToken.TokenType == PerlToken.NOT_EQUAL)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, ParseBitwiseXor());
            }

            return node;
        }

        private ASTNode ParseBitwiseXor()
        {
            ASTNode node = ParseRelational();

            while (_currentToken.TokenType == PerlToken.BITWISE_XOR || _currentToken.TokenType == PerlToken.XOR)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, ParseRelational());
            }

            return node;
        }

        private ASTNode ParseRelational()
        {
            ASTNode node = ParseAdditive();

            while (_currentToken.TokenType == PerlToken.LESS || _currentToken.TokenType == PerlToken.GRT ||
                _currentToken.TokenType == PerlToken.LESS_OR_EQUAL || _currentToken.TokenType == PerlToken.GRT_OR_EQUAL)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, ParseAdditive());
            }

            return node;
        }

        private ASTNode ParseAdditive()
        {
            ASTNode node = ParseMultiplicative();

            while (_currentToken.TokenType == PerlToken.ADD || _currentToken.TokenType == PerlToken.SUB)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, ParseMultiplicative());
            }

            return node;
        }

        private ASTNode ParseMultiplicative()
        {
            ASTNode node = Factor();

            while (_currentToken.TokenType == PerlToken.MUL || _currentToken.TokenType == PerlToken.DIV || _currentToken.TokenType == PerlToken.MOD)
            {
                Token token = _currentToken;
                Eat(token.TokenType);
                node = new BinaryOperationNode(node, token.Lexeme, Factor());
            }

            return node;
        }

        private ASTNode IfStatement()
        {
            Eat(PerlToken.IF);
            Eat(PerlToken.LPAREN);
            ASTNode condition = Expr();
            Eat(PerlToken.RPAREN);
            Eat(PerlToken.LBRACE);
            ASTNode thenBranch = ParseBlock();
            Eat(PerlToken.RBRACE);

            List<ElseIfNode> elseIfBranches = new List<ElseIfNode>();
            while (_currentToken.TokenType == PerlToken.ELSIF)
            {
                Eat(PerlToken.ELSIF);
                Eat(PerlToken.LPAREN);
                ASTNode elseIfCondition = Expr();
                Eat(PerlToken.RPAREN);
                Eat(PerlToken.LBRACE);
                ASTNode elseIfThenBranch = ParseBlock();
                Eat(PerlToken.RBRACE);
                elseIfBranches.Add(new ElseIfNode(elseIfCondition, elseIfThenBranch));
            }

            ASTNode elseBranch = null;
            if (_currentToken.TokenType == PerlToken.ELSE)
            {
                Eat(PerlToken.ELSE);
                Eat(PerlToken.LBRACE);
                elseBranch = ParseBlock();
                Eat(PerlToken.RBRACE);
            }

            return new IfNode(new ParenthesizedExpression(condition), thenBranch, elseIfBranches, elseBranch);
        }

        private ASTNode ParseBlock()
        {
            List<ASTNode> statements = new List<ASTNode>();

            while (_currentToken.TokenType != PerlToken.RBRACE && _currentToken.TokenType != PerlToken.EOF_)
            {
                ASTNode node = ParseStatement();
                if (node != null)
                {
                    statements.Add(node);
                }
                Console.WriteLine(_currentToken.TokenType);
                if (_currentToken.TokenType == PerlToken.SEMICOLON)
                {
                    statements.Add(new PunctuationNode(";"));
                    Eat(PerlToken.SEMICOLON);
                }
                else if (_currentToken.TokenType == PerlToken.LPAREN)
                {
                    statements.Add(new PunctuationNode("("));
                    Eat(PerlToken.LPAREN);
                }
                else if (_currentToken.TokenType == PerlToken.RPAREN)
                {
                    statements.Add(new PunctuationNode(")"));
                    Eat(PerlToken.RPAREN);
                }
                else if (_currentToken.TokenType != PerlToken.RBRACE && _currentToken.TokenType != PerlToken.EOF_)
                {
                    // Allow certain tokens to be present after a statement
                    if (_currentToken.TokenType == PerlToken.MY || _currentToken.TokenType == PerlToken.IF || 
                        _currentToken.TokenType == PerlToken.FOR || _currentToken.TokenType == PerlToken.FOREACH || 
                        _currentToken.TokenType == PerlToken.WHILE || _currentToken.TokenType == PerlToken.ELSIF || 
                        _currentToken.TokenType == PerlToken.ELSE)
                    {
                        continue;
                    }
                    // throw new Exception("Unexpected token after statement");
                }
                // else
                // {
                //     LogError("Unclosed block: missing '}'");
                // }
            }

            return new BlockNode(statements);
        }

        private ASTNode ParseStatement()
        {
            if (_currentToken.TokenType == PerlToken.IF)
            {
                return IfStatement();
            }
            else if (_currentToken.TokenType == PerlToken.FOR)
            {
                return ForStatement();
            }
            else if (_currentToken.TokenType == PerlToken.FOREACH)
            {
                return ForeachStatement();
            }
            else if (_currentToken.TokenType == PerlToken.WHILE)
            {
                return WhileStatement();
            }
            else if (_currentToken.TokenType == PerlToken.MY)
            {
                return Assignment();
            }
            else if (_currentToken.TokenType == PerlToken.SUB)
            {
                return FunctionDeclaration();
            }
            else if (_currentToken.TokenType == PerlToken.RETURN)
            {
                return ReturnStatement();
            }
            else if (_currentToken.TokenType == PerlToken.PRINT)
            {
                return PrintStatement();
            }
            else if (_currentToken.TokenType == PerlToken.PACKAGE)
            {
                return ClassDeclaration();
            }
            else
            {
                return Expr();
            }
        }

        private ASTNode ClassDeclaration()
        {
            Eat(PerlToken.PACKAGE);

            // Parse class name
            if (_currentToken.TokenType != PerlToken.IDENT)
            {
                string errorMessage = $"Expected class name, got '{_currentToken.TokenType}' at line {_currentToken.Line}.";
                LogError(errorMessage);
                throw new Exception($"Expected class name, got {_currentToken.TokenType}");
            }
            string className = _currentToken.Lexeme;
            Eat(PerlToken.IDENT);

            List<ASTNode> methods = new List<ASTNode>();

            // Parse methods inside the class
            while (_currentToken.TokenType == PerlToken.SUB)
            {
                methods.Add(ClassFunctionDeclaration());
            }

            return new ClassNode(className, methods);
        }

        private ASTNode PrintStatement()
        {
            Eat(PerlToken.PRINT);

            List<ASTNode> arguments = new List<ASTNode>();
            while (_currentToken.TokenType != PerlToken.SEMICOLON && _currentToken.TokenType != PerlToken.EOF_)
            {
                arguments.Add(Expr());
                if (_currentToken.TokenType == PerlToken.COMMA)
                {
                    Eat(PerlToken.COMMA);
                }
            }

            return new PrintStatementNode(arguments);
        }

        private ASTNode ReturnStatement()
        {
            Eat(PerlToken.RETURN);
            ASTNode value = Expr();
            return new ReturnNode(value);
        }

        public ASTNode Parse()
        {
            ASTNode root = ParseBlock();

            if (_parenStack.Count > 0)
            {
                // throw new Exception("Unclosed parenthesis detected.");
            }

            return root;
        }

        private ASTNode Assignment()
        {
            bool isDeclaration = false;
            string keyword = null;

            // Check if the statement starts with "my"
            if (_currentToken.TokenType == PerlToken.MY)
            {
                Eat(PerlToken.MY);
                isDeclaration = true;
                keyword = "my";
            }

            if (_currentToken.TokenType == PerlToken.LPAREN)
            {
                // Handle parentheses for grouped assignments (e.g., my ($a, $b) = @_;)
                Eat(PerlToken.LPAREN);
                List<ASTNode> variables = new List<ASTNode>();
                while (_currentToken.TokenType != PerlToken.RPAREN)
                {
                    if (_currentToken.TokenType == PerlToken.IDENT)
                    {
                        string variableName = _currentToken.Lexeme;
                        Eat(PerlToken.IDENT);
                        variables.Add(new VariableNode(variableName));
                    }
                    if (_currentToken.TokenType == PerlToken.COMMA)
                    {
                        Eat(PerlToken.COMMA);
                    }
                }
                Eat(PerlToken.RPAREN);

                if (_currentToken.TokenType == PerlToken.ASSIGN)
                {
                    Eat(PerlToken.ASSIGN);
                    ASTNode value = Expr();
                    return new GroupedAssignmentNode(variables, value);
                }
                else
                {
                    throw new Exception($"Expected token ASSIGN, got {_currentToken.TokenType}");
                }
            }

            
            if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme.StartsWith("@"))
            {
                Token arrayToken = _currentToken;
                Eat(PerlToken.IDENT);

                if (_currentToken.TokenType == PerlToken.ASSIGN)
                {
                    Eat(PerlToken.ASSIGN);
                    Eat(PerlToken.LPAREN);
                    List<ASTNode> elements = new List<ASTNode>();
                    while (_currentToken.TokenType != PerlToken.RPAREN)
                    {
                        elements.Add(Expr());
                        if (_currentToken.TokenType == PerlToken.COMMA)
                        {
                            elements.Add(new PunctuationNode(","));
                            Eat(PerlToken.COMMA);
                        }
                    }
                    Eat(PerlToken.RPAREN);
                    if (isDeclaration)
                    {
                        return new VariableDeclarationNode(keyword, arrayToken.Lexeme, new ArrayNode(arrayToken.Lexeme, elements));
                    }
                    else
                    {
                        return new AssignmentNode(arrayToken.Lexeme, new ArrayNode(arrayToken.Lexeme, elements));
                    }
                }
                else
                {
                    throw new Exception($"Expected token ASSIGN, got {_currentToken.TokenType}");
                }
            }
            else if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme.StartsWith("%"))
            {
                Token hashToken = _currentToken;
                Eat(PerlToken.IDENT);

                if (_currentToken.TokenType == PerlToken.ASSIGN)
                {
                    Eat(PerlToken.ASSIGN);
                    Eat(PerlToken.LPAREN);
                    List<(ASTNode Key, ASTNode Value)> elements = new List<(ASTNode Key, ASTNode Value)>();
                    while (_currentToken.TokenType != PerlToken.RPAREN)
                    {
                        ASTNode key = Expr();
                        if (_currentToken.TokenType == PerlToken.HASH_ASSIGN)
                        {
                            Eat(PerlToken.HASH_ASSIGN);
                        }
                        else
                        {
                            throw new Exception($"Expected token HASH_ASSIGN, got {_currentToken.TokenType}");
                        }
                        ASTNode value = Expr();
                        elements.Add((key, value));
                        if (_currentToken.TokenType == PerlToken.COMMA)
                        {
                            Eat(PerlToken.COMMA);
                        }
                    }
                    Eat(PerlToken.RPAREN);
                    if (isDeclaration)
                    {
                        return new VariableDeclarationNode(keyword, hashToken.Lexeme, new HashNode(hashToken.Lexeme, elements));
                    }
                    else
                    {
                        return new AssignmentNode(hashToken.Lexeme, new HashNode(hashToken.Lexeme, elements));
                    }
                }
                else
                {
                    throw new Exception($"Expected token ASSIGN, got {_currentToken.TokenType}");
                }
            }
            else if (_currentToken.TokenType == PerlToken.IDENT)
            {
                Token variableToken = _currentToken;
                Eat(PerlToken.IDENT);

                if (_currentToken.TokenType == PerlToken.ASSIGN)
                {
                    Eat(PerlToken.ASSIGN);

                    if (_currentToken.TokenType == PerlToken.LPAREN)
                    {
                        // Handle hash declarations
                        ASTNode hash = ParseHash();
                        if (isDeclaration)
                        {
                            return new VariableDeclarationNode(keyword, variableToken.Lexeme, hash);
                        }
                        else
                        {
                            return new AssignmentNode(variableToken.Lexeme, hash);
                        }
                    }
                    else
                    {
                        // Handle regular assignments
                        ASTNode value = Expr();
                        if (isDeclaration)
                        {
                            return new VariableDeclarationNode(keyword, variableToken.Lexeme, value);
                        }
                        else
                        {
                            return new AssignmentNode(variableToken.Lexeme, value);
                        }
                    }
                }
                else if (_currentToken.TokenType == PerlToken.ADD_ASSIGN || _currentToken.TokenType == PerlToken.SUB_ASSIGN ||
                        _currentToken.TokenType == PerlToken.MUL_ASSIGN || _currentToken.TokenType == PerlToken.DIV_ASSIGN)
                {
                    // Handle compound assignment operators
                    Token operatorToken = _currentToken;
                    Eat(operatorToken.TokenType);
                    ASTNode value = Expr();

                    if (isDeclaration)
                    {
                        // Return a declaration node for "my $a += 1;"
                        return new VariableDeclarationNode(keyword, variableToken.Lexeme, new CompoundAssignmentNode(variableToken.Lexeme, operatorToken.Lexeme, value));
                    }
                    else
                    {
                        // Return a compound assignment node for "$a += 1;"
                        return new CompoundAssignmentNode(variableToken.Lexeme, operatorToken.Lexeme, value);
                    }
                }
                else if (_currentToken.TokenType == PerlToken.INC)
                {
                    // Handle increment (++)
                    Eat(PerlToken.INC);
                    return new UnaryOperationNode(variableToken.Lexeme, "++");
                }
                else if (_currentToken.TokenType == PerlToken.DEC)
                {
                    // Handle decrement (--)
                    Eat(PerlToken.DEC);
                    return new UnaryOperationNode(variableToken.Lexeme, "--");
                }
                else
                {
                    throw new Exception($"Expected token ASSIGN, INC, DEC, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, or MOD_ASSIGN, got {_currentToken.TokenType}");
                }
            }
            else
            {
                throw new Exception($"Expected identifier, got {_currentToken.TokenType}");
            }
        }


        private ASTNode ParseHash()
        {
            Eat(PerlToken.LPAREN); // Consume the opening parenthesis

            List<(ASTNode Key, ASTNode Value)> elements = new List<(ASTNode Key, ASTNode Value)>();

            while (_currentToken.TokenType != PerlToken.RPAREN)
            {
                // Parse the key (must be a string or identifier)
                ASTNode key = null;
                if (_currentToken.TokenType == PerlToken.STRING || _currentToken.TokenType == PerlToken.IDENT)
                {
                    key = new StringNode(_currentToken.Lexeme);
                    Eat(_currentToken.TokenType);
                }
                else
                {
                    throw new Exception($"Expected string or identifier as hash key, got {_currentToken.TokenType}");
                }

                // Expect the hash assignment operator (=>)
                if (_currentToken.TokenType == PerlToken.HASH_ASSIGN)
                {
                    Eat(PerlToken.HASH_ASSIGN);
                }
                else
                {
                    throw new Exception($"Expected token HASH_ASSIGN (=>), got {_currentToken.TokenType}");
                }

                // Parse the value (expression)
                ASTNode value = Expr();
                elements.Add((key, value));

                // Handle commas between hash elements
                if (_currentToken.TokenType == PerlToken.COMMA)
                {
                    Eat(PerlToken.COMMA);
                }
            }

            Eat(PerlToken.RPAREN); // Consume the closing parenthesis

            return new HashNode("hash", elements);
        }

        private ASTNode ForStatement()
        {
            Eat(PerlToken.FOR);
            Eat(PerlToken.LPAREN);

            ASTNode initialization = Assignment();
            ASTNode initSemicolon = null;
            if (_currentToken.TokenType == PerlToken.SEMICOLON)
            {
                initSemicolon = new PunctuationNode(";");
                Eat(PerlToken.SEMICOLON);
            }

            ASTNode condition = Expr();
            ASTNode conditionSemicolon = null;
            if (_currentToken.TokenType == PerlToken.SEMICOLON)
            {
                conditionSemicolon = new PunctuationNode(";");
                Eat(PerlToken.SEMICOLON);
            }

            ASTNode increment = Assignment();
            Eat(PerlToken.RPAREN);

            ASTNode forExpression = new ParenthesizedExpression(
                new StatementListNode(new List<ASTNode>
                {
                    initialization,
                    initSemicolon,
                    condition,
                    conditionSemicolon,
                    increment
                })
            );

            Eat(PerlToken.LBRACE);
            ASTNode body = ParseBlock();
            Eat(PerlToken.RBRACE);

            return new ForNode(forExpression, null, null, body);
        }

        
        private ASTNode ForeachStatement()
        {
            Eat(PerlToken.FOREACH);
            Eat(PerlToken.MY);
            string variable = _currentToken.Lexeme;
            Eat(PerlToken.IDENT); // Variable
            ASTNode variableDeclaration = new VariableDeclarationNode("my", variable, null);
            Eat(PerlToken.LPAREN);
            ASTNode array = new VariableNode(_currentToken.Lexeme);
            Eat(PerlToken.IDENT); // Array
            Eat(PerlToken.RPAREN);
            Eat(PerlToken.LBRACE);
            ASTNode body = ParseBlock();
            Eat(PerlToken.RBRACE);

            return new ForeachNode(variableDeclaration, new ParenthesizedExpression(array), body);
        }
                            
        
        private ASTNode WhileStatement()
        {
            Eat(PerlToken.WHILE);
            Eat(PerlToken.LPAREN);
            ASTNode condition = Expr();
            Eat(PerlToken.RPAREN);
            Eat(PerlToken.LBRACE);
            ASTNode body = ParseBlock();
            Eat(PerlToken.RBRACE);

            return new WhileNode(new ParenthesizedExpression(condition), body);
        }
        
        private ASTNode FunctionDeclaration()
        {
            Eat(PerlToken.SUB);
            if (_currentToken.TokenType != PerlToken.IDENT)
            {
                throw new Exception($"Expected function name, got {_currentToken.TokenType}");
            }

            string functionName = _currentToken.Lexeme;
            Eat(PerlToken.IDENT);

            List<ASTNode> parameters = new List<ASTNode>();
            if (_currentToken.TokenType == PerlToken.LPAREN)
            {
                Eat(PerlToken.LPAREN);
                while (_currentToken.TokenType != PerlToken.RPAREN)
                {
                    if (_currentToken.TokenType == PerlToken.MY)
                    {
                        Eat(PerlToken.MY);
                        if (_currentToken.TokenType == PerlToken.LPAREN)
                        {
                            Eat(PerlToken.LPAREN);
                            while (_currentToken.TokenType != PerlToken.RPAREN)
                            {
                                string paramName = _currentToken.Lexeme;
                                Eat(PerlToken.IDENT);
                                parameters.Add(new VariableDeclarationNode("my", paramName, null));

                                if (_currentToken.TokenType == PerlToken.COMMA)
                                {
                                    Eat(PerlToken.COMMA);
                                }
                            }
                            Eat(PerlToken.RPAREN);
                        }
                        else
                        {
                            string paramName = _currentToken.Lexeme;
                            Eat(PerlToken.IDENT);
                            parameters.Add(new VariableDeclarationNode("my", paramName, null));
                        }
                    }
                    else if (_currentToken.TokenType == PerlToken.IDENT)
                    {
                        parameters.Add(new VariableNode(_currentToken.Lexeme));
                        Eat(PerlToken.IDENT);
                    }
                    if (_currentToken.TokenType == PerlToken.COMMA)
                    {
                        Eat(PerlToken.COMMA);
                    }
                }
                Eat(PerlToken.RPAREN);
            }

            // Handle assignment to parameters (e.g., my ($age) = @_;)
            if (_currentToken.TokenType == PerlToken.ASSIGN)
            {
                Eat(PerlToken.ASSIGN);
                ASTNode value = Expr();
                foreach (var param in parameters)
                {
                    if (param is VariableDeclarationNode declaration)
                    {
                        declaration.Value = value;
                    }
                }
            }

            Eat(PerlToken.LBRACE);
            ASTNode body = ParseBlock();
            Eat(PerlToken.RBRACE);

            return new FunctionNode(functionName, parameters, body);
        }


        private ASTNode ClassFunctionDeclaration()
        {
            Eat(PerlToken.SUB);

            // Parse function name
            if (_currentToken.TokenType != PerlToken.IDENT)
            {
                throw new Exception($"Expected function name, got {_currentToken.TokenType}");
            }
            string functionName = _currentToken.Lexeme;
            Eat(PerlToken.IDENT);

            // Parse parameters
            List<ASTNode> parameters = new List<ASTNode>();
            if (_currentToken.TokenType == PerlToken.LPAREN)
            {
                Eat(PerlToken.LPAREN);
                while (_currentToken.TokenType != PerlToken.RPAREN)
                {
                    if (_currentToken.TokenType == PerlToken.MY)
                    {
                        Eat(PerlToken.MY);
                        string paramName = _currentToken.Lexeme;
                        Eat(PerlToken.IDENT);
                        parameters.Add(new VariableDeclarationNode("my", paramName, null));
                    }
                    else if (_currentToken.TokenType == PerlToken.IDENT)
                    {
                        parameters.Add(new VariableNode(_currentToken.Lexeme));
                        Eat(PerlToken.IDENT);
                    }
                    if (_currentToken.TokenType == PerlToken.COMMA)
                    {
                        Eat(PerlToken.COMMA);
                    }
                }
                Eat(PerlToken.RPAREN);
            }

            // Parse function body
            Eat(PerlToken.LBRACE);
            List<ASTNode> body = new List<ASTNode>();

            while (_currentToken.TokenType != PerlToken.RBRACE)
            {
                if (_currentToken.TokenType == PerlToken.MY)
                {
                    // Handle variable declarations
                    body.Add(Assignment());
                }
                else if (_currentToken.TokenType == PerlToken.PRINT)
                {
                    // Handle print statements
                    body.Add(PrintStatement());
                }
                else if (_currentToken.TokenType == PerlToken.BLESS)
                {
                    // Handle bless statements
                    Eat(PerlToken.BLESS);
                    ASTNode objectNode = Expr();
                    Eat(PerlToken.COMMA);
                    ASTNode classNode = Expr();
                    body.Add(new BlessNode(objectNode, classNode));
                }
                else if (_currentToken.TokenType == PerlToken.RETURN)
                {
                    // Handle return statements
                    body.Add(ReturnStatement());
                }
                else
                {
                    // Handle other statements
                    body.Add(ParseStatement());
                }
            }
            Eat(PerlToken.RBRACE);

            return new FunctionNode(functionName, parameters, new BlockNode(body));
        }
    }
}