using System;
using System.Collections.Generic;

namespace MTRAN.Interpreter
{
    public class Parser
    {
        private readonly PerlLexerParser _lexer;
        private Token _currentToken;
        private int _currentTokenIndex;
        private Stack<PerlToken> _parenStack = new Stack<PerlToken>();
        
        private List<ClassNode> _classNodes = new List<ClassNode>();
        private Dictionary<string, List<FunctionNode>> _pendingClassMethods = new Dictionary<string, List<FunctionNode>>();
        private string? _currentPackage = null;


        public Parser(PerlLexerParser lexer)
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

        private ASTNode ParseHashKey()
        {
            if (_currentToken.TokenType == PerlToken.STRING || _currentToken.TokenType == PerlToken.IDENT)
            {
                ASTNode key = new StringNode(_currentToken.Lexeme); // Handle quoted keys or identifiers
                Eat(_currentToken.TokenType);
                return key;
            }
            else
            {
                throw new Exception($"Expected hash key (STRING or IDENT), got {_currentToken.TokenType} at line {_currentToken.Line}.");
            }
        }
        

        private ASTNode Factor()
        {
            Token token = _currentToken;
            
            if (_currentToken.TokenType == PerlToken.REGEX)
            {
                string regexPattern = _currentToken.Lexeme;
                Eat(PerlToken.REGEX); // Consume the regex token
                return new RegexNode(regexPattern); // Return a RegexNode
            }
            if (_currentToken.TokenType == PerlToken.BLESS)
            {
                Eat(PerlToken.BLESS);
                ASTNode refNode = Expr(); // $self
                Eat(PerlToken.COMMA);     // ,
                ASTNode classNode = Expr(); // $class
                return new BlessNode(refNode, classNode);
            }
            if (_currentToken.TokenType == PerlToken.SHIFT)
            {
                Eat(PerlToken.SHIFT); // Consume 'shift'
                return new KeywordNode("shift"); // Return a KeywordNode for 'shift'
            }

            if (token.TokenType == PerlToken.INPUT)
            {
                Eat(PerlToken.INPUT); // Consume '<STDIN>'
                return new InputNode(); // Return an InputNode
            }
            if (token.TokenType == PerlToken.SUB)
            {
                Eat(PerlToken.SUB); // Consume the '-' token
                ASTNode numberNode = Factor(); // Parse the number after the '-'
                if (numberNode is NumberNode number)
                {
                    return new NumberNode(-number.Value); // Return the negative number
                }
                else
                {
                    throw new Exception($"Expected a number after '-', got {numberNode.GetType().Name}.");
                }
            }
            if (token.TokenType == PerlToken.REF_HASH)
            {
                // Handle the REF_HASH token
                string referencedVariable = token.Lexeme;
                Eat(PerlToken.REF_HASH); // Consume the REF_HASH token
                return new ReferenceNode(referencedVariable); // Return a ReferenceNode
            }
            if (token.TokenType == PerlToken.USE)
            {
                Eat(PerlToken.USE);

                // Check for "constant" keyword
                if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme == "constant")
                {
                    Eat(PerlToken.IDENT); // Consume "constant"

                    // Parse the constant name
                    if (_currentToken.TokenType != PerlToken.IDENT)
                    {
                        throw new Exception($"Expected constant name, got {_currentToken.TokenType} at line {_currentToken.Line}.");
                    }
                    string constantName = _currentToken.Lexeme;
                    Eat(PerlToken.IDENT);

                    // Expect the hash assignment operator (=>)
                    if (_currentToken.TokenType != PerlToken.HASH_ASSIGN)
                    {
                        throw new Exception($"Expected '=>' after constant name, got {_currentToken.TokenType} at line {_currentToken.Line}.");
                    }
                    Eat(PerlToken.HASH_ASSIGN);

                    // Parse the constant value
                    ASTNode constantValue = Expr();

                    // Return a ConstantDeclarationNode
                    return new ConstantDeclarationNode(constantName, constantValue);
                }
                else
                {
                    // Просто пропускаем use strict; use warnings; и т.п.
                    if (_currentToken.TokenType == PerlToken.IDENT)
                    {
                        Eat(PerlToken.IDENT);
                    }
                    // Можно вернуть null или специальный UseNode, если нужно
                    return null;
                }
            }
           if (token.TokenType == PerlToken.NUMBER)
            {
                Eat(PerlToken.NUMBER);

                // Parse the token as a float
                return new NumberNode(float.Parse(token.Lexeme, System.Globalization.CultureInfo.InvariantCulture));
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

                if (_currentToken.TokenType == PerlToken.LBRACKET)
                {
                    Eat(PerlToken.LBRACKET);
                    ASTNode index = Expr(); // Parse the index expression
                    Eat(PerlToken.RBRACKET);

                    // Check if this is an array element assignment
                    if (_currentToken.TokenType == PerlToken.ASSIGN)
                    {
                        Eat(PerlToken.ASSIGN);
                        ASTNode value = Expr(); // Parse the value being assigned
                        return new ArrayElementAssignmentNode(functionName, index, value); // Return an ArrayElementAssignmentNode
                    }

                    return new ArrayAccessNode(functionName, index); // Return an ArrayAccessNode for access
                }

                if (_currentToken.TokenType == PerlToken.LBRACE)
                {
                    Eat(PerlToken.LBRACE);
                    ASTNode key = Expr(); // Parse the key inside the braces
                    Eat(PerlToken.RBRACE);

                    // Check if this is a hash element assignment
                    if (_currentToken.TokenType == PerlToken.ASSIGN)
                    {
                        Eat(PerlToken.ASSIGN);
                        ASTNode value = Expr(); // Parse the value being assigned
                        return new HashElementAssignmentNode(functionName, key, value); // Return a HashElementAssignmentNode
                    }

                    return new HashAccessNode(functionName, key); // Return a HashAccessNode for access
                }

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

                 if (_currentToken.TokenType == PerlToken.POINTER_HASH)
                {
                    Token targetToken = _currentToken;
                    Eat(PerlToken.POINTER_HASH); // Consume the POINTER_HASH token

                    if (_currentToken.TokenType == PerlToken.IDENT)
                    {
                        string methodName = _currentToken.Lexeme;
                        Eat(PerlToken.IDENT);

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

                            // ВАЖНО: первым аргументом добавляем объект
                            arguments.Insert(0, new VariableNode(functionName));
                            // Имя функции делаем с префиксом класса, если нужно, или просто methodName
                            return new FunctionCallNode(methodName, arguments);
                        }
                        else
                        {
                            throw new Exception($"Expected '(' after method name '{methodName}', got {_currentToken.TokenType} at line {_currentToken.Line}.");
                        }
                    }
                    else if (_currentToken.TokenType == PerlToken.LBRACE)
                    {
                        // Handle hash access (e.g., $object->{key})
                        Eat(PerlToken.LBRACE); // Consume the opening brace

                        // Parse the key inside the braces
                        ASTNode key = ParseHashKey();

                        // Expect the closing brace '}'
                        if (_currentToken.TokenType != PerlToken.RBRACE)
                        {
                            throw new Exception($"Expected '}}' after hash key, got {_currentToken.TokenType} at line {_currentToken.Line}.");
                        }
                        Eat(PerlToken.RBRACE); // Consume the closing brace

                        // Create a `HashAccessNode` with the correct hash name and key
                        return new HashAccessNode(functionName, key);
                    }
                    else
                    {
                        throw new Exception($"Expected method name or '{{' after '->', got {_currentToken.TokenType} at line {_currentToken.Line}.");
                    }
                }

