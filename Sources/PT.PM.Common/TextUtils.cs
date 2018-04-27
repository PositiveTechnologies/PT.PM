﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PT.PM.Common
{
    public static class TextUtils
    {
        private static char[] semicolon = new char[] { ';' };

        private const int StartLine = 1;
        private const int StartColumn = 1;

        public static readonly Regex HttpRegex = new Regex("^https?://", RegexOptions.Compiled);

        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
        }

        public static int LastIndexOf(this string str, int index, bool whitespace)
        {
            int i = index;
            while (i >= 0 && (whitespace ? char.IsWhiteSpace(str[i]) : !char.IsWhiteSpace(str[i])))
                i--;
            return i;
        }

        public static int FirstIndexOf(this string str, int index, bool whitespace)
        {
            int i = index;
            while (i < str.Length && (whitespace ? char.IsWhiteSpace(str[i]) : !char.IsWhiteSpace(str[i])))
                i++;
            return i;
        }

        public static string NormDirSeparator(this string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        public static TextSpan Union(this IEnumerable<TextSpan> textSpans)
        {
            if (textSpans == null || textSpans.Count() == 0)
            {
                return TextSpan.Zero;
            }

            var resultTextSpan = textSpans.First();
            if (textSpans.Count() == 1)
            {
                return resultTextSpan;
            }

            foreach (TextSpan textSpan in textSpans.Skip(1))
            {
                resultTextSpan = resultTextSpan.Union(textSpan);
            }

            return resultTextSpan;
        }

        public static TextSpan ParseTextSpan(string text, CodeFile currentCodeFile = null, HashSet<CodeFile> codeFiles = null)
        {
            string[] parts = text.Split(semicolon, 2);

            string fileName = parts.Length == 2
                ? parts[1].Trim()
                : null;

            CodeFile codeFile = GetCodeFile(fileName, currentCodeFile, codeFiles);

            TextSpan result;
            try
            {
                string range = parts[0].Trim().Substring(1, parts[0].Length - 2);
                int index = range.IndexOf("..");

                int start, end;
                if (index != -1)
                {
                    string value = range.Remove(index);
                    if (!int.TryParse(value, out start))
                    {
                        throw new FormatException($"Invalid or too big value {value} while {nameof(TextSpan)} parsing.");
                    }

                    value = range.Substring(index + 2);
                    if (!int.TryParse(value, out end))
                    {
                        throw new FormatException($"Invalid or too big value {value} while {nameof(TextSpan)} parsing.");
                    }
                }
                else
                {
                    if (!int.TryParse(range, out start))
                    {
                        throw new FormatException($"Invalid or too big value {range} while {nameof(TextSpan)} parsing.");
                    }

                    end = start;
                }

                result = TextSpan.FromBounds(start, end, codeFile);
            }
            catch (Exception ex) when (!(ex is FormatException))
            {
                throw new FormatException($"{nameof(TextSpan)} should be written in [start..end) format.");
            }

            return result;
        }

        public static LineColumnTextSpan ParseLineColumnTextSpan(string text, CodeFile currentCodeFile = null, HashSet<CodeFile> codeFiles = null)
        {
            string[] parts = text.Split(semicolon, 2);

            string fileName = parts.Length == 2
                ? parts[1].Trim()
                : null;

            CodeFile codeFile = GetCodeFile(fileName, currentCodeFile, codeFiles);

            LineColumnTextSpan result;
            string firstPart = parts[0].Trim().Substring(1, parts[0].Length - 2);

            try
            {
                int beginLine, beginColumn, endLine, endColumn;

                var index = firstPart.IndexOf("..");
                if (index != -1)
                {
                    ParseLineColumn(firstPart.Remove(index), out beginLine, out beginColumn);
                    ParseLineColumn(firstPart.Substring(index + 2), out endLine, out endColumn);
                }
                else
                {
                    ParseLineColumn(firstPart, out beginLine, out beginColumn);
                    endLine = beginLine;
                    endColumn = beginColumn;
                }

                result = new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn, codeFile);
            }
            catch (Exception ex) when (!(ex is FormatException))
            {
                throw new FormatException($"{nameof(LineColumnTextSpan)} should be written in [start-line,start-column..end-line,end-column) format.");
            }

            return result;
        }

        public static CodeFile GetCodeFile(string fileName, CodeFile currentCodeFile, HashSet<CodeFile> codeFiles)
        {
            CodeFile result = null;
            if (fileName == null)
            {
                result = currentCodeFile;
            }
            else
            {
                result = codeFiles?.FirstOrDefault(codeFile => codeFile.Equals(fileName) || codeFile.RelativeName.Equals(fileName));
                if (result == null)
                {
                    if (!File.Exists(fileName))
                    {
                        if (currentCodeFile != null)
                        {
                            fileName = Path.Combine(currentCodeFile.RootPath, fileName);
                            if (!File.Exists(fileName))
                            {
                                throw new FileNotFoundException($"File {fileName} is not found.", fileName);
                            }
                        }
                    }

                    var code = File.ReadAllText(fileName);
                    result = new CodeFile(code)
                    {
                        RootPath = Path.GetDirectoryName(fileName),
                        Name = Path.GetFileName(fileName)
                    };
                    codeFiles.Add(result);
                }
            }
            return result;
        }

        public static string Substring(this string str, TextSpan textSpan)
        {
            return str.Substring(textSpan.Start, textSpan.Length);
        }

        public static void PadLeft(this StringBuilder builder, int totalWidth)
        {
            builder.Append(' ', totalWidth);
        }

        public static string RemoveWhitespaces(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            var result = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if (!char.IsWhiteSpace(c))
                    result.Append(c);
            }
            return result.ToString();
        }

        private static void ParseLineColumn(string text, out int line, out int column)
        {
            int commaIndex = text.IndexOf(',');
            if (commaIndex == -1)
            {
                throw new FormatException($"Begin position for line-column format should have line,column format instead of {text}.");
            }

            string value = text.Remove(commaIndex);
            if (!int.TryParse(value, out line))
            {
                throw new FormatException($"Invalid or too big line value {value} while {nameof(LineColumnTextSpan)} parsing.");
            }

            value = text.Substring(commaIndex + 1);
            if (!int.TryParse(value, out column))
            {
                throw new FormatException($"Invalid or too big column value {value} while {nameof(LineColumnTextSpan)} parsing.");
            }
        }
    }
}