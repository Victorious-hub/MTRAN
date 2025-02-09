using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class PerlTokenizer
{
    private static readonly Dictionary<string, string> Keywords = new()
    {
        { "if", "KEYWORD" }, { "else", "KEYWORD" }, { "while", "KEYWORD" },
        { "for", "KEYWORD" }, { "sub", "KEYWORD" }, { "my", "KEYWORD" },
        { "print", "KEYWORD" }, { "return", "KEYWORD" }
    };

    private static readonly List<(string Pattern, string Type)> TokenPatterns = new()
    {
        ("#[^\n]*", "COMMENT"),   // Комментарии
        ("\"(\\.|[^\\"])*\"", "STRING"),  // Строки в " "
        ("'([^\\']|\\.)*'", "STRING"),    // Строки в ' '
        ("\\d+", "NUMBER"),   // Числа
        ("[a-zA-Z_][a-zA-Z0-9_]*", "IDENTIFIER"), // Идентификаторы и ключевые слова
        ("[{}()\[\];,]", "DELIMITER"),  // Разделители
        ("[=+\-*/%<>!&|]+", "OPERATOR")  // Операторы
    };

    public static List<(string Type, string Value)> Tokenize(string code)
    {
        var tokens = new List<(string, string)>();
        int position = 0;

        while (position < code.Length)
        {
            bool matchFound = false;
            foreach (var (pattern, type) in TokenPatterns)
            {
                var regex = new Regex("^" + pattern);
                var match = regex.Match(code[position..]);

                if (match.Success)
                {
                    string value = match.Value;
                    string tokenType = Keywords.ContainsKey(value) ? Keywords[value] : type;
                    tokens.Add((tokenType, value));
                    position += value.Length;
                    matchFound = true;
                    break;
                }
            }
            
            if (!matchFound)
            {
                position++; // Пропуск некорректного символа
            }
        }
        
        return tokens;
    }

    static void Main()
    {
        string code = "my $x = 42; # This is a comment\nprint \"Hello, Perl!\";";
        var tokens = Tokenize(code);

        foreach (var (type, value) in tokens)
        {
            Console.WriteLine($"{type}: {value}");
        }
    }
}