                if (_currentToken.TokenType == PerlToken.LBRACE)
                {
                    Eat(PerlToken.LBRACE); // Consume the opening brace

                    // Parse the key inside the braces
                    ASTNode key = ParseHashKey();

                    // Expect the closing brace '}'
                    if (_currentToken.TokenType != PerlToken.RBRACE)
                    {
                        throw new Exception($"Expected '}}' after hash key, got {_currentToken.TokenType} at line {_currentToken.Line}.");
                    }
                    Eat(PerlToken.RBRACE); // Consume the closing brace

                    // Create a `HashAccessNode` with the correct hash name and key
                    return new HashAccessNode(functionName, key);
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
            else if (token.TokenType == PerlToken.LPAREN)
            {
                Eat(PerlToken.LPAREN);
                ASTNode node = Expr();
                Eat(PerlToken.RPAREN);
                return node;
            }
            else if (token.TokenType == PerlToken.FUNC_SUB)
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
            else if (_currentToken.TokenType == PerlToken.LBRACE)
            {
                // Handle object initialization (e.g., my $self = { key => value, ... })
                Eat(PerlToken.LBRACE);
                List<(ASTNode Key, ASTNode Value)> properties = new List<(ASTNode Key, ASTNode Value)>();

                while (_currentToken.TokenType != PerlToken.RBRACE)
                {
                    // Ключ: идентификатор или строка
                    ASTNode key = ParseHashKey();

                    // Expect '=>'
                    if (_currentToken.TokenType == PerlToken.HASH_ASSIGN)
                    {
                        Eat(PerlToken.HASH_ASSIGN);
                    }

                    // Parse the value
                    ASTNode value = Expr();
                    properties.Add((key, value));

                    // Handle commas between properties
                    if (_currentToken.TokenType == PerlToken.COMMA)
                    {
                        Eat(PerlToken.COMMA);
                    }
                }

                Eat(PerlToken.RBRACE); // Consume the closing brace
                return new ObjectNode("self", properties); // Return an ObjectNode
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
 
            while (_currentToken.TokenType == PerlToken.LAND || _currentToken.TokenType == PerlToken.AND || 
             _currentToken.TokenType == PerlToken.EQUAL_TILDA)
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

        private ASTNode ParseConcatenation()
        {
            ASTNode node = ParseAdditive();

            while (_currentToken.TokenType == PerlToken.DOT)
            {
                Token token = _currentToken;
                Eat(PerlToken.DOT);
                node = new BinaryOperationNode(node, ".", ParseAdditive());
            }

            return node;
        }

        private ASTNode ParseRelational()
        {
            ASTNode node = ParseConcatenation();

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

            while (_currentToken.TokenType == PerlToken.ADD || _currentToken.TokenType == PerlToken.SUB || _currentToken.TokenType == PerlToken.POWER)
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
                ASTNode elseIfCondition = new ParenthesizedExpression(Expr());
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

        private ASTNode UnlessStatement()
        {
            Eat(PerlToken.UNLESS);
            Eat(PerlToken.LPAREN);
            ASTNode condition = Expr();
            Eat(PerlToken.RPAREN);
            Eat(PerlToken.LBRACE);
            ASTNode thenBranch = ParseBlock();
            Eat(PerlToken.RBRACE);

            ASTNode elseBranch = null;
            if (_currentToken.TokenType == PerlToken.ELSE)
            {
                Eat(PerlToken.ELSE);
                Eat(PerlToken.LBRACE);
                elseBranch = ParseBlock();
                Eat(PerlToken.RBRACE);
            }

            return new UnlessNode(new ParenthesizedExpression(condition), thenBranch, elseBranch);
        }

        private ASTNode ParseStatement()
        {
            if (_currentToken.TokenType == PerlToken.PACKAGE)
            {
                Eat(PerlToken.PACKAGE);
                string className = _currentToken.Lexeme;
                Eat(PerlToken.IDENT);
                _currentPackage = className;
                // Можно добавить создание ClassNode, если встретили package
                var classNode = new ClassNode(className, new List<ASTNode>());
                _classNodes.Add(classNode);
                return classNode;
            }
            if (_currentToken.TokenType == PerlToken.IF)
            {
                return IfStatement();
            }
            else if (_currentToken.TokenType == PerlToken.UNLESS) // Handle 'unless'
            {
                return UnlessStatement();
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
            else if (_currentToken.TokenType == PerlToken.FUNC_SUB)
            {
                var funcNode = FunctionDeclaration();
                if (funcNode is FunctionNode fn && fn.IsMethod && _currentPackage != null)
                {
                    var classNode = _classNodes.Find(c => c.ClassName == _currentPackage);
                    if (classNode != null)
                    {
                        classNode.Methods.Add(fn);
                        return null;
                    }
                }

                return funcNode;
            }
            else if (_currentToken.TokenType == PerlToken.RETURN)
            {
                return ReturnStatement();
            }
            else if (_currentToken.TokenType == PerlToken.PRINT)
            {
                return PrintStatement();
            }
            else if (_currentToken.TokenType == PerlToken.NEXT) // Handle 'next' keyword
            {
                return NextStatement();
            }
            else if (_currentToken.TokenType == PerlToken.LAST) // Handle 'last' keyword
            {
                return LastStatement();
            }
            else if (_currentToken.TokenType == PerlToken.INPUT)
            {
                return new InputNode(); // Handle `<STDIN>` directly
            }
            else
            {
                return Expr();
            }
        }

        private ASTNode NextStatement()
        {
            Eat(PerlToken.NEXT); // Consume the 'next' token

            // Check if the 'next' statement is part of an 'if' condition
            if (_currentToken.TokenType == PerlToken.IF)
            {
                Eat(PerlToken.IF); // Consume the 'if' keyword
                Eat(PerlToken.LPAREN); // Consume the opening parenthesis
                ASTNode condition = Expr(); // Parse the condition
                Eat(PerlToken.RPAREN); // Consume the closing parenthesis
                return new NextNode(condition); // Return a NextNode with the condition
            }

            return new NextNode(); // Return a NextNode without a condition
        }

        private ASTNode LastStatement()
        {
            Eat(PerlToken.LAST); // Consume the 'last' token

            string? label = null;
            if (_currentToken.TokenType == PerlToken.IDENT)
            {
                label = _currentToken.Lexeme; // Get the label
                Eat(PerlToken.IDENT); // Consume the label
            }

            // Check if the 'last' statement is part of an 'if' condition
            if (_currentToken.TokenType == PerlToken.IF)
            {
                Eat(PerlToken.IF); // Consume the 'if' keyword
                Eat(PerlToken.LPAREN); // Consume the opening parenthesis
                ASTNode condition = Expr(); // Parse the condition
                Eat(PerlToken.RPAREN); // Consume the closing parenthesis
                return new LastNode(condition, label); // Return a LastNode with the condition and label
            }

            return new LastNode(null, label); // Return a LastNode with only the label
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
            string? keyword = null;

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

            if (_currentToken.TokenType == PerlToken.ILLEGAL)
            {
                string errorMessage = $"Invalid variable name '{_currentToken.Lexeme}' at line {_currentToken.Line}. Variable name must not start from number";
                LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (_currentToken.TokenType == PerlToken.IDENT)
            {
                string variableName = _currentToken.Lexeme;
                if (!variableName.StartsWith("$") && !variableName.StartsWith("@") && !variableName.StartsWith("%"))
                {
                    string errorMessage = $"Invalid variable name '{variableName}' at line {_currentToken.Line}. Variable names must start with $, @, or %.";
                    LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }


            if (_currentToken.TokenType == PerlToken.REF_HASH)
            {
                Console.WriteLine("1111111111111111111111111");
                string referencedVariable = _currentToken.Lexeme;
                Eat(PerlToken.REF_HASH); // Consume the REF_HASH token

                ASTNode referenceNode = new ReferenceNode(referencedVariable);
                if (isDeclaration)
                {
                    return new VariableDeclarationNode(keyword, _currentToken.Lexeme, referenceNode);
                }
                else
                {
                    return new AssignmentNode(_currentToken.Lexeme, referenceNode);
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

                    
                    if (_currentToken.TokenType == PerlToken.IDENT)
                    {
                        string identifier = _currentToken.Lexeme;
                        Eat(PerlToken.IDENT);

                        // Check if this is a method call
                        if (_currentToken.TokenType == PerlToken.POINTER_HASH)
                        {
                            Eat(PerlToken.POINTER_HASH);

                            if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme == "new")
                            {
                                Eat(PerlToken.IDENT);

                                // Parse constructor arguments
                                List<ASTNode> arguments = new List<ASTNode>();
                                if (_currentToken.TokenType == PerlToken.LPAREN)
                                {
                                    Eat(PerlToken.LPAREN);
                                    // List<ASTNode> arguments = new List<ASTNode>();
                                    while (_currentToken.TokenType != PerlToken.RPAREN)
                                    {
                                        arguments.Add(Expr());
                                        if (_currentToken.TokenType == PerlToken.COMMA)
                                        {
                                            Eat(PerlToken.COMMA);
                                        }
                                    }

                                    if (_currentToken.TokenType != PerlToken.RPAREN)
                                    {
                                        throw new Exception($"Expected closing parenthesis ')', got {_currentToken.TokenType} at line {_currentToken.Line}.");
                                    }
                                    Eat(PerlToken.RPAREN);

                                    // Create a ConstructorCallNode
                                    // ASTNode constructorCall = new ConstructorCallNode(identifier, "new", arguments);

                                    // if (isDeclaration)
                                    // {
                                    //     return new VariableDeclarationNode(keyword, variableToken.Lexeme, constructorCall);
                                    // }
                                    // else
                                    // {
                                    //     return new AssignmentNode(variableToken.Lexeme, constructorCall);
                                    // }
                                }
                                return new ConstructorCallNode(identifier, "new", arguments);
                            }
                            else
                            {
                                throw new Exception($"Expected 'new' after '->', got {_currentToken.Lexeme} at line {_currentToken.Line}.");
                            }
                        }
                        // Check if this is a standalone function call
                        else if (_currentToken.TokenType == PerlToken.LPAREN)
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

                            if (_currentToken.TokenType != PerlToken.RPAREN)
                            {
                                throw new Exception($"Expected closing parenthesis ')', got {_currentToken.TokenType} at line {_currentToken.Line}.");
                            }
                            Eat(PerlToken.RPAREN);

                            // Create a FunctionCallNode for the standalone function call
                            ASTNode functionCall = new FunctionCallNode(identifier, arguments);

                            if (isDeclaration)
                            {
                                return new VariableDeclarationNode(keyword, variableToken.Lexeme, functionCall);
                            }
                            else
                            {
                                return new AssignmentNode(variableToken.Lexeme, functionCall);
                            }
                        }
                        else
                        {
                            throw new Exception($"Unexpected token '{_currentToken.TokenType}' after identifier '{identifier}' at line {_currentToken.Line}.");
                        }
                    }


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
            Eat(PerlToken.FOR); // Consume 'for'

            if (_currentToken.TokenType == PerlToken.MY)
            {
                Eat(PerlToken.MY); // Consume 'my'

                // Parse the loop variable
                string variableName = _currentToken.Lexeme;
                Eat(PerlToken.IDENT);
                VariableDeclarationNode loopVariable = new VariableDeclarationNode("my", variableName, null);

                if (_currentToken.TokenType == PerlToken.LPAREN)
                {
                    Eat(PerlToken.LPAREN);

                    // Variant 1: for my $num (@numbers)
                    if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme.StartsWith("@"))
                    {
                        string arrayName = _currentToken.Lexeme;
                        Eat(PerlToken.IDENT);
                        Eat(PerlToken.RPAREN); // Consume ')'
                        Eat(PerlToken.LBRACE); // Consume '{'
                        ASTNode body = ParseBlock();
                        Eat(PerlToken.RBRACE); // Consume '}'

                        return new ForeachNode(loopVariable, new VariableNode(arrayName), body);
                    }
                    // Variant 2: for my $i (1..$n)
                    else
                    {
                        ASTNode start = Expr(); // Parse start of range
                        Eat(PerlToken.LOOP_DOT); // Consume '..'
                        ASTNode end = Expr(); // Parse end of range
                        Eat(PerlToken.RPAREN); // Consume ')'
                        Eat(PerlToken.LBRACE); // Consume '{'
                        ASTNode body = ParseBlock();
                        Eat(PerlToken.RBRACE); // Consume '}'

                        return new ForRangeNode(loopVariable, start, end, body);
                    }
                }
            }
            else if (_currentToken.TokenType == PerlToken.LPAREN)
            {
                // Variant 3: for (my $i = 0; $i < @numbers; $i++)
                Eat(PerlToken.LPAREN); // Consume '('
                ASTNode initialization = Assignment(); // Parse initialization
                Eat(PerlToken.SEMICOLON); // Consume ';'
                ASTNode condition = Expr(); // Parse condition
                Eat(PerlToken.SEMICOLON); // Consume ';'
                ASTNode increment = Assignment(); // Parse increment
                Eat(PerlToken.RPAREN); // Consume ')'
                Eat(PerlToken.LBRACE); // Consume '{'
                ASTNode body = ParseBlock(); // Parse loop body
                Eat(PerlToken.RBRACE); // Consume '}'

                return new ForNode(initialization, condition, increment, body);
            }

            throw new Exception("Invalid 'for' loop syntax.");
        }

        
        private ASTNode ForeachStatement()
        {
            Eat(PerlToken.FOREACH);
            Eat(PerlToken.MY);

            // Parse the loop variable (e.g., $key)
            string variable = _currentToken.Lexeme;
            Eat(PerlToken.IDENT);
            ASTNode variableDeclaration = new VariableDeclarationNode("my", variable, null);

            Eat(PerlToken.LPAREN);

            // Parse the iterable (e.g., keys %person or @array)
            ASTNode array;
            if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme == "keys")
            {
                Eat(PerlToken.IDENT); // Consume 'keys'

                // Parse the hash reference (e.g., %person)
                if (_currentToken.TokenType == PerlToken.IDENT && _currentToken.Lexeme.StartsWith("%"))
                {
                    string hashName = _currentToken.Lexeme;
                    Eat(PerlToken.IDENT);

                    array = new HashKeysNode(hashName);
                }
                else
                {
                    throw new Exception($"Expected hash reference (e.g., %person) after 'keys', got {_currentToken.TokenType}.");
                }
            }
            else
            {
                // Handle regular foreach loops (e.g., foreach my $var (@array))
                array = new VariableNode(_currentToken.Lexeme);
                Eat(PerlToken.IDENT);
            }

            Eat(PerlToken.RPAREN);
            Eat(PerlToken.LBRACE);

            // Parse the loop body
            ASTNode body = ParseBlock();

            Eat(PerlToken.RBRACE);

            // Return the updated ForeachNode
            return new ForeachNode(variableDeclaration, array, body);
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
            Eat(PerlToken.FUNC_SUB);
            if (_currentToken.TokenType != PerlToken.IDENT)
            {
                throw new Exception($"Expected function name, got {_currentToken.TokenType}");
            }

            string functionName = _currentToken.Lexeme;
            Eat(PerlToken.IDENT);

            List<ASTNode> parameters = new List<ASTNode>();
            Dictionary<string, string> shiftMappings = new Dictionary<string, string>();
            bool isMethod = false;

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

                                if (paramName == "$self")
                                {
                                    isMethod = true; // Если есть $self, это метод
                                }

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
            else
            {
                // If no parentheses, assume arguments will be accessed via @_ array
                parameters.Add(new VariableNode("@_"));
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
            foreach (var statement in ((BlockNode)body).Statements)
            {
                if (statement is VariableDeclarationNode dec2 &&
                    dec2.Keyword == "my" && dec2.Variable == "$class")
                {
                    isMethod = true;
                    break;
                }

                if (statement is VariableDeclarationNode decl &&
                    decl.Keyword == "my" && decl.Variable == "$self" &&
                    decl.Value is KeywordNode kw && kw.Keyword == "shift")
                {
                    isMethod = true;
                    break;
                }
                if (statement is AssignmentNode assign &&
                    assign.Variable == "$self" &&
                    assign.Value is KeywordNode rightKw && rightKw.Keyword == "shift")
                {
                    isMethod = true;
                    break;
                }

    
                if (statement is KeywordNode keywordNode && keywordNode.Keyword == "shift")
                {
                    // Assign a unique identifier for the shift usage
                    string shiftId = $"param_{shiftMappings.Count + 1}";
                    shiftMappings[shiftId] = "@_"; // Map shift to the @_ array
                }
            }

            Eat(PerlToken.RBRACE);

            var functionNode = new FunctionNode(functionName, parameters, body, isMethod, null);
            return functionNode;
        }

        private ASTNode ClassFunctionDeclaration()
        {
            Eat(PerlToken.FUNC_SUB);

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