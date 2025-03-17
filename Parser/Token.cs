using System.Collections.Generic;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MTRAN.Parser
{
    public enum PerlToken
    {
        /* Special tokens */
        ILLEGAL,
        EOF_,
        COMMENT,

        /* Literals*/
        INT,                // 123
        HEX,
        OCT,
        BIN,
        NUMBER,              // 123.45, 1e+300
        IMAG,               // 123.45i
        STRING,             // 'abc'

        /* Operators */
        OR,                 // or
        AND,                // and
        NOT,                // not
        XOR,                // xor
        LOR,                // ||
        LAND,               // &&
        LNOT,               // !
        IDENT,                // $a,... @a, %a

        BITWISE_XOR,        // ^
        BITWISE_AND,        // &
        BITWISE_OR,         // |
        BITWISE_NOT,        // ~
        BITWISE_LEFT,       // <<
        BITWISE_RIGHT,      // >>
        BIT_CLEAR,          // &^

        ADD,                // +
        SUB,                // -
        MUL,                // *
        DIV,                // /
        MOD,                // %
        INC,                // ++(postfix and prefix)
        DEC,                // --(postfix and prefix)

        ADD_ASSIGN,         // +=
        SUB_ASSIGN,         // -=
        MUL_ASSIGN,         // *=
        DIV_ASSIGN,         // /=
        MO_ASSIGND,         // %=
        AND_ASSIGN,         // &=
        OR_ASSIGN,          // |=
        XOR_ASSIGN,         // ^=
        BITWISE_LEFT_ASSIGN, // <<=
        BITWISE_RIGHT_ASSIGN, // >>=
        AND_NOT_ASSIGN,     // &^=

        EQUAL,              // ==
        ASSIGN,             // =
        NOT_EQUAL,          // !=
        LESS,               // <
        LESS_OR_EQUAL,      // <=
        GRT,                // >
        GRT_OR_EQUAL,       // >=
        DOT,                // .

        /* Delimiters */
        LPAREN,             // (
        RPAREN,             // ) 
        LBRACE,             // {
        RBRACE,             // }
        LBRACKET,           // [
        RBRACKET,           // ]
        COMMA,              // ,
        SEMICOLON,          // ;
        COLON,              // :
        ELLIPSIS,           // ...

        /* Keywords */
        IF,                 // if
        ELSE,               // else
        ELSIF,              // elsif
        RETURN,             // return
        PRINT,              // print
        MY,                 // my
        FOR,                // for
        UNTIL,              // until
        WHILE,              // while
        FOREACH,            // foreach
        GOTO,               // goto
        PACKAGE,            // package
        FUNC_SUB,           // sub(func declaration)
        USE,                // use

        HASH_ASSIGN               //: '=>' ;

    }
    
    public class TokenDictionary
    {
        public readonly Dictionary<PerlToken, string> KeywordPatterns = new()
        {
            { PerlToken.MY, @"\bmy\b" },                        // my (e.g., my)
            { PerlToken.IF, @"\bif\b" },                        // if (e.g., if)
            { PerlToken.ELSE, @"\belse\b" },                    // else (e.g., else)
            { PerlToken.ELSIF, @"\belsif\b" },                  // elsif (e.g., elsif)
            { PerlToken.RETURN, @"\breturn\b" },                // return (e.g., return)
            { PerlToken.PRINT, @"\bprint\b" },                  // print (e.g., print)
            { PerlToken.FOR, @"\bfor\b" },                      // for (e.g., for)
            { PerlToken.UNTIL, @"\buntil\b" },                  // until (e.g., until)
            { PerlToken.WHILE, @"\bwhile\b" },                  // while (e.g., while)
            { PerlToken.FOREACH, @"\bforeach\b" },              // foreach (e.g., foreach)
            { PerlToken.GOTO, @"\bgoto\b" },                    // goto (e.g., goto)
            { PerlToken.PACKAGE, @"\bpackage\b" },              // package (e.g., package)
            { PerlToken.SUB, @"\bsub\b" },                 // sub (e.g., sub)
            { PerlToken.USE, @"\buse\b" },                      // use (e.g., use)
        };

        public readonly Dictionary<PerlToken, string> TokenPatterns = new()
        {
            
            // { PerlToken.COMMENT, @"#.*" },                      // comment (single line)
            
            /* Special tokens */
            { PerlToken.IDENT, @"[\$@%][a-zA-Z_]\w*" },                // variable (e.g., $a, $var_name)

            { PerlToken.EQUAL, @"==" },                         // equality (e.g., ==)
            { PerlToken.ASSIGN, @"=" },                         // assignment (e.g., =)
            { PerlToken.INT, @"\b[0-9]+\b" },                      // integer (e.g., 123)
            { PerlToken.STRING, @"(['""])(?:(?=(\\?))\2.)*?\1" },            // string (e.g., 'abc')
            { PerlToken.NUMBER, @"\b\d+(\.\d+)?([eE][-+]?\d+)?\b" }, // float (e.g., 123.45, 1e+300)
            { PerlToken.HEX, @"\b0[xX][0-9a-fA-F]+\b" }, // hexadecimal (e.g., 0x1A3F)
            { PerlToken.OCT, @"\b0[oO]?[0-7]+\b" },      // octal (e.g., 0755 or 0o755)
            { PerlToken.BIN, @"\b0[bB][01]+\b" },
            { PerlToken.IMAG, @"\b\d+(\.\d+)?i\b" },             // imaginary number (e.g., 123.45i)

            { PerlToken.ADD, @"\+" },                           // addition (e.g., +)
            { PerlToken.SUB, @"-" },                            // subtraction (e.g., -)
            { PerlToken.MUL, @"\*" },                           // multiplication (e.g., *)
            { PerlToken.DIV, @"/" },                            // division (e.g., /)
            { PerlToken.MOD, @"%" },                            // modulus (e.g., %)
            { PerlToken.INC, @"\+\+" },                         // increment (e.g., ++)
            { PerlToken.DEC, @"--" },                           // decrement (e.g., --)
            { PerlToken.ILLEGAL, @"[^\x20-\x7E]" },           // any non-printable characters

            /* Literals */
            { PerlToken.COMMA, @"," },                          // comma (e.g., ,)
            { PerlToken.LPAREN, @"\(" },                        // left parenthesis (e.g., ()
            { PerlToken.RPAREN, @"\)" },                        // right parenthesis (e.g., )
            { PerlToken.LBRACE, @"\{" },                        // left brace (e.g., {)
            { PerlToken.RBRACE, @"\}" },                        // right brace (e.g., })
            { PerlToken.LBRACKET, @"\[" },                      // left bracket (e.g., [)
            { PerlToken.RBRACKET, @"\]" },                      // right bracket (e.g., ])
            { PerlToken.SEMICOLON, @";" },                      // semicolon (e.g., ;)
            { PerlToken.COLON, @":" },                          // colon (e.g., :)
            { PerlToken.ELLIPSIS, @"\.\.\." },                   // ellipsis (e.g., ...)
            { PerlToken.HASH_ASSIGN, @"=>" },                  // less or equal (e.g., <=)

            { PerlToken.NOT_EQUAL, @"!=" },                     // not equal (e.g., !=)
            { PerlToken.LESS_OR_EQUAL, @"<=" },                  // less or equal (e.g., <=)
            { PerlToken.LESS, @"<=" },                           // less than (e.g., <)
            { PerlToken.GRT, @">" },                            // greater than (e.g., >)
            { PerlToken.GRT_OR_EQUAL, @">=" },                  // greater or equal (e.g., >=)
            { PerlToken.DOT, @"\." },                           // dot (e.g., .)
           
            /* Operators */
            { PerlToken.OR, @"\bor\b" },                        // logical OR (e.g., or)
            { PerlToken.AND, @"\band\b" },                      // logical AND (e.g., and)
            { PerlToken.NOT, @"\bnot\b" },                      // logical NOT (e.g., not)
            { PerlToken.XOR, @"\bxor\b" },                      // XOR (e.g., xor)
            { PerlToken.LOR, @"\|\|" },                          // logical OR (e.g., ||)
            { PerlToken.LAND, @"&&" },                           // logical AND (e.g., &&)
            { PerlToken.LNOT, @"!" },                            // logical NOT (e.g., !)

            { PerlToken.BITWISE_XOR, @"\^" },                   // bitwise XOR (e.g., ^)
            { PerlToken.BITWISE_AND, @"\&" },                   // bitwise AND (e.g., &)
            { PerlToken.BITWISE_OR, @"\|" },                    // bitwise OR (e.g., |)
            { PerlToken.BITWISE_NOT, @"~" },                    // bitwise NOT (e.g., ~)
            // { PerlToken.BITWISE_LEFT, @"<<", },                 // left shift (e.g., <<)
            // { PerlToken.BITWISE_RIGHT, @">>", },                // right shift (e.g., >>)
            { PerlToken.BIT_CLEAR, @"&\^" },                    // bitwise clear (e.g., &^)

            { PerlToken.ADD_ASSIGN, @"\+=" },                   // addition assignment (e.g., +=)
            { PerlToken.SUB_ASSIGN, @"-=" },                    // subtraction assignment (e.g., -=)
            { PerlToken.MUL_ASSIGN, @"\*=" },                   // multiplication assignment (e.g., *=)
            { PerlToken.DIV_ASSIGN, @"/=" },                    // division assignment (e.g., /=)
            { PerlToken.MO_ASSIGND, @"%=" },                    // modulus assignment (e.g., %=)
            { PerlToken.AND_ASSIGN, @"\&=" },                   // bitwise AND assignment (e.g., &=)
            { PerlToken.OR_ASSIGN, @"\|=" },                    // bitwise OR assignment (e.g., |=)
            { PerlToken.XOR_ASSIGN, @"\^=" },                   // bitwise XOR assignment (e.g., ^=)
            { PerlToken.BITWISE_LEFT_ASSIGN, @"<<=" },          // left shift assignment (e.g., <<=)
            { PerlToken.BITWISE_RIGHT_ASSIGN, @">>=" },         // right shift assignment (e.g., >>=)
            { PerlToken.AND_NOT_ASSIGN, @"\&\^=" },             // bit clear assignment (e.g., &^=)
        

            { PerlToken.EOF_, @"\z" },                          // end of string
        };

    }


    public class Token {
        public PerlToken TokenType { get; set; }
        public string Lexeme { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public int Id { get; set; } = 0;
        public string Error { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Add description property

        public Token() { }

        public Token(PerlToken tokenType, string lex, int line, int column, string description = "")
        {
            this.TokenType = tokenType;
            this.Lexeme = lex;
            this.Line = line;
            this.Column = column;
            this.Description = description; // Initialize description
        }
    }

}


