﻿using PT.PM.Common;
using PT.PM.Common.SourceRepository;
using PT.PM.Dsl;
using PT.PM.Matching.PatternsRepository;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common.Files;
using PT.PM.Matching;

namespace PT.PM.TestUtils
{
    public static class PatternMatchingUtils
    {
        public static MatchResultDto[] GetMatches(string code, string pattern, Language analyseLanguage)
        {
            return GetMatches(code, pattern, new[] { analyseLanguage });
        }

        public static MatchResultDto[] GetMatches(string code, string pattern,
            IEnumerable<Language> analyzedLanguages,
            IEnumerable<Language> patternLanguages = null)
        {
            var sourceRep = new MemorySourceRepository(code)
            {
                Languages = new HashSet<Language>(analyzedLanguages)
            };
            var patternsRep = new MemoryPatternsRepository();
            var logger = new LoggerMessageCounter();
            var workflow = new Workflow(sourceRep, patternsRep) {Logger = logger};

            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(new TextFile(pattern) { PatternKey = pattern });
            patternNode.Languages = new HashSet<Language>(patternLanguages ?? LanguageUtils.PatternLanguages);
            patternNode.DebugInfo = pattern;
            var patternsConverter = new PatternConverter();
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRoot> { patternNode }));
            workflow.Process();
            MatchResultDto[] matchResults = logger.Matches.ToDto()
                .OrderBy(r => r.PatternKey)
                .ToArray();

            return matchResults;
        }
    }
}
