using PT.PM.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PT.PM.Common.Files;

namespace PT.PM.Common
{
    public static class TextUtils
    {
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

        public static TextSpan Union(this IList<TextSpan> textSpans)
        {
            if (textSpans == null || textSpans.Count == 0)
            {
                return TextSpan.Zero;
            }

            var resultTextSpan = textSpans[0];
            if (textSpans.Count == 1)
            {
                return resultTextSpan;
            }

            for (int i = 1; i < textSpans.Count; i++)
            {
                resultTextSpan = resultTextSpan.Union(textSpans[i]);
            }

            return resultTextSpan;
        }

        public static TextSpan ParseAnyTextSpan(string textSpanString, out bool isLineColumn, TextFile currentSourceFile, HashSet<IFile> sourceFiles)
        {
            TextSpan result;

            isLineColumn = textSpanString.Contains(",");
            if (!isLineColumn)
            {
                result = ParseTextSpan(textSpanString, currentSourceFile, sourceFiles);
            }
            else
            {
                LineColumnTextSpan lineColumnTextSpan = ParseLineColumnTextSpan(textSpanString, currentSourceFile, sourceFiles);
                result = lineColumnTextSpan.File.GetTextSpan(lineColumnTextSpan);
            }

            return result;
        }

        public static TextSpan ParseTextSpan(string text, TextFile currentSourceFile = null, HashSet<IFile> sourceFiles = null)
        {
            ReadOnlySpan<char> textSpan = text.AsSpan();
            int semicolonIndex = textSpan.IndexOf(';');

            ReadOnlySpan<char> location;
            ReadOnlySpan<char> fileName;

            if (semicolonIndex >= 0)
            {
                location = textSpan.Slice(0, semicolonIndex).Trim();
                fileName = textSpan.Slice(semicolonIndex + 1).Trim();
            }
            else
            {
                location = textSpan.Trim();
                fileName = ReadOnlySpan<char>.Empty;
            }

            TextFile sourceFile = (TextFile)GetSourceFile(fileName, currentSourceFile, sourceFiles);

            TextSpan result;
            try
            {
                ReadOnlySpan<char> range = location.Slice(1, location.Length - 2);
                int index = range.IndexOf("..".AsSpan());

                int start, end;
                if (index != -1)
                {
                    string value = range.Slice(0, index).ToString(); // TODO: It will be replaced with Span when netstandard2.1 comes out
                    if (!int.TryParse(value, out start))
                    {
                        throw new FormatException($"Invalid or too big value {value} while {nameof(TextSpan)} parsing.");
                    }

                    value = range.Slice(index + 2).ToString();  // TODO: It will be replaced with Span when netstandard2.1 comes out
                    if (!int.TryParse(value, out end))
                    {
                        throw new FormatException($"Invalid or too big value {value} while {nameof(TextSpan)} parsing.");
                    }
                }
                else
                {
                    string value = range.ToString(); // TODO: It will be replaced with Span when netstandard2.1 comes out
                    if (!int.TryParse(range.ToString(), out start))
                    {
                        throw new FormatException($"Invalid or too big value {value} while {nameof(TextSpan)} parsing.");
                    }

                    end = start;
                }

                result = TextSpan.FromBounds(start, end, sourceFile);
            }
            catch (Exception ex) when (!(ex is FormatException))
            {
                throw new FormatException($"{nameof(TextSpan)} should be written in [start..end) format.");
            }

            return result;
        }

        public static LineColumnTextSpan ParseLineColumnTextSpan(string text, TextFile currentSourceFile = null, HashSet<IFile> sourceFiles = null)
        {
            ReadOnlySpan<char> textSpan = text.AsSpan();
            int semicolonIndex = textSpan.IndexOf(';');

            ReadOnlySpan<char> location;
            ReadOnlySpan<char> fileName;

            if (semicolonIndex >= 0)
            {
                location = textSpan.Slice(0, semicolonIndex).Trim();
                fileName = textSpan.Slice(semicolonIndex + 1).Trim();
            }
            else
            {
                location = textSpan.Trim();
                fileName = ReadOnlySpan<char>.Empty;
            }

            TextFile sourceFile = (TextFile)GetSourceFile(fileName, currentSourceFile, sourceFiles);

            LineColumnTextSpan result;
            ReadOnlySpan<char> firstPart = location.Slice(1, location.Length - 2);

            try
            {
                int beginLine, beginColumn, endLine, endColumn;

                var index = firstPart.IndexOf("..".AsSpan());
                if (index != -1)
                {
                    ReadOnlySpan<char> begin = firstPart.Slice(0, index);
                    ReadOnlySpan<char> end = firstPart.Slice(index + 2);

                    if (end.IndexOf(',') == -1)
                    {
                        ParseLineColumn(begin, out beginLine, out beginColumn);
                        endLine = beginLine;
                        string endStr = end.ToString();
                        if (!int.TryParse(endStr, out endColumn)) // TODO: It will be replaced with Span when netstandard2.1 comes out
                        {
                            throw new FormatException($"Invalid or too big column value {endStr} while {nameof(LineColumnTextSpan)} parsing.");
                        }
                    }
                    else if (begin.IndexOf(',') == -1)
                    {
                        ParseLineColumn(end, out endLine, out endColumn);
                        beginColumn = endColumn;
                        string beginStr = begin.ToString();
                        if (!int.TryParse(beginStr, out beginLine)) // TODO: It will be replaced with Span when netstandard2.1 comes out
                        {
                            throw new FormatException($"Invalid or too big line value {beginStr} while {nameof(LineColumnTextSpan)} parsing.");
                        }
                    }
                    else
                    {
                        ParseLineColumn(begin, out beginLine, out beginColumn);
                        ParseLineColumn(end, out endLine, out endColumn);
                    }
                }
                else
                {
                    ParseLineColumn(firstPart, out beginLine, out beginColumn);
                    endLine = beginLine;
                    endColumn = beginColumn;
                }

                result = new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn, sourceFile);
            }
            catch (Exception ex) when (!(ex is FormatException))
            {
                throw new FormatException($"{nameof(LineColumnTextSpan)} should be written in [start-line,start-column..end-line,end-column) format.");
            }

