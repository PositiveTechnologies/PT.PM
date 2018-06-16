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

        public bool IsLinear { get; set; } = false;

        public CodeFile CurrentCodeFile { get; set; }

        public HashSet<CodeFile> CodeFiles { get; set; } = new HashSet<CodeFile>();

        public CodeFile JsonFile { get; set; } = CodeFile.Empty;

        public void AddCodeFile(CodeFile codeFile)
        {
            CurrentCodeFile = codeFile;
            lock (CodeFiles)
                CodeFiles.Add(codeFile);
        }

        public override void WriteJson(JsonWriter writer, TextSpan textSpan, JsonSerializer serializer)
        {
            try
            {
                string textSpanString;
                bool includeFileName = textSpan.CodeFile != CurrentCodeFile;

                if (IsLinear)
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
                try
                {
                    result = DeserializeTextSpan(reader, IsLinear);
                }
                catch (FormatException)
                {
                    result = DeserializeTextSpan(reader, !IsLinear);
                    IsLinear = !IsLinear;
                }
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(JsonFile, (reader as JTokenReader)?.CurrentToken, ex);
            }

            return result;
        }

        private TextSpan DeserializeTextSpan(JsonReader reader, bool isLinear)
        {
            TextSpan result = TextSpan.Zero;

            string textSpanString = (string)reader.Value;
            if (textSpanString != EmptyTextSpanFormat)
            {
                if (isLinear)
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
