using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Json;
using PT.PM.Matching.Patterns;
using System;

namespace PT.PM.Matching.Json
{
    public class PatternJsonConverterWriter : UstJsonConverterWriter
    {
        public override bool CanConvert(Type objectType) => objectType.CanConvert();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is IRegexPattern regexPattern)
            {
                PatternUst patternUst = (PatternUst)value;
                JObject jObject = CreateJObject(patternUst);

                if (!ExcludeDefaults || !patternUst.Any)
                {
                    jObject.Add(nameof(IRegexPattern.Regex), regexPattern.RegexString);
                }

                AddTextSpanAndWrite(writer, jObject, patternUst, serializer);
            }
            else if (value is PatternIntRangeLiteral patternIntRangeLiteral)
            {
                JObject jObject = CreateJObject(patternIntRangeLiteral);

                if (!ExcludeDefaults || !patternIntRangeLiteral.Any)
                {
                    string str = patternIntRangeLiteral.ToString();
                    jObject.Add(nameof(PatternIntLiteral.Value), str.Substring(2, str.Length - 4));
                }

                AddTextSpanAndWrite(writer, jObject, patternIntRangeLiteral, serializer);
            }
            else
            {
                base.WriteJson(writer, value, serializer);
            }
        }

        private static JObject CreateJObject(PatternUst patternUst)
        {
            var jObject = new JObject();
            jObject.Add(nameof(PatternUst.Kind), patternUst.Kind);
            return jObject;
        }

        private void AddTextSpanAndWrite(JsonWriter writer, JObject jObject, PatternUst patternUst, JsonSerializer serializer)
        {
            if (IncludeTextSpans && (!ExcludeDefaults || !patternUst.TextSpan.IsZero))
            {
                jObject.Add(nameof(PatternUst.TextSpan), JToken.FromObject(patternUst.TextSpan, serializer));
            }
            jObject.WriteTo(writer);
        }
    }
}
