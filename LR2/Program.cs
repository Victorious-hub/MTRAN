using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MTRAN.LR2;

class PerlTokenizer
{
    [STAThread]
    static void Main()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|Perl files (*.pl)|*.pl|All files (*.*)|*.*",
            Title = "Select a .txt or .pl file"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;

            try
            {
                string fileContent = System.IO.File.ReadAllText(filePath);
                Console.WriteLine("File content successfully read:\n" + fileContent);

                var lexer = new PerlLexer(fileContent);
                var tokens = lexer.Tokenize();

                foreach (var token in tokens)
                {
                    Console.WriteLine($"Token: {token.TokenType} - Lexeme: {token.Lexeme} - Line: {token.Line} - Column: {token.Column}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("No file selected.");
        }
    }
}