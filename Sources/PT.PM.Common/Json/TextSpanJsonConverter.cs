using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter
    {
        public bool ShortFormat { get; set; } = true;

        public string EmptyTextSpanFormat { get; set; } = null;

        public bool IsLineColumn { get; set; } = false;

        public CodeFile CodeFile { get; set; } = CodeFile.Empty;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TextSpan);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TextSpan result;
            if (ShortFormat)
            {
                string textSpanString = (string)reader.Value;
                if (!IsLineColumn)
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
                if (!IsLineColumn)
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
            return result;
        }
    }
}