            return result;
        }

        public static IFile GetSourceFile(ReadOnlySpan<char> fileName, IFile currentSourceFile, HashSet<IFile> sourceFiles)
        {
            IFile result = null;
            if (fileName.IsEmpty)
            {
                result = currentSourceFile;
            }
            else
            {
                string fileNameString = fileName.ToString().NormalizeDirSeparator();

                if (sourceFiles != null)
                    lock (sourceFiles)
                    {
                        result = sourceFiles.FirstOrDefault(sourceFile =>
                            sourceFile.FullName == fileNameString || sourceFile.RelativeName == fileNameString);
                    }

                if (result == null)
                {
                    if (!FileExt.Exists(fileNameString))
                    {
                        if (currentSourceFile != null)
                        {
                            fileNameString = Path.Combine(currentSourceFile.RootPath, fileNameString);
                            if (!FileExt.Exists(fileNameString))
                            {
                                throw new FileNotFoundException($"File {fileNameString} not found.", fileNameString);
                            }
                        }
                    }

                    var code = FileExt.ReadAllText(fileNameString);
                    result = new TextFile(code)
                    {
                        RootPath = Path.GetDirectoryName(fileNameString),
                        Name = Path.GetFileName(fileNameString)
                    };

                    if (sourceFiles != null)
                        lock (sourceFiles)
                        {
                            sourceFiles.Add(result);
                        }
                }
            }

            return result;
        }

        public static LineColumnTextSpan GetLineColumnTextSpan(this TextSpan textSpan, TextFile currentFile)
        {
            return textSpan.GetSourceFile(currentFile).GetLineColumnTextSpan(textSpan);
        }

        public static TextFile GetSourceFile(this TextSpan textSpan, TextFile currentFile) => textSpan.File ?? currentFile;

        public static string Substring(this string str, TextSpan textSpan)
        {
            return str.Substring(textSpan.Start, textSpan.Length);
        }

        public static void PadLeft(this StringBuilder builder, int totalWidth)
        {
            builder.Append(' ', totalWidth);
        }

        public static string Escape(this string str)
        {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static string CollectWords(params object[] objects)
        {
            var result = new StringBuilder();

            for (int i = 0; i < objects.Length; i++)
            {
                string str = objects[i]?.ToString();
                if (!string.IsNullOrEmpty(str))
                {
                    result.Append(str);
                    if (i < objects.Length - 1)
                    {
                        result.Append(' ');
                    }
                }
            }

            return result.ToString();
        }

        public static string ToStringNullable(this object obj)
        {
            return obj?.ToString() ?? "";
        }

        public static string ToStringWithLeadSpace(this object obj)
        {
            return ToStringWithLead(obj, ' ');
        }

        public static string ToStringWithTrailSpace(this object obj)
        {
            return ToStringWithTrail(obj, ' ');
        }

        public static string ToStringWithLeadAndTrailSpace(this object obj)
        {
            return ToStringWithLeadAndTrail(obj, ' ');
        }

        public static string ToStringWithTrailNL(this object obj)
        {
            return ToStringWithTrail(obj, '\n');
        }

        public static string ToStringWithTrail(this object obj, char trail)
        {
            string result = obj?.ToString() ?? null;
            return string.IsNullOrEmpty(result) ? "" : result + trail;
        }

        public static string ToStringWithLead(this object obj, char lead)
        {
            string result = obj?.ToString();
            return string.IsNullOrEmpty(result) ? "" : lead + result;
        }

        public static string ToStringWithLeadAndTrail(this object obj, char leadAndTrailSymbol)
        {
            string result = obj?.ToString();
            return string.IsNullOrEmpty(result) ? "" : $"{leadAndTrailSymbol}{result}{leadAndTrailSymbol}";
        }

        public static void GetNewlineIndent(bool newline, string prevIndent, out string nl, out string indent)
        {
            if (newline)
            {
                nl = "\n";
                indent = prevIndent + "    ";
            }
            else
            {
                nl = " ";
                indent = "";
            }
        }

        private static void ParseLineColumn(ReadOnlySpan<char> text, out int line, out int column)
        {
            int commaIndex = text.IndexOf(',');
            if (commaIndex == -1)
            {
                throw new FormatException($"Begin position for line-column format should have line,column format instead of {text.ToString()}.");
            }

            string value = text.Slice(0, commaIndex).ToString(); // TODO: It will be replaced with Span when netstandard2.1 comes out
            if (!int.TryParse(value, out line))
            {
                throw new FormatException($"Invalid or too big line value {value} while {nameof(LineColumnTextSpan)} parsing.");
            }

            value = text.Slice(commaIndex + 1).ToString(); // TODO: It will be replaced with Span when netstandard2.1 comes out
            if (!int.TryParse(value, out column))
            {
                throw new FormatException($"Invalid or too big column value {value} while {nameof(LineColumnTextSpan)} parsing.");
            }
        }
    }
}
