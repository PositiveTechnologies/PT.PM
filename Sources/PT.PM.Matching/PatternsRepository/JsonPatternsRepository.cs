using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Json;
using PT.PM.Matching.Json;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

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
                        patternDto = new PatternDto()
                        {
                            Name = token[nameof(PatternDto.Name)]?.ToString() ?? string.Empty,
                            Key = token[nameof(PatternDto.Key)]?.ToString() ?? string.Empty,
                            DataFormat = token[nameof(PatternDto.DataFormat)]?.ToString(),
                            Value = token[nameof(PatternDto.Value)]?.ToString(),
                            CweId = token[nameof(PatternDto.CweId)]?.ToString(),
                            Description = token[nameof(PatternDto.Description)]?.ToString(),
                            FilenameWildcard = token[nameof(PatternDto.FilenameWildcard)]?.ToString()
                        };

                        HashSet<string> languages;
                        try
                        {
                            languages = token["Languages"]?.ToObject<HashSet<string>>();
                        }
                        catch(JsonSerializationException)
                        {
                            languages = new HashSet<string>(token["Languages"].ToString()
                                .Split(',').Select(x => x.Trim()));
                        }

                        patternDto.Languages = languages;
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
