using System;
using Newtonsoft.Json;
using System.Linq;

namespace PT.PM.Common.Json
{
    public class LanguageJsonConverter : JsonConverter
    {
        public static LanguageJsonConverter Instance = new LanguageJsonConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Language);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Language)value).Key);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return LanguageUtils.ParseLanguages((string)reader.Value).FirstOrDefault() ?? Uncertain.Language;
        }
    }
}
