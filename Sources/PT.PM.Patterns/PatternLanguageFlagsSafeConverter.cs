using PT.PM.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace PT.PM.Patterns
{
    public class PatternLanguageFlagsSafeConverter: JsonConverter, ILoggable
    {
        private static char[] languageFlagSeparators = new char[] { ',', ' ', '\t' };

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PatternDto);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var languageFlagsString = (string)jObject[nameof(PatternDto.Languages)];
            LanguageFlags resultLanguages;
            if (languageFlagsString != null)
            {
                resultLanguages = LanguageFlags.None;
                string[] languageStrings = languageFlagsString.Split(languageFlagSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string languageString in languageStrings)
                {
                    Language language;
                    if (Enum.TryParse(languageString, true, out language))
                    {
                        resultLanguages |= language.ToFlags();
                    }
                    else
                    {
                        Logger.LogError($"Language \"{languageString}\" is not supported or wrong.");
                    }
                }
            }
            else
            {
                resultLanguages = LanguageExt.AllPatternLanguages;
            }

            var result = new PatternDto
            {
                Name = (string)jObject[nameof(PatternDto.Name)] ?? "",
                Key = (string)jObject[nameof(PatternDto.Key)] ?? "",
                Languages = resultLanguages,
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
            throw new InvalidOperationException($"Do not use {nameof(PatternLanguageFlagsSafeConverter)} for serialization.");
        }
    }
}
