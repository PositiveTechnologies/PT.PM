using PT.PM.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PT.PM.Patterns
{
    public class PatternJsonSafeConverter: JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PatternDto);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var languageFlagsString = (string)jObject[nameof(PatternDto.Languages)];
            LanguageFlags resultLanguages = LanguageExt.ParseLanguages(languageFlagsString);

            var result = new PatternDto
            {
                Name = (string)jObject[nameof(PatternDto.Name)] ?? "",
                Key = (string)jObject[nameof(PatternDto.Key)] ?? "",
                Languages = resultLanguages,
                FilenameWildcard = (string)jObject[nameof(PatternDto.FilenameWildcard)] ?? "",
                Value = (string)jObject[nameof(PatternDto.Value)] ?? "",
                CweId = (string)jObject[nameof(PatternDto.CweId)] ?? "",
                Description = (string)jObject[nameof(PatternDto.Description)] ?? "",
            };
            var dataFormatString = (string)jObject[nameof(PatternDto.DataFormat)];
            UstNodeSerializationFormat format;
            if (dataFormatString != null && Enum.TryParse(dataFormatString, out format))
            {
                result.DataFormat = format;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException($"Do not use {nameof(PatternJsonSafeConverter)} for serialization.");
        }
    }
}
