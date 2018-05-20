using System;
using Newtonsoft.Json;
using System.Linq;

namespace PT.PM.Common.Json
{
    public class LanguageJsonConverter : JsonConverter<Language>
    {
        public static LanguageJsonConverter Instance = new LanguageJsonConverter();

        public override void WriteJson(JsonWriter writer, Language language, JsonSerializer serializer)
        {
            writer.WriteValue(language.Key);
        }

        public override Language ReadJson(JsonReader reader, Type objectType, Language existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return LanguageUtils.ParseLanguages((string)reader.Value).FirstOrDefault() ?? Uncertain.Language;
        }
    }
}
