using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTRAN.LR2
{
    public enum TokenMathType
    {
        KEYWORD,
        TOKEN
    }

    public class PerlLexer
    {
        public TokenDictionary _tokenDicnionary = new TokenDictionary();
        public List<Token> tokens = new List<Token>();
        public Dictionary<MTRAN.LR2.PerlToken, List<(int line, int column)>> tokenBuffer = new Dictionary<MTRAN.LR2.PerlToken, List<(int line, int column)>>();
        private string _input;
        private int _index;
        private int _line;
        private int _column;

        public PerlLexer(string input)
        {
            _input = input;
            _index = 0;
            _line = 1;
            _column = 1;
        }

        public void PrintTokens()
        {
            Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10}", "ID", "Token Type", "Lexeme", "Line", "Column");
            Console.WriteLine(new string('-', 60));
            int id = 0;
            foreach (var token in tokens)
            {
                token.Id = id;
                Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10}", id, token.TokenType, token.Lexeme, token.Line, token.Column);
                id++;
            }
        }

        public List<Token> Tokenize()
        {
            var lines = _input.Split('\n');
            var stack = new Stack<(char, int, int)>();
            int id = 0;

            foreach (var line in lines)
            {
                _index = 0;
                while (_index < line.Length)
                {
                    var currentChar = line[_index];
                    
                    if (char.IsWhiteSpace(currentChar))
                    {
                        HandleWhitespace(line);
                        continue;
                    }

                    if (char.IsDigit(currentChar) || (currentChar == '0' && _index + 1 < line.Length && 
                        (line[_index + 1] == 'x' || line[_index + 1] == 'X' || 
                        line[_index + 1] == 'o' || line[_index + 1] == 'O' || 
                        line[_index + 1] == 'b' || line[_index + 1] == 'B')))
                    {
                        var numberToken = LexNumber(line);
                        Console.WriteLine(numberToken.Lexeme);
                        tokens.Add(numberToken);
                        continue;
                    }

                    var punctuationToken = IsPunctuation(currentChar);
                    if (punctuationToken.HasValue)
                    {
                        var token = new Token(punctuationToken.Value, currentChar.ToString(), _line, _column);
                        token.Id = id++;
                        tokens.Add(token);

                        if (currentChar == '(' || currentChar == '{' || currentChar == '[' || currentChar == '\'' || currentChar == '"')
                        {
                            stack.Push((currentChar, _line, _column));
                        }
                        else if (currentChar == ')' || currentChar == '}' || currentChar == ']')
                        {
                            if (stack.Count == 0 || !IsMatchingPair(stack.Peek().Item1, currentChar))
                            {
                                var errorToken = new Token(PerlToken.ILLEGAL, currentChar.ToString(), _line, _column)
                                {
                                    Id = id++,
                                    Error = $"Unmatched closing '{currentChar}' at line {_line}, column {_column}"
                                };
                                tokens.Add(errorToken);
                            }
                            else
                            {
                                stack.Pop();
                            }
                        }
                        _index++;
                        _column++;
                        continue;
                    }

                    var (operatorToken, length) = IsOperator(line);
                    if (operatorToken.HasValue)
                    {
                        tokens.Add(new Token(operatorToken.Value, line.Substring(_index, length), _line, _column));
                        _index += length;
                        _column += length;
                        continue;
                    }

                    bool matched = false;
                    matched = RegexIdentifier(line, TokenMathType.KEYWORD);

                    if (!matched && (char.IsLetter(currentChar)))
                    {
                        var identifierToken = LexIdentifier(line);
                        tokens.Add(identifierToken);
                        matched = true;
                    }

                    if (!matched)
                    {
                        matched = RegexIdentifier(line, TokenMathType.TOKEN);
                    }


                    if (!matched)
                    {
                        var errorToken = new Token(PerlToken.ILLEGAL, currentChar.ToString(), _line, _column)
                        {
                            Id = id++,
                            Error = $"Unexpected character '{currentChar}' at line {_line}, column {_column}"
                        };
                        tokens.Add(errorToken);
                        _index++;
                        _column++;
                    }
                }

                _line++;
                _column = 1;
            }

            while (stack.Count > 0)
            {
                var (unclosedChar, line, column) = stack.Pop();
                var errorToken = new Token(PerlToken.ILLEGAL, unclosedChar.ToString(), line, column)
                {
                    Id = id++,
                    Error = $"Unclosed '{unclosedChar}' at line {line}, column {column}"
                };
                tokens.Add(errorToken);
            }

            tokens.Add(new Token(PerlToken.EOF_, "", _line, _column));
            return tokens;
        }

        private (PerlToken? token, int length) IsOperator(string line)
        {
            switch (line[_index])
            {
                case '+':
                    if (_index + 1 < line.Length && line[_index + 1] == '+')
                        return (PerlToken.INC, 2);
                    return (line[_index + 1] == '=' ? PerlToken.ADD_ASSIGN : PerlToken.ADD, line[_index + 1] == '=' ? 2 : 1);
                case '-':
                    if (_index + 1 < line.Length && line[_index + 1] == '-')
                        return (PerlToken.DEC, 2);
                    return (line[_index + 1] == '=' ? PerlToken.SUB_ASSIGN : PerlToken.SUB, line[_index + 1] == '=' ? 2 : 1);
                case '*':
                    return (line[_index + 1] == '=' ? PerlToken.MUL_ASSIGN : PerlToken.MUL, line[_index + 1] == '=' ? 2 : 1);
                case '/':
                    return (line[_index + 1] == '=' ? PerlToken.DIV_ASSIGN : PerlToken.DIV, line[_index + 1] == '=' ? 2 : 1);
                case '!':
                    return (line[_index + 1] == '=' ? PerlToken.NOT_EQUAL : PerlToken.LNOT, line[_index + 1] == '=' ? 2 : 1);
                case '<':
                    return (line[_index + 1] == '=' ? PerlToken.LESS_OR_EQUAL : PerlToken.LESS, line[_index + 1] == '=' ? 2 : 1);
                case '>':
                    return (line[_index + 1] == '=' ? PerlToken.GRT_OR_EQUAL : PerlToken.GRT, line[_index + 1] == '=' ? 2 : 1);
                case '&':
                    return (line[_index + 1] == '&' ? PerlToken.LAND : PerlToken.BITWISE_AND, line[_index + 1] == '&' ? 2 : 1);
                case '|':
                    return (line[_index + 1] == '|' ? PerlToken.LOR : PerlToken.BITWISE_OR, line[_index + 1] == '|' ? 2 : 1);
                case '^':
                    return (PerlToken.BITWISE_XOR, 1);
                case '~':
                    return (PerlToken.BITWISE_NOT, 1);
                 default:
                    if (line.Substring(_index).StartsWith("or "))
                        return (PerlToken.OR, 2);
                    if (line.Substring(_index).StartsWith("and "))
                        return (PerlToken.AND, 3);
                    if (line.Substring(_index).StartsWith("not "))
                        return (PerlToken.NOT, 3);
                    if (line.Substring(_index).StartsWith("xor "))
                        return (PerlToken.XOR, 3);
                    return (null, 0);
            }
        }


        private PerlToken? IsPunctuation(char c)
        {
            switch (c)
            {
                case ',':
                    return PerlToken.COMMA;
                case ';':
                    return PerlToken.SEMICOLON;
                case '.':
                    return PerlToken.DOT;
                case '(':
                    return PerlToken.LPAREN;
                case ')':
                    return PerlToken.RPAREN;
                case '{':
                    return PerlToken.LBRACE;
                case '}':
                    return PerlToken.RBRACE;
                case '[':
                    return PerlToken.LBRACKET;
                case ']':
                    return PerlToken.RBRACKET;
                case ':':
                    return PerlToken.COLON;
                default:
                    return null;
            }
        }

        private void HandleWhitespace(string line)
        {
            char currentChar = line[_index];
            if (currentChar == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _index++;
        }

        private Token LexNumber(string line)
        {
            int start = _index;

            if (_index + 1 < line.Length && line[_index] == '0')
            {
                char nextChar = line[_index + 1];
                if (nextChar == 'x' || nextChar == 'X')
                {
                    _index += 2;
                    while (_index < line.Length && Uri.IsHexDigit(line[_index]))
                        _index++;

                    string hexValue = line.Substring(start, _index - start);
                    var hexToken = new Token(PerlToken.HEX, hexValue, _line, _column);
                    _column += hexValue.Length;
                    return hexToken;
                }
                else if (nextChar == 'o' || nextChar == 'O')
                {
                    _index += 2;
                    while (_index < line.Length && line[_index] >= '0' && line[_index] <= '7')
                        _index++;

                    string octValue = line.Substring(start, _index - start);
                    var octToken = new Token(PerlToken.OCT, octValue, _line, _column);
                    _column += octValue.Length;
                    return octToken;
                }
                else if (nextChar == 'b' || nextChar == 'B')
                {
                    _index += 2;
                    while (_index < line.Length && (line[_index] == '0' || line[_index] == '1'))
                        _index++;

                    string binValue = line.Substring(start, _index - start);
                    var binToken = new Token(PerlToken.BIN, binValue, _line, _column);
                    _column += binValue.Length;
                    return binToken;
                }
            }

            while (_index < line.Length && (char.IsDigit(line[_index]) || line[_index] == '.'))
                _index++;

            if (_index < line.Length && (line[_index] == 'e' || line[_index] == 'E'))
            {
                _index++;
                if (_index < line.Length && (line[_index] == '+' || line[_index] == '-'))
                    _index++;

                while (_index < line.Length && char.IsDigit(line[_index]))
                    _index++;
            }

            string numberValue = line.Substring(start, _index - start);
            var numberToken = new Token(PerlToken.NUMBER, numberValue, _line, _column);
            _column += numberValue.Length;
            return numberToken;
        }

        private Token LexIdentifier(string line)
        {
            int start = _index;
            while (_index < line.Length && (char.IsLetterOrDigit(line[_index]) || line[_index] == '_'))
                _index++;

            string value = line.Substring(start, _index - start);

            foreach (var pattern in _tokenDicnionary.KeywordPatterns)
            {
                var regex = new Regex(pattern.Value);
                if (regex.IsMatch(value))
                {
                    var token = new Token(pattern.Key, value, _line, _column);
                    _column += value.Length;
                    return token;
                }
            }

            var identifierToken = new Token(PerlToken.IDENT, value, _line, _column);
            _column += value.Length;
            return identifierToken;
        }

        private bool RegexIdentifier(string line, TokenMathType tokenType)
        {
            Console.WriteLine("Ты шлюха не моя");
            var patterns = tokenType == TokenMathType.KEYWORD ? _tokenDicnionary.KeywordPatterns : _tokenDicnionary.TokenPatterns;

            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern.Value);
                var match = regex.Match(line.Substring(_index));

                if (match.Success)
                {
                    Console.WriteLine(pattern.Key);
                    string value = match.Value;
                    tokens.Add(new Token(pattern.Key, match.Value, _line, _column));
                    _index += match.Length;
                    _column += match.Length;
                    if (!tokenBuffer.ContainsKey(pattern.Key))
                    {
                        tokenBuffer[pattern.Key] = new List<(int line, int column)>();
                    }
                    
                    bool exists = tokenBuffer[pattern.Key].Exists(t => t.line == _line && t.column == _column);
                    if (!exists)
                    {
                        tokenBuffer[pattern.Key].Add((_line, _column));
                    }
                    return true;
                }
            }

            return false;
        }

       public void SaveModifiedCode(string filePath)
        {
            var modifiedCode = new List<string>();
            var identifierMap = new Dictionary<string, int>();

            foreach (var line in _input.Split('\n'))
            {
                var modifiedLine = line;
                foreach (var token in tokens)
                {
                    if (token.TokenType == PerlToken.IDENT)
                    {
                        if (!identifierMap.ContainsKey(token.Lexeme))
                        {
                            identifierMap[token.Lexeme] = token.Id;
                        }
                        modifiedLine = modifiedLine.Replace(token.Lexeme, $"<{identifierMap[token.Lexeme]}>");
                    }
                }
                modifiedCode.Add(modifiedLine);
            }

            System.IO.File.WriteAllLines(filePath, modifiedCode);

            var errors = tokens.Where(t => !string.IsNullOrEmpty(t.Error)).Select(t => t.Error).ToList();
            if (errors.Any())
            {
                System.IO.File.WriteAllLines("errors.txt", errors);
            }
        }

        private bool IsMatchingPair(char open, char close)
        {
            return (open == '(' && close == ')') ||
                (open == '{' && close == '}') ||
                (open == '[' && close == ']') ||
                (open == '\'' && close == '\'') ||
                (open == '"' && close == '"');
        }
        
    }
}