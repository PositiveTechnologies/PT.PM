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
using PT.PM.Common.Files;

namespace PT.PM.Matching.PatternsRepository
{
    public class JsonPatternsRepository : MemoryPatternsRepository
    {
        protected string patternsData;
        private PatternConverter patternConverter = new PatternConverter();
        private JsonSerializer patternJsonSerializer;

        public string DefaultDataFormat { get; set; } = "Json";

        public string DefaultKey { get; set; } = "";

        public string DefaultFilenameWildcard { get; set; } = "";

        public HashSet<Language> DefaultLanguages { get; set; } = new HashSet<Language>(LanguageUtils.PatternLanguages);

        public JsonPatternsRepository(string patternsData)
        {
            this.patternsData = patternsData;
        }

        public JsonPatternsRepository()
        {
        }

        protected override List<PatternDto> InitPatterns()
        {
            JToken patternToken = null;

            try
            {
                patternToken = JToken.Parse(patternsData);
            }
            catch (Exception ex)
            {
                Logger.LogError(new ParsingException(
                   new TextFile(patternsData) { PatternKey = DefaultKey }, ex, ex.FormatExceptionMessage()));
            }

            List<PatternDto> result;
            patternJsonSerializer = null;

            if (patternToken is JArray jArray)
            {
                result = new List<PatternDto>(jArray.Count);

                foreach (JToken token in jArray)
                {
                    result.Add(ProcessToken(token));
                }
            }
            else
            {
                result = new List<PatternDto>(1) {ProcessToken(patternToken)};
            }

            return result;
        }

        private PatternDto ProcessToken(JToken token)
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
                        var patternJsonConverterReader = new PatternJsonConverterReader(new TextFile(patternsData))
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

                        var sourceFileJsonConverter = new SourceFileJsonConverter
                        {
                            Logger = Logger,
                            SetCurrentSourceFileAction = sourceFile =>
                            {
                                textSpanJsonConverter.CurrentSourceFile = sourceFile;
                            }
                        };

                        converters.Add(sourceFileJsonConverter);
                    }

                    PatternRoot patternRoot = token.ToObject<PatternRoot>(patternJsonSerializer);
                    patternDto = patternConverter.ConvertBack(new[] {patternRoot})[0];
                }
                else
                {
                    patternDto = new PatternDto
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
                    var languagesToken = token["Languages"];

                    if (languagesToken == null)
                    {
                        languages = new HashSet<string>();
                    }
                    else if (languagesToken is JArray)
                    {
                        languages = new HashSet<string>(languagesToken.Select(x => x.ToString()));
                    }
                    else
                    {
                        languages = new HashSet<string>(languagesToken.ToString()
                            .Split(',').Select(x => x.Trim()));
                    }

                    patternDto.Languages = languages;
                }

                return patternDto;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return null;
        }
    }
}
