using System;
using System.Collections.Generic;
using System.IO;
using Gtk;
using MTRAN.Interpreter;

namespace MTRAN.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            FileChooserDialog fileChooser = new FileChooserDialog(
                "Select a .txt or .pl file",
                null,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept
            );

            // Add file filters
            FileFilter filter = new FileFilter();
            filter.AddPattern("*.txt");
            filter.AddPattern("*.pl");
            filter.AddPattern("*.*");
            fileChooser.Filter = filter;

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                string filePath = fileChooser.Filename;

                try
                {
                    // Read the file content
                    string fileContent = File.ReadAllText(filePath);
                    Console.WriteLine("File content successfully read:\n" + fileContent);

                    // Tokenize and parse the Perl code
                    var lexer = new PerlLexerParser(fileContent);
                    lexer.Tokenize();
                    var parser = new Parser(lexer);
                    ASTNode ast = parser.Parse();

                    // Perform semantic analysis
                    var interpreterAnalyzer = new InterpreterAnalyzer();
                    interpreterAnalyzer.Analyze(ast);

                    // Console.WriteLine("Semantic analysis completed successfully.");
                    // Console.WriteLine("\nSemantic Tree:");
                    // semanticAnalyzer.PrintSymbolTable();
                    interpreterAnalyzer.ReportErrors();   // Print all semantic errors

                    // Perform interpretation
                    Console.WriteLine("\nInterpreting the program...");
                    interpreterAnalyzer.Interpret(ast);   // Interpret the AST
                    // interpreterAnalyzer.PrintVariableValues(); // Print variable values after interpretation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No file selected.");
            }

            fileChooser.Destroy();
            Application.Quit();
        }
    }
}