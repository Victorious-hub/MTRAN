using System;
using System.Collections.Generic;

namespace MTRAN.Parser
{
    public class Parser
    {
        private readonly PerlLexer _lexer;
        private Token _currentToken;
        private int _currentTokenIndex;

        public Parser(PerlLexer lexer)
        {
            _lexer = lexer;
            _currentTokenIndex = 0;
            _currentToken = _lexer.tokens[_currentTokenIndex];
        }

        private void Eat(PerlToken tokenType)
        {
            if (_currentToken.TokenType == tokenType)
            {
                _currentTokenIndex++;
                if (_currentTokenIndex < _lexer.tokens.Count)
                {
                    _currentToken = _lexer.tokens[_currentTokenIndex];
                }
            }
            else
            {
                throw new Exception($"Expected token {tokenType}, got {_currentToken.TokenType}");
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
                Eat(PerlToken.IDENT);
                if (_currentToken.TokenType == PerlToken.ASSIGN)
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
                Console.WriteLine(token.TokenType);
                throw new Exception("Invalid factor");
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
            else
            {
                return Expr();
            }
        }

        private ASTNode ReturnStatement()
        {
            Eat(PerlToken.RETURN);
            ASTNode value = Expr();
            return new ReturnNode(value);
        }

        public ASTNode Parse()
        {
            return ParseBlock();
        }

        private ASTNode Assignment()
        {
            bool isDeclaration = false;
            string keyword = null;

            if (_currentToken.TokenType == PerlToken.MY)
            {
                Eat(PerlToken.MY);
                isDeclaration = true;
                keyword = "my";
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
                else if (_currentToken.TokenType == PerlToken.INC)
                {
                    Eat(PerlToken.INC);
                    return new UnaryOperationNode(variableToken.Lexeme, "++");
                }
                else if (_currentToken.TokenType == PerlToken.DEC)
                {
                    Eat(PerlToken.DEC);
                    return new UnaryOperationNode(variableToken.Lexeme, "--");
                }
                else if (_currentToken.TokenType == PerlToken.ADD_ASSIGN || _currentToken.TokenType == PerlToken.SUB_ASSIGN ||
                 _currentToken.TokenType == PerlToken.MUL_ASSIGN || _currentToken.TokenType == PerlToken.DIV_ASSIGN)
                {
                    Token operatorToken = _currentToken;
                    Eat(operatorToken.TokenType);
                    ASTNode value = Expr();
                    return new CompoundAssignmentNode(variableToken.Lexeme, operatorToken.Lexeme, value);
                }
                else
                {
                    throw new Exception($"Expected token ASSIGN, INC, DEC, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, or MOD_ASSIGN, got {_currentToken.TokenType}");
                }
            }
            else
            {
                return null;
            }
        }

        private ASTNode ForStatement()
        {
            Eat(PerlToken.FOR);
            Eat(PerlToken.LPAREN);

            ASTNode initialization = Assignment();
            Eat(PerlToken.SEMICOLON);
            ASTNode condition = Expr();
            Eat(PerlToken.SEMICOLON);
            ASTNode increment = Assignment();
            Eat(PerlToken.RPAREN);

            ASTNode forExpression = new ParenthesizedExpression(
                new StatementListNode(new List<ASTNode> { initialization, condition, increment })
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

            Eat(PerlToken.LBRACE);
            ASTNode body = ParseBlock();
            Eat(PerlToken.RBRACE);

            return new FunctionNode(functionName, parameters, body);
        }
    }
}