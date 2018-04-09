using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string EmptyTextSpanFormat { get; set; } = null;

        public bool IsLineColumn { get; set; } = false;

        public CodeFile CurrentCodeFile { get; set; }

        public HashSet<CodeFile> CodeFiles { get; set; } = new HashSet<CodeFile>();

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
                            Logger.LogError(JsonFile, (writer as JTokenWriter)?.CurrentToken , $"Unable convert {nameof(TextSpan)} to {nameof(LineColumnTextSpan)} due to undefined file");
                            textSpanString = LineColumnTextSpan.Zero.ToString();
                        }
                    }
                }

                writer.WriteValue(textSpanString);
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, (writer as JTokenWriter)?.CurrentToken, ex);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TextSpan result = TextSpan.Zero;

            try
            {
                try
                {
                    result = DeserializeTextSpan(reader, IsLineColumn);
                }
                catch (FormatException)
                {
                    result = DeserializeTextSpan(reader, !IsLineColumn);
                    IsLineColumn = !IsLineColumn;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, (reader as JTokenReader)?.CurrentToken, ex);
            }

            return result;
        }

        private TextSpan DeserializeTextSpan(JsonReader reader, bool isLineColumn)
        {
            TextSpan result = TextSpan.Zero;

            string textSpanString = (string)reader.Value;
            if (textSpanString != EmptyTextSpanFormat)
            {
                if (!isLineColumn)
                {
                    try
                    {
                        result = TextUtils.ParseTextSpan(textSpanString, CurrentCodeFile, CodeFiles);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Logger.LogError(JsonFile, (reader as JTokenReader)?.CurrentToken, ex);
                    }
                }
                else
                {
                    try
                    {
                        LineColumnTextSpan lineColumnTextSpan = TextUtils.ParseLineColumnTextSpan(textSpanString, CurrentCodeFile, CodeFiles);
                        result = lineColumnTextSpan.CodeFile.GetTextSpan(lineColumnTextSpan);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Logger.LogError(JsonFile, (reader as JTokenReader)?.CurrentToken, ex);
                    }
                }

                if (result.CodeFile == CurrentCodeFile)
                {
                    result.CodeFile = null;
                }
            }

            return result;
        }
    }
}
