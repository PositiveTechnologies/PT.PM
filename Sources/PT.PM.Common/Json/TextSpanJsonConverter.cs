using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

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

                string textSpanString;
                bool includeFileName = textSpan.CodeFile != CurrentCodeFile;

                if (!IsLineColumn)
                {
                    textSpanString = textSpan.IsZero && EmptyTextSpanFormat != null
                        ? EmptyTextSpanFormat
                        : textSpan.ToString(includeFileName);
                }
                else
                {
                    if (textSpan.IsZero && EmptyTextSpanFormat != null)
                    {
                        textSpanString = EmptyTextSpanFormat;
                    }
                    else
                    {
                        CodeFile codeFile = textSpan.CodeFile ?? CurrentCodeFile;
                        if (codeFile != null)
                        {
                            textSpanString = codeFile.GetLineColumnTextSpan(textSpan).ToString(includeFileName);
                        }
                        else
                        {
                            Logger.LogError(JsonFile, null, $"Unable convert {nameof(TextSpan)} to {nameof(LineColumnTextSpan)} due to undefined file");
                            textSpanString = LineColumnTextSpan.Zero.ToString();
                        }
                    }
                }

                writer.WriteValue(textSpanString);
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
                if (TryDeserializeTextSpan(reader, IsLineColumn, out result))
                {
                    return result;
                }

                DeserializeTextSpan(reader, !IsLineColumn, out result);
                IsLineColumn = !IsLineColumn;

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, jsonLineInfo, ex);
            }
            return result;
        }

        private bool TryDeserializeTextSpan(JsonReader reader, bool isLineColumn, out TextSpan result, bool throwException = false)
        {
            try
            {
                DeserializeTextSpan(reader, isLineColumn, out result);
                return true;
            }
            catch
            {
                result = TextSpan.Zero;
                return false;
            }
        }

        private void DeserializeTextSpan(JsonReader reader, bool isLineColumn, out TextSpan result)
        {
            result = TextSpan.Zero;

            string textSpanString = (string)reader.Value;
            if (textSpanString != EmptyTextSpanFormat)
            {
                if (!isLineColumn)
                {
                    result = TextUtils.ParseTextSpan(textSpanString, CurrentCodeFile, CodeFiles);
                }
                else
                {
                    LineColumnTextSpan lineColumnTextSpan = TextUtils.ParseLineColumnTextSpan(textSpanString, CurrentCodeFile, CodeFiles);
                    var codeFile = lineColumnTextSpan.CodeFile ?? CurrentCodeFile;
                    if (codeFile != null)
                    {
                        result = codeFile.GetTextSpan(lineColumnTextSpan);
                    }
                    else
                    {
                        Logger.LogError(JsonFile, null, $"Unable convert {nameof(LineColumnTextSpan)} to {nameof(TextSpan)} due to undefined file");
                    }
                }

                if (result.CodeFile == CurrentCodeFile)
                {
                    result.CodeFile = null;
                }
            }
        }
    }
}
