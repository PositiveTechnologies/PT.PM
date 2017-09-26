using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Matching.Patterns;
using PT.PM.Matching.PatternsRepository;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching.Tests
{
    public class PatternMatchingUtils
    {
        public static MatchingResultDto[] GetMatchings(string code, string pattern, Language analyseLanguage)
        {
            return GetMatchings(code, pattern, new[] { analyseLanguage });
        }

        public static MatchingResultDto[] GetMatchings(string code, string pattern,
            IEnumerable<Language> analyzedLanguages,
            IEnumerable<Language> patternLanguages = null)
        {
            var sourceCodeRep = new MemoryCodeRepository(code);
            var patternsRep = new MemoryPatternsRepository();
            var workflow = new Workflow(sourceCodeRep, analyzedLanguages, patternsRep)
            {
                Logger = new LoggerMessageCounter()
            };

            var processor = new DslProcessor();
            var patternNode = (PatternRootUst)processor.Deserialize(pattern);
            patternNode.Languages = new HashSet<Language>(patternLanguages ?? LanguageExt.AllPatternLanguages);
            patternNode.DebugInfo = pattern;
            var patternsConverter = new PatternConverter(
                new JsonUstSerializer());
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRootUst>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            MatchingResultDto[] matchingResults = workflowResult.MatchingResults.ToDto(workflow.SourceCodeRepository)
                .OrderBy(r => r.PatternKey)
                .ToArray();

            return matchingResults;
        }
    }
}
