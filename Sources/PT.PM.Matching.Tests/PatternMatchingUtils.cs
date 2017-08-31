using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.Common.Nodes;
using PT.PM.Dsl;
using PT.PM.Patterns;
using PT.PM.Patterns.Nodes;
using PT.PM.Patterns.PatternsRepository;
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
            var patternNode = (PatternRootNode)processor.Deserialize(pattern);
            patternNode.Languages = new HashSet<Language>(patternLanguages ?? LanguageExt.AllPatternLanguages);
            patternNode.DebugInfo = pattern;
            var patternsConverter = new PatternConverter(
                new JsonUstNodeSerializer(typeof(UstNode), typeof(PatternVarDef)));
            patternsRep.Add(patternsConverter.ConvertBack(new List<PatternRootNode>() { patternNode }));
            WorkflowResult workflowResult = workflow.Process();
            MatchingResultDto[] matchingResults = workflowResult.MatchingResults.ToDto(workflow.SourceCodeRepository)
                .OrderBy(r => r.PatternKey)
                .ToArray();

            return matchingResults;
        }
    }
}
