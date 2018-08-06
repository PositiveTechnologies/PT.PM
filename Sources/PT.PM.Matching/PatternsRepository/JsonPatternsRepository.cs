using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
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

        public string DefaultDataFormat { get; set; } = "Json";

        public string DefaultKey { get; set; } = "";

        public string DefaultFilenameWildcard { get; set; } = "";

        public HashSet<Language> DefaultLanguages { get; set; } = new HashSet<Language>(LanguageUtils.PatternLanguages.Values);

        public JsonPatternsRepository(string patternsData)
        {
            this.patternsData = patternsData;
        }

        protected override List<PatternDto> InitPatterns()
        {
            JToken[] jsonTokens;

            try
            {
                jsonTokens = JToken.Parse(patternsData).ReadArray();
            }
            catch (Exception ex)
            {
                Logger.LogError(new ParsingException(
                   new CodeFile(patternsData) { PatternKey = DefaultKey }, ex, ex.FormatExceptionMessage()));
                jsonTokens = ArrayUtils<JToken>.EmptyArray;
            }

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
                            var converters = patternJsonSerializer.Converters;
                            var patternJsonConverterReader = new PatternJsonConverterReader(new CodeFile(patternsData))
                            {
                                Logger = Logger,
                                DefaultDataFormat = DefaultDataFormat,
                                DefaultKey = DefaultKey,
                                DefaultFilenameWildcard = DefaultFilenameWildcard,
                                DefaultLanguages = DefaultLanguages
                            };
                            converters.Add(patternJsonConverterReader);
                            var textSpanJsonConverter = new TextSpanJsonConverter
                            {
                                Logger = Logger
                            };
                            converters.Add(textSpanJsonConverter);

                            var codeFileJsonConverter = new CodeFileJsonConverter
                            {
                                Logger = Logger
                            };

                            codeFileJsonConverter.SetCurrentCodeFileEvent += (object sender, CodeFile codeFile) =>
                            {
                                textSpanJsonConverter.CurrentCodeFile = codeFile;
                            };

                            converters.Add(codeFileJsonConverter);
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
