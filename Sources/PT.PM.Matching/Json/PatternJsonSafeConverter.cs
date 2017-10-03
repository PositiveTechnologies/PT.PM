using PT.PM.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Json
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
            var languagesArray = (JArray)jObject[nameof(PatternDto.Languages)];
            HashSet<Language> resultLanguages;
            if (languagesArray?.Count > 0)
            {
                resultLanguages = new HashSet<Language>(languagesArray.Values<string>()
                    .Select(value => Enum.TryParse(value, true, out Language language) ? (Language?)language : null)
                    .Where(lang => lang != null)
                    .Cast<Language>());
            }
            else
            {
                resultLanguages = new HashSet<Language>(LanguageExt.AllPatternLanguages);
            }

            var result = new PatternDto
            {
                Name = (string)jObject[nameof(PatternDto.Name)] ?? "",
                Key = (string)jObject[nameof(PatternDto.Key)] ?? "",
                Languages = new HashSet<Language>(resultLanguages),
                FilenameWildcard = (string)jObject[nameof(PatternDto.FilenameWildcard)] ?? "",
                Value = (string)jObject[nameof(PatternDto.Value)] ?? "",
                CweId = (string)jObject[nameof(PatternDto.CweId)] ?? "",
                Description = (string)jObject[nameof(PatternDto.Description)] ?? "",
                DataFormat = (string)jObject[nameof(PatternDto.DataFormat)]
            };

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException($"Do not use {nameof(PatternJsonSafeConverter)} for serialization.");
        }
    }
}
