using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTRAN.Parser
{
    public enum TokenMathType
    {
        KEYWORD,
        TOKEN
    }

    public class PerlLexerParser
    {
        public Dictionary<string, List<(PerlToken token, int line, int column, int id)>> outputTokens = new Dictionary<string, List<(PerlToken token, int line, int column, int id)>>();
        public TokenDictionary _tokenDicnionary = new TokenDictionary();
        public List<Token> tokens = new List<Token>();
        public Dictionary<MTRAN.Parser.PerlToken, List<(int line, int column)>> tokenBuffer = new Dictionary<MTRAN.Parser.PerlToken, List<(int line, int column)>>();
        private string _input;
        private int _index;
        private int _line;
        private int _column;
        private Stack<(char unclosedChar, int line, int column)> stack = new Stack<(char, int, int)>();

        public readonly Dictionary<PerlToken, string> TokenDescriptions = new()
        {
            { PerlToken.ILLEGAL, "Illegal token" },
            { PerlToken.EOF_, "End of file" },
            { PerlToken.COMMENT, "Comment" },
            { PerlToken.INT, "Integer" },
            { PerlToken.HEX, "Hexadecimal" },
            { PerlToken.OCT, "Octal" },
            { PerlToken.BIN, "Binary" },
            { PerlToken.NUMBER, "Number" },
            { PerlToken.IMAG, "Imaginary number" },
            { PerlToken.STRING, "String" },
            { PerlToken.OR, "Logical OR" },
            { PerlToken.AND, "Logical AND" },
            { PerlToken.NOT, "Logical NOT" },
            { PerlToken.XOR, "Logical XOR" },
            { PerlToken.LOR, "Logical OR (||)" },
            { PerlToken.LAND, "Logical AND (&&)" },
            { PerlToken.LNOT, "Logical NOT (!)" },
            { PerlToken.IDENT, "Identifier" },
            { PerlToken.BITWISE_XOR, "Bitwise XOR (^)" },
            { PerlToken.BITWISE_AND, "Bitwise AND (&)" },
            { PerlToken.BITWISE_OR, "Bitwise OR (|)" },
            { PerlToken.BITWISE_NOT, "Bitwise NOT (~)" },
            { PerlToken.BITWISE_LEFT, "Bitwise left shift (<<)" },
            { PerlToken.BITWISE_RIGHT, "Bitwise right shift (>>)" },
            { PerlToken.BIT_CLEAR, "Bitwise clear (&^)" },
            { PerlToken.ADD, "Addition (+)" },
            { PerlToken.SUB, "Subtraction (-)" },
            { PerlToken.MUL, "Multiplication (*)" },
            { PerlToken.DIV, "Division (/)" },
            { PerlToken.MOD, "Modulus (%)" },
            { PerlToken.INC, "Increment (++)" },
            { PerlToken.DEC, "Decrement (--)" },
            { PerlToken.ADD_ASSIGN, "Addition assignment (+=)" },
            { PerlToken.SUB_ASSIGN, "Subtraction assignment (-=)" },
            { PerlToken.MUL_ASSIGN, "Multiplication assignment (*=)" },
            { PerlToken.DIV_ASSIGN, "Division assignment (/=)" },
            { PerlToken.MO_ASSIGND, "Modulus assignment (%=)" },
            { PerlToken.AND_ASSIGN, "Bitwise AND assignment (&=)" },
            { PerlToken.OR_ASSIGN, "Bitwise OR assignment (|=)" },
            { PerlToken.XOR_ASSIGN, "Bitwise XOR assignment (^=)" },
            { PerlToken.BITWISE_LEFT_ASSIGN, "Bitwise left shift assignment (<<=)" },
            { PerlToken.BITWISE_RIGHT_ASSIGN, "Bitwise right shift assignment (>>=)" },
            { PerlToken.AND_NOT_ASSIGN, "Bitwise clear assignment (&^=)" },
            { PerlToken.EQUAL, "Equality (==)" },
            { PerlToken.ASSIGN, "Assignment (=)" },
            { PerlToken.NOT_EQUAL, "Not equal (!=)" },
            { PerlToken.LESS, "Less than (<)" },
            { PerlToken.LESS_OR_EQUAL, "Less than or equal (<=)" },
            { PerlToken.GRT, "Greater than (>)" },
            { PerlToken.GRT_OR_EQUAL, "Greater than or equal (>=)" },
            { PerlToken.DOT, "Dot (.)" },
            { PerlToken.LPAREN, "Left parenthesis (()"},
            { PerlToken.RPAREN, "Right parenthesis ())"},
            { PerlToken.LBRACE, "Left brace ({)" },
            { PerlToken.RBRACE, "Right brace (})" },
            { PerlToken.LBRACKET, "Left bracket ([)" },
            { PerlToken.RBRACKET, "Right bracket (])" },
            { PerlToken.COMMA, "Comma (,)" },
            { PerlToken.SEMICOLON, "Semicolon (;)" },
            { PerlToken.COLON, "Colon (:)" },
            { PerlToken.ELLIPSIS, "Ellipsis (...)" },
            { PerlToken.HASH_ASSIGN, "Hash assignment (=>)" },
            { PerlToken.IF, "If" },
            { PerlToken.ELSE, "Else" },
            { PerlToken.ELSIF, "Else if" },
            { PerlToken.RETURN, "Return" },
            { PerlToken.PRINT, "Print" },
            { PerlToken.MY, "My" },
            { PerlToken.FOR, "For" },
            { PerlToken.UNTIL, "Until" },
            { PerlToken.WHILE, "While" },
            { PerlToken.FOREACH, "Foreach" },
            { PerlToken.GOTO, "Goto" },
            { PerlToken.PACKAGE, "Package" },
            { PerlToken.FUNC_SUB, "Function declaration (sub)" },
            { PerlToken.USE, "Use" }
        };

        public PerlLexerParser(string input)
        {
            _input = input;
            _index = 0;
            _line = 1;
            _column = 1;
        }

        public void PrintTokens()
        {
            var identifiers = new List<Token>();
            var constants = new List<Token>();
            var delimiters = new List<Token>();
            var operators = new List<Token>();
            var keywords = new List<Token>();

            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case PerlToken.IDENT:
                        identifiers.Add(token);
                        break;
                    case PerlToken.NUMBER:
                    case PerlToken.STRING:
                    case PerlToken.HEX:
                    case PerlToken.OCT:
                    case PerlToken.BIN:
                        constants.Add(token);
                        break;
                    case PerlToken.COMMA:
                    case PerlToken.SEMICOLON:
                    case PerlToken.DOT:
                    case PerlToken.LPAREN:
                    case PerlToken.RPAREN:
                    case PerlToken.LBRACE:
                    case PerlToken.RBRACE:
                    case PerlToken.LBRACKET:
                    case PerlToken.RBRACKET:
                    case PerlToken.COLON:
                        delimiters.Add(token);
                        break;
                    case PerlToken.ADD:
                    case PerlToken.SUB:
                    case PerlToken.MUL:
                    case PerlToken.DIV:
                    case PerlToken.ASSIGN:
                    case PerlToken.ADD_ASSIGN:
                    case PerlToken.SUB_ASSIGN:
                    case PerlToken.MUL_ASSIGN:
                    case PerlToken.DIV_ASSIGN:
                    case PerlToken.INC:
                    case PerlToken.DEC:
                    case PerlToken.LAND:
                    case PerlToken.LOR:
                    case PerlToken.LNOT:
                    case PerlToken.BITWISE_AND:
                    case PerlToken.BITWISE_OR:
                    case PerlToken.BITWISE_XOR:
                    case PerlToken.BITWISE_NOT:
                    case PerlToken.LESS:
                    case PerlToken.LESS_OR_EQUAL:
                    case PerlToken.GRT:
                    case PerlToken.GRT_OR_EQUAL:
                    case PerlToken.NOT_EQUAL:
                    case PerlToken.HASH_ASSIGN:
                        operators.Add(token);
                        break;
                    default:
                        if (_tokenDicnionary.KeywordPatterns.ContainsKey(token.TokenType))
                        {
                            keywords.Add(token);
                        }
                        break;
                }
            }

            Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10}", "ID", "Token Type", "Lexeme", "Line", "Column");
            Console.WriteLine(new string('-', 60));

            PrintCategory("Identifiers", identifiers);
            PrintCategory("Constants", constants);
            PrintCategory("Delimiters", delimiters);
            PrintCategory("Operators", operators);
            PrintCategory("Keywords", keywords);
        }

        private void PrintCategory(string category, List<Token> tokens)
        {
            Console.WriteLine($"\n{category}:");
            foreach (var token in tokens)
            {
                Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10}", token.Id, token.TokenType, token.Lexeme, token.Line, token.Column);
            }
        }

        public void PrintUniqueTokens()
        {
            var identifiers = new List<(string lexeme, int line, int column, int id, string description, PerlToken token)>();
            var constants = new List<(string lexeme, int line, int column, int id, string description, PerlToken token)>();
            var delimiters = new List<(string lexeme, int line, int column, int id, string description, PerlToken token)>();
            var operators = new List<(string lexeme, int line, int column, int id, string description, PerlToken token)>();
            var keywords = new List<(string lexeme, int line, int column, int id, string description, PerlToken token)>();

            foreach (var kvp in outputTokens)
            {
                var tokenType = kvp.Key;
                var tokenDataList = kvp.Value;

                foreach (var tokenData in tokenDataList)
                {
                    if (tokenData.token == PerlToken.ILLEGAL)
                    {
                        continue;
                    }

                    var description = TokenDescriptions.ContainsKey(tokenData.token) ? TokenDescriptions[tokenData.token] : "Unknown";

                    switch (tokenData.token)
                    {
                        case PerlToken.IDENT:
                            identifiers.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.INT:
                            constants.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.NUMBER:
                            constants.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.STRING:
                            constants.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.HEX:
                            constants.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.OCT:
                            constants.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.BIN:
                            constants.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.COMMA:
                        case PerlToken.SEMICOLON:
                        case PerlToken.DOT:
                        case PerlToken.LPAREN:
                        case PerlToken.RPAREN:
                        case PerlToken.LBRACE:
                        case PerlToken.RBRACE:
                        case PerlToken.LBRACKET:
                        case PerlToken.RBRACKET:
                        case PerlToken.COLON:
                            delimiters.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        case PerlToken.ADD:
                        case PerlToken.SUB:
                        case PerlToken.MUL:
                        case PerlToken.DIV:
                        case PerlToken.ASSIGN:
                        case PerlToken.ADD_ASSIGN:
                        case PerlToken.SUB_ASSIGN:
                        case PerlToken.MUL_ASSIGN:
                        case PerlToken.DIV_ASSIGN:
                        case PerlToken.INC:
                        case PerlToken.DEC:
                        case PerlToken.LAND:
                        case PerlToken.LOR:
                        case PerlToken.LNOT:
                        case PerlToken.BITWISE_AND:
                        case PerlToken.BITWISE_OR:
                        case PerlToken.BITWISE_XOR:
                        case PerlToken.BITWISE_NOT:
                        case PerlToken.LESS:
                        case PerlToken.LESS_OR_EQUAL:
                        case PerlToken.GRT:
                        case PerlToken.GRT_OR_EQUAL:
                        case PerlToken.NOT_EQUAL:
                        case PerlToken.HASH_ASSIGN:
                        case PerlToken.EQUAL:
                        case PerlToken.REF_HASH:
                        case PerlToken.POINTER_HASH:
                            operators.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            break;
                        default:
                            if (_tokenDicnionary.KeywordPatterns.ContainsKey(tokenData.token))
                            {
                                keywords.Add((tokenType, tokenData.line, tokenData.column, tokenData.id, description, tokenData.token));
                            }
                            break;
                    }
                }
            }

            Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10} {5,-15} {6,-10}", "ID", "Token Type", "Lexeme", "Line", "Column", "Description", "Token");
            Console.WriteLine(new string('-', 90));

            PrintCategory("Identifiers", identifiers);
            PrintCategory("Constants", constants);
            PrintCategory("Delimiters", delimiters);
            PrintCategory("Operators", operators);
            PrintCategory("Keywords", keywords);
        }

        private void PrintCategory(string category, List<(string lexeme, int line, int column, int id, string description, PerlToken token)> tokens)
        {
            Console.WriteLine($"\n{category}:");
            foreach (var token in tokens)
            {
                Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10} {5,-15} {6,-10}", token.id, category, token.lexeme, token.line, token.column, token.description, token.token);
            }
        }

        private void PrintCategory(string category, List<(string lexeme, int line, int column, int id, string type, string description, PerlToken token)> tokens)
        {
            Console.WriteLine($"\n{category}:");
            foreach (var token in tokens)
            {
                Console.WriteLine("{0,-5} {1,-15} {2,-15} {3,-10} {4,-10} {5,-10} {6,-15} {7,-10}", token.id, category, token.lexeme, token.line, token.column, token.type, token.description, token.token);
            }
        }

        public List<Token> Tokenize()
        {
            var lines = _input.Split('\n');
            int id = 0;
            bool inPodComment = false;

            foreach (var line in lines)
            {
                _index = 0;
                while (_index < line.Length)
                {
                    var currentChar = line[_index];

                    if (_index + 1 < line.Length && line[_index] == '-' && line[_index + 1] == '>')
                    {
                        Console.WriteLine($"Found a pointer hash operator at line {_line}, column {_column}");
                        var token = new Token(PerlToken.POINTER_HASH, "->", _line, _column, "Pointer hash operator") { Id = id++ };
                        tokens.Add(token);
                        _index += 2; // Skip the '->'
                        _column += 2;
                        AddToOutputTokens(token);
                        continue;
                    }

                    // Handle long comments
                    if (line.Substring(_index).StartsWith("=pod"))
                    {
                        inPodComment = true;
                        break; // Skip the rest of the line
                    }

                    if (inPodComment)
                    {
                        if (line.Substring(_index).StartsWith("=cut"))
                        {
                            inPodComment = false;
                        }
                        break; // Skip the rest of the line
                    }

                    // Skip comments
                    if (currentChar == '#')
                    {
                        break; // Skip the rest of the line
                    }

                    if (char.IsWhiteSpace(currentChar))
                    {
                        HandleWhitespace(line);
                        continue;
                    }

                    var (operatorToken, length) = IsOperator(line);
                    if (operatorToken.HasValue)
                    {
                        var description = TokenDescriptions.ContainsKey(operatorToken.Value) ? TokenDescriptions[operatorToken.Value] : "Operator";
                        var token = new Token(operatorToken.Value, line.Substring(_index, length), _line, _column, description) { Id = id++ };
                        tokens.Add(token);
                        AddToOutputTokens(token);
                        _index += length;
                        _column += length;
                        continue;
                    }

                    if (char.IsDigit(currentChar) || (currentChar == '0' && _index + 1 < line.Length && 
                        (line[_index + 1] == 'x' || line[_index + 1] == 'X' || 
                        line[_index + 1] == 'o' || line[_index + 1] == 'O' || 
                        line[_index + 1] == 'b' || line[_index + 1] == 'B')))
                    {
                        var numberToken = LexNumber(line);
                        numberToken.Id = id++;
                        tokens.Add(numberToken);
                        AddToOutputTokens(numberToken);
                        continue;
                    }

                    if (line[_index] == '\\' && _index + 1 < line.Length && line[_index + 1] == '%')
                    {
                        
                        int start = _index;
                        _index += 2; // Skip the '\' and '%'

                        // Collect the hash variable name
                        while (_index < line.Length && (char.IsLetterOrDigit(line[_index]) || line[_index] == '_'))
                        {
                            _index++;
                        }

                        string refHash = line.Substring(start, _index - start);
                        var refHashToken = new Token(PerlToken.REF_HASH, refHash, _line, _column, "Reference to hash");
                        _column += refHash.Length;
                        tokens.Add(refHashToken);
                        Console.WriteLine($"Found a reference to a hash {refHashToken.Lexeme}");
                        AddToOutputTokens(refHashToken);
                        continue;
                    }

                
                    if (currentChar == '$' || currentChar == '@' || currentChar == '%')
                    {
                        int start = _index;
                        _index++;
                        if (_index < line.Length && char.IsDigit(line[_index]))
                        {
                            // Variable starts with a number, mark as ILLEGAL
                            while (_index < line.Length && (char.IsLetterOrDigit(line[_index]) || line[_index] == '_'))
                            {
                                _index++;
                            }
                            string invalidIdentifier = line.Substring(start, _index - start);
                            var errorToken = new Token(PerlToken.ILLEGAL, invalidIdentifier, _line, _column, "Invalid identifier starting with a number")
                            {
                                Id = id++,
                                Error = $"Invalid identifier starting with a number at line {_line}, column {_column}, line: {line}"
                            };
                            tokens.Add(errorToken);
                            AddToOutputTokens(errorToken);
                            _column += invalidIdentifier.Length;
                            continue;
                        }
                        while (_index < line.Length && (char.IsLetterOrDigit(line[_index]) || line[_index] == '_'))
                        {
                            _index++;
                        }
                        string identifier = line.Substring(start, _index - start);
                        var identifierToken = new Token(PerlToken.IDENT, identifier, _line, _column, "Identifier") { Id = id++ };
                        tokens.Add(identifierToken);
                        AddToOutputTokens(identifierToken);
                        _column += identifier.Length;
                        continue;
                    }

                    if (currentChar == '"')
                    {
                        var stringToken = LexString(line);
                        stringToken.Id = id++;
                        tokens.Add(stringToken);
                        AddToOutputTokens(stringToken);
                        continue;
                    }

                    if (char.IsLetter(currentChar) || currentChar == '_')
                    {
                        var identifierToken = LexIdentifier(line);
                        identifierToken.Id = id++;
                        tokens.Add(identifierToken);
                        AddToOutputTokens(identifierToken);
                        continue;
                    }

                    if (!IsSupportedCharacter(currentChar))
                    {
                        var errorToken = new Token(PerlToken.ILLEGAL, currentChar.ToString(), _line, _column, "Unsupported character")
                        {
                            Id = id++,
                            Error = $"Unsupported character '{currentChar}' at line {_line}, column {_column}, line: {line}"
                        };
                        tokens.Add(errorToken);
                        AddToOutputTokens(errorToken);
                        _index++;
                        _column++;
                        continue;
                    }

                    var punctuationToken = IsPunctuation(currentChar);
                    if (punctuationToken.HasValue)
                    {
                        var description = TokenDescriptions.ContainsKey(punctuationToken.Value) ? TokenDescriptions[punctuationToken.Value] : "Punctuation";
                        var token = new Token(punctuationToken.Value, currentChar.ToString(), _line, _column, description) { Id = id++ };
                        tokens.Add(token);
                        AddToOutputTokens(token);
                        _index++;
                        _column++;
                        continue;
                    }

                    bool matched = false;
                    matched = RegexIdentifier(line, TokenMathType.KEYWORD, ref id);

                    if (!matched && (char.IsLetter(currentChar)))
                    {
                        var identifierToken = LexIdentifier(line);
                        identifierToken.Id = id++;
                        tokens.Add(identifierToken);
                        AddToOutputTokens(identifierToken);
                        matched = true;
                    }

                    if (!matched)
                    {
                        matched = RegexIdentifier(line, TokenMathType.TOKEN, ref id);
                    }

                    if (!matched)
                    {
                        var errorToken = new Token(PerlToken.ILLEGAL, currentChar.ToString(), _line, _column, "Unexpected character")
                        {
                            Id = id++,
                            Error = $"Unexpected character '{currentChar}' at line {_line}, column {_column}, line: {line}"
                        };
                        tokens.Add(errorToken);
                        AddToOutputTokens(errorToken);
                        _index++;
                        _column++;
                    }
                }

                _line++;
                _column = 1;

                while (stack.Count > 0)
                {
                    var (unclosedChar, unclosedLine, unclosedColumn) = stack.Pop();
                    var errorToken = new Token(PerlToken.ILLEGAL, unclosedChar.ToString(), unclosedLine, unclosedColumn, "Unclosed character")
                    {
                        Id = id++,
                        Error = $"Unclosed '{unclosedChar}' at line {unclosedLine}, column {unclosedColumn}, line: {line}"
                    };
                    tokens.Add(errorToken);
                    AddToOutputTokens(errorToken);
                }
            }

            if (inPodComment)
            {
                var errorToken = new Token(PerlToken.ILLEGAL, "=pod", _line, _column, "Unclosed pod comment")
                {
                    Id = id++,
                    Error = $"Unclosed pod comment starting at line {_line}"
                };
                tokens.Add(errorToken);
                AddToOutputTokens(errorToken);
            }

            var eofToken = new Token(PerlToken.EOF_, "", _line, _column, "End of file") { Id = id++ };
            tokens.Add(eofToken);
            AddToOutputTokens(eofToken);
            return tokens;
        }

        private void AddToOutputTokens(Token token)
        {
            if (!outputTokens.ContainsKey(token.Lexeme))
            {
                outputTokens[token.Lexeme] = new List<(PerlToken token, int line, int column, int id)>();
            }
            outputTokens[token.Lexeme].Add((token.TokenType, token.Line, token.Column, token.Id));
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
                case '=':
                    if (_index + 1 < line.Length && line[_index + 1] == '=')
                        return (PerlToken.EQUAL, 2);
                    if (_index + 1 < line.Length && line[_index + 1] == '>')
                        return (PerlToken.HASH_ASSIGN, 2);
                    return (PerlToken.ASSIGN, 1);
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
            int decimalPointCount = 0;
            bool isExponent = false;

            if (_index + 1 < line.Length && line[_index] == '0')
            {
                char nextChar = line[_index + 1];
                if (nextChar == 'x' || nextChar == 'X')
                {
                    _index += 2;
                    while (_index < line.Length && Uri.IsHexDigit(line[_index]))
                        _index++;

                    string hexValue = line.Substring(start, _index - start);
                    var hexToken = new Token(PerlToken.HEX, hexValue, _line, _column, "Hexadecimal");
                    _column += hexValue.Length;
                    return hexToken;
                }
                else if (nextChar == 'o' || nextChar == 'O')
                {
                    _index += 2;
                    while (_index < line.Length && line[_index] >= '0' && line[_index] <= '7')
                        _index++;

                    string octValue = line.Substring(start, _index - start);
                    var octToken = new Token(PerlToken.OCT, octValue, _line, _column, "Octal");
                    _column += octValue.Length;
                    return octToken;
                }
                else if (nextChar == 'b' || nextChar == 'B')
                {
                    _index += 2;
                    while (_index < line.Length && (line[_index] == '0' || line[_index] == '1'))
                        _index++;

                    string binValue = line.Substring(start, _index - start);
                    var binToken = new Token(PerlToken.BIN, binValue, _line, _column, "Binary");
                    _column += binValue.Length;
                    return binToken;
                }
            }

            while (_index < line.Length && (char.IsDigit(line[_index]) || line[_index] == '.'))
            {
                if (line[_index] == '.')
                {
                    decimalPointCount++;
                    if (decimalPointCount > 1)
                    {
                        while (_index < line.Length && (char.IsDigit(line[_index]) || line[_index] == '.'))
                        {
                            _index++;
                        }
                        string invalidNumber = line.Substring(start, _index - start);
                        var errorToken = new Token(PerlToken.ILLEGAL, invalidNumber, _line, _column, "Invalid numeric literal")
                        {
                            Error = $"Invalid numeric literal with multiple decimal points at line {_line}, column {_column}, line: {line}"
                        };
                        _column += invalidNumber.Length;
                        return errorToken;
                    }
                }
                _index++;
            }

            if (_index < line.Length && (line[_index] == 'e' || line[_index] == 'E'))
            {
                isExponent = true;
                _index++;
                if (_index < line.Length && (line[_index] == '+' || line[_index] == '-'))
                    _index++;

                while (_index < line.Length && char.IsDigit(line[_index]))
                    _index++;
            }

            string numberValue = line.Substring(start, _index - start);
            var numberToken = new Token(decimalPointCount > 0 || isExponent ? PerlToken.NUMBER : PerlToken.INT, numberValue, _line, _column, decimalPointCount > 0 || isExponent ? "Number" : "Integer");
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

        private bool RegexIdentifier(string line, TokenMathType tokenType, ref int id)
        {
            var patterns = tokenType == TokenMathType.KEYWORD ? _tokenDicnionary.KeywordPatterns : _tokenDicnionary.TokenPatterns;

            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern.Value);
                var match = regex.Match(line.Substring(_index));

                if (match.Success)
                {
                    string value = match.Value;
                    var stringToken = new Token(pattern.Key, match.Value, _line, _column)
                    {
                        Id = id++
                    };
                    tokens.Add(stringToken);
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

        private bool IsSupportedCharacter(char c)
        {
            return c >= 32 && c <= 126;
        }

        private Token LexString(string line)
        {
            int start = _index;
            char quoteType = line[_index];
            _index++;

            while (_index < line.Length && line[_index] != quoteType)
            {
                _index++;
            }

            if (_index >= line.Length || line[_index] != quoteType)
            {
                string unclosedString = line.Substring(start);
                var errorToken = new Token(PerlToken.ILLEGAL, unclosedString, _line, _column)
                {
                    Error = $"Unclosed string literal at line {_line}, column {_column}, line: {line}"
                };
                _index++;
                _column += unclosedString.Length;
                return errorToken;
            }

            _index++; // Skip the closing quote
            string value = line.Substring(start, _index - start);
            var stringToken = new Token(PerlToken.STRING, value, _line, _column);
            _column += value.Length;
            return stringToken;
        }

    }
}