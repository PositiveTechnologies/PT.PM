using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using PT.PM.Common.Json;
using PT.PM.Matching.Json;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.PatternsRepository
{
    public class JsonPatternsRepository : MemoryPatternsRepository
    {
        private string patternsData;
        private PatternConverter patternConverter = new PatternConverter();

        public JsonPatternsRepository(string patternsData)
        {
            this.patternsData = patternsData;
        }

        protected override List<PatternDto> InitPatterns()
        {
            JToken[] jsonTokens = JToken.Parse(patternsData).ReadArray();

            var result = new List<PatternDto>();
            JsonSerializer patternJsonSerializer = null;

            foreach (JToken token in jsonTokens)
            {
                try
                {
                    PatternDto patternDto;
                    if (token[nameof(PatternUst.Kind)] != null)
                    {
                        if (patternJsonSerializer == null)
                        {
                            patternJsonSerializer = new JsonSerializer();
                            patternJsonSerializer.Converters.Add(new PatternJsonConverterReader(new CodeFile(patternsData)));
                            var textSpanJsonConverter = new TextSpanJsonConverter();
                            patternJsonSerializer.Converters.Add(textSpanJsonConverter);
                            patternJsonSerializer.Converters.Add(new CodeFileJsonConverter() { TextSpanJsonConverter = textSpanJsonConverter });
                        }
                        PatternRoot patternRoot = token.ToObject<PatternRoot>(patternJsonSerializer);
                        patternDto = patternConverter.ConvertBack(new[] { patternRoot })[0];
                    }
                    else
                    {
                        patternDto = token.ToObject<PatternDto>();
                    }
                    result.Add(patternDto);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }

            return result;
        }
    }
}
