using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool ShortFormat { get; set; } = true;

        public string EmptyTextSpanFormat { get; set; } = null;

        public bool IsLineColumn { get; set; } = false;

        public CodeFile CodeFile { get; set; } = CodeFile.Empty;

        public CodeFile JsonFile { get; set; } = CodeFile.Empty;

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
                        textSpanString = textSpan.IsEmpty && EmptyTextSpanFormat != null
                            ? EmptyTextSpanFormat
                            : textSpan.ToString();
                    }
                    else
                    {
                        textSpanString = textSpan.IsEmpty && EmptyTextSpanFormat != null
                            ? EmptyTextSpanFormat
                            : CodeFile.GetLineColumnTextSpan(textSpan).ToString();
                    }
                    writer.WriteValue(textSpanString);
                }
                else
                {
                    JObject pdgObject = new JObject();
                    if (!IsLineColumn)
                    {
                        pdgObject.Add(nameof(textSpan.Start), textSpan.Start);
                        pdgObject.Add(nameof(textSpan.Length), textSpan.Length);
                    }
                    else
                    {
                        var lineColumnTextSpan = CodeFile.GetLineColumnTextSpan(textSpan);
                        pdgObject.Add(nameof(LineColumnTextSpan.BeginLine), lineColumnTextSpan.BeginLine);
                        pdgObject.Add(nameof(LineColumnTextSpan.BeginColumn), lineColumnTextSpan.BeginColumn);
                        pdgObject.Add(nameof(LineColumnTextSpan.EndLine), lineColumnTextSpan.EndLine);
                        pdgObject.Add(nameof(LineColumnTextSpan.EndColumn), lineColumnTextSpan.EndColumn);
                    }
                    pdgObject.WriteTo(writer);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(JsonFile, null, ex);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TextSpan result = TextSpan.Empty;
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
                result = TextSpan.Empty;
                return false;
            }
        }

        private void DeserializeTextSpan(JsonReader reader, bool shortFormat, bool isLineColumn, out TextSpan result)
        {
            result = TextSpan.Empty;
            if (shortFormat)
            {
                string textSpanString = (string)reader.Value;
                if (!isLineColumn)
                {
                    result = textSpanString == EmptyTextSpanFormat
                        ? TextSpan.Empty
                        : TextSpan.Parse(textSpanString);
                }
                else
                {
                    result = textSpanString == EmptyTextSpanFormat
                         ? TextSpan.Empty
                         : CodeFile.GetTextSpan(LineColumnTextSpan.Parse(textSpanString));
                }
            }
            else
            {
                var jObject = JObject.Load(reader);
                if (!isLineColumn)
                {
                    int start = jObject.GetValueIgnoreCase(nameof(TextSpan.Start))?.ToObject<int>() ?? 0;
                    int length = jObject.GetValueIgnoreCase(nameof(TextSpan.Length))?.ToObject<int>() ?? 0;
                    result = new TextSpan(start, length);
                }
                else
                {
                    int beginLine = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.BeginLine))?.ToObject<int>() ?? CodeFile.StartLine;
                    int beginColumn = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.BeginColumn))?.ToObject<int>() ?? CodeFile.StartColumn;
                    int endLine = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.EndLine))?.ToObject<int>() ?? CodeFile.StartLine;
                    int endColumn = jObject.GetValueIgnoreCase(nameof(LineColumnTextSpan.EndColumn))?.ToObject<int>() ?? CodeFile.StartColumn;
                    var lcTextSpan = new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn);
                    result = CodeFile.GetTextSpan(lcTextSpan);
                }
            }
        }
    }
}
