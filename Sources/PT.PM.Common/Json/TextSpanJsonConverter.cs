using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool ShortFormat { get; set; } = true;

        public string EmptyTextSpanFormat { get; set; } = null;

        public bool IsLineColumn { get; set; } = false;

        public CodeFile CurrentCodeFile { get; set; }

        public List<CodeFile> CodeFiles { get; set; } = new List<CodeFile>();

        public CodeFile JsonFile { get; set; } = CodeFile.Empty;

        public void AddCodeFile(CodeFile codeFile)
        {
            CurrentCodeFile = codeFile;
            CodeFiles.Add(codeFile);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TextSpan);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                var textSpan = (TextSpan)value;
                if (ShortFormat)
                {
                    string textSpanString;
                    if (!IsLineColumn)
                    {
                        textSpanString = textSpan.IsZero && EmptyTextSpanFormat != null
                            ? EmptyTextSpanFormat
                            : textSpan.ToString();
                    }
                    else
                    {
                        textSpanString = textSpan.IsZero && EmptyTextSpanFormat != null
                            ? EmptyTextSpanFormat
                            : (GetCodeFile(textSpan.FileName, null)?.GetLineColumnTextSpan(textSpan).ToString() ?? EmptyTextSpanFormat);
                    }
                    writer.WriteValue(textSpanString);
                }
                else
                {
                    JObject jObject = new JObject();
                    if (!IsLineColumn)
                    {
                        jObject.Add(nameof(textSpan.Start), textSpan.Start);
                        jObject.Add(nameof(textSpan.Length), textSpan.Length);
                    }
                    else
                    {
                        LineColumnTextSpan lineColumnTextSpan = GetCodeFile(textSpan.FileName, null)?.GetLineColumnTextSpan(textSpan) ?? LineColumnTextSpan.Zero;
                        jObject.Add(nameof(LineColumnTextSpan.BeginLine), lineColumnTextSpan.BeginLine);
                        jObject.Add(nameof(LineColumnTextSpan.BeginColumn), lineColumnTextSpan.BeginColumn);
                        jObject.Add(nameof(LineColumnTextSpan.EndLine), lineColumnTextSpan.EndLine);
                        jObject.Add(nameof(LineColumnTextSpan.EndColumn), lineColumnTextSpan.EndColumn);
                    }
                    jObject.WriteTo(writer);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, null, ex);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TextSpan result = TextSpan.Zero;
            IJsonLineInfo jsonLineInfo = null;
            try
            {
                if (TryDeserializeTextSpan(reader, ShortFormat, IsLineColumn, out result))
                {
                    return result;
                }

                if (TryDeserializeTextSpan(reader, ShortFormat, !IsLineColumn, out result))
                {
                    IsLineColumn = !IsLineColumn;
                    return result;
                }

                if (TryDeserializeTextSpan(reader, !ShortFormat, IsLineColumn, out result))
                {
                    ShortFormat = !ShortFormat;
                    return result;
                }

                DeserializeTextSpan(reader, !ShortFormat, !IsLineColumn, out result);
                ShortFormat = !ShortFormat;
                IsLineColumn = !IsLineColumn;

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, jsonLineInfo, ex);
            }
            return result;
        }

        private bool TryDeserializeTextSpan(JsonReader reader, bool shortFormat, bool isLineColumn, out TextSpan result, bool throwException = false)
        {
            try
            {
                DeserializeTextSpan(reader, shortFormat, isLineColumn, out result);
                return true;
            }
            catch
            {
                result = TextSpan.Zero;
                return false;
            }
        }

        private void DeserializeTextSpan(JsonReader reader, bool shortFormat, bool isLineColumn, out TextSpan result)
        {
            result = TextSpan.Zero;
            if (shortFormat)
            {
                string textSpanString = (string)reader.Value;
                if (!isLineColumn)
                {
                    result = textSpanString == EmptyTextSpanFormat
                        ? TextSpan.Zero
                        : TextSpan.Parse(textSpanString);
                }
                else
                {
                    if (textSpanString == EmptyTextSpanFormat)
                    {
                        result = TextSpan.Zero;
                    }
                    else
                    {
                        LineColumnTextSpan textSpan = LineColumnTextSpan.Parse(textSpanString);
                        result = GetCodeFile(textSpan.FileName, null)?.GetTextSpan(textSpan) ?? TextSpan.Zero;
                    }
                }
            }
            else
            {
                JObject jObject = JObject.Load(reader);
                if (!isLineColumn)
                {
                    int start = jObject.GetValueIgnoreCase(nameof(TextSpan.Start))?.ToObject<int>() ?? 0;
                    int length = jObject.GetValueIgnoreCase(nameof(TextSpan.Length))?.ToObject<int>() ?? 0;
                    string fileName = jObject.GetValueIgnoreCase(nameof(TextSpan.FileName))?.ToObject<string>();
                    result = new TextSpan(start, length, fileName);
                }
                else
                {
                    int beginLine = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.BeginLine))?.ToObject<int>() ?? CodeFile.StartLine;
                    int beginColumn = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.BeginColumn))?.ToObject<int>() ?? CodeFile.StartColumn;
                    int endLine = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.EndLine))?.ToObject<int>() ?? CodeFile.StartLine;
                    int endColumn = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.EndColumn))?.ToObject<int>() ?? CodeFile.StartColumn;
                    string fileName = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.FileName))?.ToObject<string>();
                    var lcTextSpan = new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn, fileName);
                    result = GetCodeFile(lcTextSpan.FileName, jObject)?.GetTextSpan(lcTextSpan) ?? TextSpan.Zero;
                }
            }
        }

        private CodeFile GetCodeFile(string fileName, IJsonLineInfo jsonLineInfo)
        {
            CodeFile result = null;

            if (fileName == null)
            {
                result = CurrentCodeFile;
            }
            else
            {
                result = CodeFiles.FirstOrDefault(codeFile => codeFile.RelativeName == fileName || codeFile.FullName == fileName);
            }

            if (result == null)
            {
                Logger.LogError(JsonFile, jsonLineInfo, $"File {fileName} is not loaded. Try using another load order or use existing file name");
            }

            return result;
        }
    }
}
