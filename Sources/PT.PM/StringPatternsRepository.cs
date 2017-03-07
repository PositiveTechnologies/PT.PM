﻿using PT.PM.Common;
using PT.PM.Patterns;
using PT.PM.Patterns.PatternsRepository;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace PT.PM
{
    public class StringPatternsRepository : MemoryPatternsRepository
    {
        private string patternsData;
        private readonly JsonConverter stringEnumConverter;

        public StringPatternsRepository(string patternsData)
        {
            this.patternsData = patternsData;
            stringEnumConverter = new StringEnumConverter();
        }

        protected override List<PatternDto> InitPatterns()
        {
            var languageFlagsConverter = new PatternLanguageFlagsSafeConverter
            {
                Logger = Logger
            };

            List<PatternDto> patternDtos = JsonConvert
                .DeserializeObject<List<PatternDto>>(patternsData, stringEnumConverter, languageFlagsConverter);

            var result = new List<PatternDto>();
            foreach (var patternDto in patternDtos)
            {
                if (patternDto.Languages == LanguageFlags.None)
                {
                    Logger.LogInfo($"Pattern \"{patternDto.Key}\" ignored because of it doesn't have target languages.");
                }
                else
                {
                    result.Add(patternDto);
                }
            }

            return result;
        }
    }
}
