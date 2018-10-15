using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter<TextSpan>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string EmptyTextSpanFormat { get; set; } = null;

        public bool IsLineColumn { get; set; } = false;

        public bool RelativeFileNames { get; set; }

        public CodeFile CurrentCodeFile { get; set; }

        public HashSet<CodeFile> CodeFiles { get; set; } = new HashSet<CodeFile>();

        public CodeFile JsonFile { get; set; } = CodeFile.Empty;

        public override void WriteJson(JsonWriter writer, TextSpan textSpan, JsonSerializer serializer)
        {
            try
            {
                string textSpanString;
                bool includeFileName = textSpan.CodeFile != CurrentCodeFile;

                if (!IsLineColumn)
                {
                    textSpanString = textSpan.IsZero && EmptyTextSpanFormat != null
                        ? EmptyTextSpanFormat
                        : textSpan.ToString(includeFileName, RelativeFileNames);
                }
                else
                {
                    if (textSpan.IsZero && EmptyTextSpanFormat != null)
                    {
                        textSpanString = EmptyTextSpanFormat;
                    }
                    else
                    {
                        CodeFile codeFile = textSpan.GetCodeFile(CurrentCodeFile);
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
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(JsonFile, (writer as JTokenWriter)?.CurrentToken, ex);
            }
        }

        public override TextSpan ReadJson(JsonReader reader, Type objectType, TextSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            TextSpan result = TextSpan.Zero;

            try
            {
                string textSpanString = (string)reader.Value;

                if (textSpanString != EmptyTextSpanFormat)
                {
                    result = TextUtils.ParseAnyTextSpan(textSpanString, out bool isLineColumn, CurrentCodeFile, CodeFiles);
                    IsLineColumn = isLineColumn;
                }

                if (result.CodeFile == CurrentCodeFile)
                {
                    result.CodeFile = null;
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(JsonFile, (reader as JTokenReader)?.CurrentToken, ex);
            }

            return result;
        }
    }
}
