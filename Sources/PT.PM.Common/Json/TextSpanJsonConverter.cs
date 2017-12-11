using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PT.PM.Common.Json
{
    public class TextSpanJsonConverter : JsonConverter
    {
        public bool ShortFormat { get; set; } = true;

        public string EmptyTextSpanFormat { get; set; } = null;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TextSpan);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var textSpan = (TextSpan)value;
            if (ShortFormat)
            {
                string textSpanString = textSpan.IsEmpty && EmptyTextSpanFormat != null
                    ? EmptyTextSpanFormat
                    : textSpan.ToString();
                writer.WriteValue(textSpanString);
            }
            else
            {
                JObject pdgObject = new JObject();
                pdgObject.Add(nameof(textSpan.Start), textSpan.Start);
                pdgObject.Add(nameof(textSpan.Length), textSpan.Length);
                pdgObject.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (ShortFormat)
            {
                string textSpanString = (string)reader.Value;
                if (textSpanString == EmptyTextSpanFormat)
                {
                    return TextSpan.Empty;
                }
                else
                {
                    return TextSpan.Parse(textSpanString);
                }
            }
            else
            {
                var jObject = JObject.Load(reader);
                int start = jObject.GetValueIgnoreCase(nameof(TextSpan.Start))?.ToObject<int>() ?? 0;
                int length = jObject.GetValueIgnoreCase(nameof(TextSpan.Length))?.ToObject<int>() ?? 0;
                return new TextSpan(start, length);
            }
        }
    }
}
