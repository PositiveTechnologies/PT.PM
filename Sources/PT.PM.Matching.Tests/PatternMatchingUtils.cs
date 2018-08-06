using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Dsl;
using PT.PM.Matching.PatternsRepository;
using PT.PM.TestUtils;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    public class PatternMatchingUtils
    {
        public static MatchResultDto[] GetMatches(string code, string pattern, Language analyseLanguage)
        {
            return GetMatches(code, pattern, new[] { analyseLanguage });
        }

        public static MatchResultDto[] GetMatches(string code, string pattern,
            IEnumerable<Language> analyzedLanguages,
            IEnumerable<Language> patternLanguages = null)
        {
            var sourceCodeRep = new MemoryCodeRepository(code)
            {
                Languages = new HashSet<Language>(analyzedLanguages)
            };
            var patternsRep = new MemoryPatternsRepository();
            var workflow = new Workflow(sourceCodeRep, patternsRep)
            {
                Logger = new LoggerMessageCounter()
            };

            var processor = new DslProcessor();
            PatternRoot patternNode = processor.Deserialize(new CodeFile(pattern) { PatternKey = pattern });
            patternNode.Languages = new HashSet<Language>(patternLanguages ?? LanguageUtils.PatternLanguages.Values);
            patternNode.DebugInfo = pattern;
            var patternsConverter = new PatternConverter();
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRoot>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            MatchResultDto[] matchResults = workflowResult.MatchResults.ToDto()
                .OrderBy(r => r.PatternKey)
                .ToArray();

            return matchResults;
        }
    }
}
