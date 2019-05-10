using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter<TextSpan>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public string EmptyTextSpanFormat { get; set; }

        public bool IsLineColumn { get; set; }

        public TextFile CurrentSourceFile { get; set; }

        public HashSet<IFile> SourceFiles { get; set; } = new HashSet<IFile>();

        public TextFile SerializedFile { get; set; } = TextFile.Empty;

        public override void WriteJson(JsonWriter writer, TextSpan textSpan, JsonSerializer serializer)
        {
            try
            {
                string textSpanString;
                bool includeFileName = !(textSpan.File is null) && textSpan.File != CurrentSourceFile;

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
                        TextFile sourceFile = textSpan.GetSourceFile(CurrentSourceFile);
                        if (sourceFile != null)
                        {
                            textSpanString = sourceFile.GetLineColumnTextSpan(textSpan).ToString(includeFileName);
                        }
                        else
                        {
                            Logger.LogError(SerializedFile, (writer as JTokenWriter)?.CurrentToken , $"Unable convert {nameof(TextSpan)} to {nameof(LineColumnTextSpan)} due to undefined file");
                            textSpanString = LineColumnTextSpan.Zero.ToString();
                        }
                    }
                }

                writer.WriteValue(textSpanString);
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(SerializedFile, (writer as JTokenWriter)?.CurrentToken, ex);
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
                    result = TextUtils.ParseAnyTextSpan(textSpanString, out bool isLineColumn, CurrentSourceFile, SourceFiles);
                    IsLineColumn = isLineColumn;
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(SerializedFile, (reader as JTokenReader)?.CurrentToken, ex);
            }

            return result;
        }
    }
}
