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

        public List<Token> Tokenize()
        {
            var lines = _input.Split('\n');
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

                    if (char.IsDigit(currentChar))
                    {
                        var numberToken = LexNumber(line);
                        Console.WriteLine(numberToken.Lexeme);
                        tokens.Add(numberToken);
                        continue;
                    }

                    var punctuationToken = IsPunctuation(currentChar);
                    if (punctuationToken.HasValue)
                    {
                        tokens.Add(new Token(punctuationToken.Value, currentChar.ToString(), _line, _column));
                        _index++;
                        _column++;
                        continue;
                    }

                    // var operatorToken = IsOperator(line);
                    // if (operatorToken.HasValue)
                    // {
                    //     tokens.Add(new Token(operatorToken.Value, line.Substring(_index, operatorToken.Value.ToString().Length), _line, _column));
                    //     _index += operatorToken.Value.ToString().Length;
                    //     _column += operatorToken.Value.ToString().Length;
                    //     continue;
                    // }

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
                        tokens.Add(new Token(PerlToken.ILLEGAL, currentChar.ToString(), _line, _column));
                        _index++;
                        _column++;
                    }
                }

                _line++;
                _column = 1;
            }

            tokens.Add(new Token(PerlToken.EOF_, "", _line, _column));
            return tokens;
        }

        private PerlToken? IsOperator(string line)
        {
            switch (line[_index])
            {
                case '+':
                    return line[_index + 1] == '=' ? PerlToken.ADD_ASSIGN : PerlToken.ADD;
                case '-':
                    return line[_index + 1] == '=' ? PerlToken.SUB_ASSIGN : PerlToken.SUB;
                case '*':
                    return line[_index + 1] == '=' ? PerlToken.MUL_ASSIGN : PerlToken.MUL;
                case '/':
                    return line[_index + 1] == '=' ? PerlToken.DIV_ASSIGN : PerlToken.DIV;
                case '!':
                    return line[_index + 1] == '=' ? PerlToken.NOT_EQUAL : PerlToken.LNOT;
                case '<':
                    return line[_index + 1] == '=' ? PerlToken.LESS_OR_EQUAL : PerlToken.LESS;
                case '>':
                    return line[_index + 1] == '=' ? PerlToken.GRT_OR_EQUAL : PerlToken.GRT;
                case '&':
                    return line[_index + 1] == '&' ? PerlToken.LAND : PerlToken.BITWISE_AND;
                case '|':
                    return line[_index + 1] == '|' ? PerlToken.LOR : PerlToken.BITWISE_OR;
                case '^':
                    return PerlToken.BITWISE_XOR;
                case '~':
                    return PerlToken.BITWISE_NOT;
                default:
                    return null;
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
            Console.WriteLine("Pidoras");
            int start = _index;
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

            string value = line.Substring(start, _index - start);
            var token = new Token(PerlToken.NUMBER, value, _line, _column);
            _column += value.Length;
            return token;
        }

        private Token LexIdentifier(string line)
        {
            int start = _index;
            while (_index < line.Length && (char.IsLetterOrDigit(line[_index]) || line[_index] == '_'))
                _index++;

            string value = line.Substring(start, _index - start);

            // Check if the value matches any keyword patterns using regex
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

            var identifierToken = new Token(PerlToken.VAR, value, _line, _column);
            _column += value.Length;
            return identifierToken;
        }

        private bool RegexIdentifier(string line, TokenMathType tokenType)
        {
            var patterns = tokenType == TokenMathType.KEYWORD ? _tokenDicnionary.KeywordPatterns : _tokenDicnionary.TokenPatterns;

            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern.Value);
                var match = regex.Match(line.Substring(_index));

                if (match.Success)
                {
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
    }
}