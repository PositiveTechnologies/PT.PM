using Fclp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM.Prebuild
{
    public enum GenerateStatus
    {
        NotGenerated,
        Generated,
        Error,
    }

    /// <summary>
    /// Generate parser files if grammar has been changed.
    /// Grammar changing date (.g4) stored to the header of generating file (.cs).
    /// If dates are not equal then files will be regenerated.
    /// </summary>
    class Program
    {
        static string AntlrDefaultPath;
        static string AntlrJarFileName;

        static void Main(string[] args)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AntlrDefaultPath = Path.Combine(currentPath,
                @"..\..\Sources\packages\Antlr4.CodeGenerator.4.6.1-beta002\tools\antlr4-csharp-4.6.1-SNAPSHOT-complete.jar");

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var cmdParser = new FluentCommandLineParser();

            string lexerFile = null;
            string parserFile = null;
            string packageName = null;
            bool listener = false;
            string output = null;
            AntlrJarFileName = Path.GetFullPath(AntlrDefaultPath);
            cmdParser.Setup<string>("lexer").Callback(lexer => lexerFile = Path.GetFullPath(lexer));
            cmdParser.Setup<string>("parser").Callback(parser => parserFile = Path.GetFullPath(parser));
            cmdParser.Setup<string>("package").Callback(package => packageName = package);
            cmdParser.Setup<string>("antlrJar").Callback(antlrJar => AntlrJarFileName = Path.GetFullPath(antlrJar));
            cmdParser.Setup<bool>("listener").Callback(l => listener = l);
            cmdParser.Setup<string>("output").Callback(o => output = o);

            var result = cmdParser.Parse(args);
            if (!result.HasErrors)
            {
                GenerateStatus generateStatus = GenerateStatus.NotGenerated;
                if (!string.IsNullOrEmpty(lexerFile))
                {
                    generateStatus = GenerateCode(lexerFile, packageName, true, listener, output);
                }
                if (generateStatus == GenerateStatus.Error)
                {
                    Console.Error.WriteLine("Code generation error");
                }

                if (!string.IsNullOrEmpty(parserFile))
                {
                    generateStatus = GenerateCode(parserFile, packageName, false, listener, output);
                }
                if (generateStatus == GenerateStatus.Error)
                {
                    Console.Error.WriteLine("Code generation error");
                }
            }
            else
            {
                var errorMessage = "Command line arguments processing error: " + result.ErrorText;
                Console.Error.WriteLine(errorMessage);
            }
        }

        private static GenerateStatus GenerateCode(string grammarFileName, string packageName, bool lexer, bool listener, string output)
        {
            DateTime grammarModifyDate = File.GetLastWriteTime(grammarFileName);
            grammarModifyDate = DateTime.Parse(grammarModifyDate.ToString());
            grammarModifyDate = new DateTime(
                grammarModifyDate.Year, grammarModifyDate.Month, grammarModifyDate.Day,
                grammarModifyDate.Hour, grammarModifyDate.Minute, grammarModifyDate.Second);
            DateTime maxModifyDate = grammarModifyDate;
            string shortGrammarFileName = Path.GetFileName(grammarFileName);
            string grammarFileDir = Path.GetDirectoryName(grammarFileName);
            string outputDirectory = output ?? Path.Combine(grammarFileDir, "Generated");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            string generatedFileName = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(grammarFileName));
            if (!lexer && !Path.GetFileNameWithoutExtension(grammarFileName).EndsWith("Parser"))
            {
                generatedFileName += "Parser";
            }
            generatedFileName += ".cs";
            bool generate = false;
            var importedGrammarFileNames = new List<string>() { shortGrammarFileName };
            if (File.Exists(generatedFileName))
            {
                string line1 = "";
                using (StreamReader reader = new StreamReader(generatedFileName))
                {
                    line1 = reader.ReadLine();
                }
                try
                {
                    // Get stored date from comment in generated file.
                    var dateString = line1.Substring(line1.LastIndexOf(' ', line1.LastIndexOf(' ') - 1) + 1);
                    DateTime modifyDateInGenerated = DateTime.Parse(dateString);

                    if (grammarModifyDate > modifyDateInGenerated)
                    {
                        generate = true;
                    }

                    string grammarData = File.ReadAllText(grammarFileName);

                    MatchCollection vocabMatches = Regex.Matches(grammarData, @"tokenVocab\s*=\s*([^;]+);");
                    MatchCollection importMatches = Regex.Matches(grammarData, @"\r\nimport\s+([^;]+);\r\n");

                    // Detect file dependencies: lexer or import files.
                    var imports = new HashSet<string>();
                    foreach (var match in vocabMatches)
                    {
                        string[] importFileNames = ((Match)match).Groups[1].Value.Split(',');
                        foreach (var importFileName in importFileNames)
                        {
                            imports.Add(importFileName);
                        }
                    }
                    foreach (var match in importMatches)
                    {
                        string[] importFileNames = ((Match)match).Groups[1].Value.Split(',');
                        foreach (var importFileName in importFileNames)
                        {
                            imports.Add(importFileName);
                        }
                    }

                    foreach (string import in imports)
                    {
                        importedGrammarFileNames.Insert(0, import + ".g4");
                        DateTime lastWriteTime = File.GetLastWriteTime(Path.Combine(grammarFileDir, import + ".g4"));
                        lastWriteTime = new DateTime(lastWriteTime.Year, lastWriteTime.Month, lastWriteTime.Day,
                            lastWriteTime.Hour, lastWriteTime.Minute, lastWriteTime.Second);
                        if (lastWriteTime != modifyDateInGenerated)
                        {
                            generate = true;
                            if (lastWriteTime > maxModifyDate)
                            {
                                maxModifyDate = lastWriteTime;
                            }
                        }
                    }
                }
                catch
                {
                    generate = true;
                }
            }
            else
            {
                // If generating file does not exist then generate new file.
                generate = true;
            }

            // Regenerated files if required.
            GenerateStatus result = GenerateStatus.NotGenerated;
            if (generate)
            {
                string lexerParser = lexer ? "Lexer" : "Parser";
                Console.WriteLine($"{lexerParser} for {shortGrammarFileName} generation...");
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "java";
                process.StartInfo.WorkingDirectory = grammarFileDir;
                string visitorListenerStr = (listener ? "-listener " : "-no-listener ") + "-visitor";
                process.StartInfo.Arguments = $@"-jar ""{AntlrJarFileName}"" -o ""{outputDirectory}"" ""{shortGrammarFileName}"" -Dlanguage=CSharp_v4_5 {visitorListenerStr} -Werror -package {packageName}";
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit(7500);

                string outputText = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(outputText))
                {
                    Console.Write(outputText);
                }
                string errorText = process.StandardError.ReadToEnd();
                if (string.IsNullOrWhiteSpace(errorText))
                {
                    string grammarsString = string.Join(", ", importedGrammarFileNames);
                    string modifyDateString = $"// {grammarsString} date: {maxModifyDate}";
                    string generateCode = File.ReadAllText(generatedFileName);
                    string resultCode = modifyDateString + Environment.NewLine + generateCode;
                    File.WriteAllText(generatedFileName, resultCode);
                    result = GenerateStatus.Generated;
                    Console.WriteLine($"{lexerParser} for {shortGrammarFileName} has been generated.");
                }
                else
                {
                    Console.Write(errorText);
                    File.Delete(generatedFileName);
                    result = GenerateStatus.Error;
                    Console.WriteLine($"{lexerParser}  for {shortGrammarFileName} generation error.");
                }
            }
            else
            {
                Console.WriteLine($"{shortGrammarFileName} has not been changed. Parser has not been generated.");
            }
            return result;
        }
    }
}
