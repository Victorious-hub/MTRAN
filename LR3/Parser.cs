using System;
using System.Collections.Generic;

namespace MTRAN.LR3
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
            Console.WriteLine(token.Lexeme);
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
            else
            {
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
            ASTNode node = Term();

            while (
                _currentToken.TokenType == PerlToken.ADD || 
                _currentToken.TokenType == PerlToken.SUB ||
                _currentToken.TokenType == PerlToken.MUL ||
                _currentToken.TokenType == PerlToken.DIV ||
                _currentToken.TokenType == PerlToken.MOD ||
                _currentToken.TokenType == PerlToken.LESS ||
                _currentToken.TokenType == PerlToken.GRT ||
                _currentToken.TokenType == PerlToken.LESS_OR_EQUAL ||
                _currentToken.TokenType == PerlToken.GRT_OR_EQUAL ||
                _currentToken.TokenType == PerlToken.EQUAL ||
                _currentToken.TokenType == PerlToken.NOT_EQUAL
            ) 
            {
                Token token = _currentToken;
                if (token.TokenType == PerlToken.ADD)
                {
                    Eat(PerlToken.ADD);
                }
                else if (token.TokenType == PerlToken.SUB)
                {
                    Eat(PerlToken.SUB);
                }
                else if (token.TokenType == PerlToken.LESS)
                {
                    Eat(PerlToken.LESS);
                }
                else if (token.TokenType == PerlToken.GRT)
                {
                    Eat(PerlToken.GRT);
                }
                else if (token.TokenType == PerlToken.LESS_OR_EQUAL)
                {
                    Eat(PerlToken.LESS_OR_EQUAL);
                }
                else if (token.TokenType == PerlToken.GRT_OR_EQUAL)
                {
                    Eat(PerlToken.GRT_OR_EQUAL);
                }
                else if (token.TokenType == PerlToken.EQUAL)
                {
                    Eat(PerlToken.EQUAL);
                }
                else if (token.TokenType == PerlToken.NOT_EQUAL)
                {
                    Eat(PerlToken.NOT_EQUAL);
                }

                node = new BinaryOperationNode(node, token.Lexeme, Term());
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

            return new IfNode(condition, thenBranch, elseIfBranches, elseBranch);
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

                if (_currentToken.TokenType == PerlToken.SEMICOLON)
                {
                    Eat(PerlToken.SEMICOLON);
                }
                else if (_currentToken.TokenType != PerlToken.RBRACE && _currentToken.TokenType != PerlToken.EOF_)
                {
                    throw new Exception("Unexpected token after statement");
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
            else
            {
                return Assignment();
            }
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

            if (_currentToken.TokenType == PerlToken.IDENT)
            {
                Token variableToken = _currentToken;
                Eat(PerlToken.IDENT);
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
            else
            {
                return null;
            }
        }
    }

    
}