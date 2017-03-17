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
        public static MatchingResultDto[] GetMatchings(string code, string pattern, Language analyseLanguages)
        {
            return GetMatchings(code, pattern, analyseLanguages.ToFlags());
        }

        public static MatchingResultDto[] GetMatchings(string code, string pattern, LanguageFlags analyzedLanguages,
            LanguageFlags? patternLanguages = null)
        {
            var sourceCodeRep = new MemoryCodeRepository(code);
            var patternsRep = new MemoryPatternsRepository();
            var workflow = new Workflow(sourceCodeRep, analyzedLanguages, patternsRep)
            {
                Logger = new LoggerMessageCounter()
            };

            var processor = new DslProcessor();
            var patternNode = (PatternNode)processor.Deserialize(pattern, patternLanguages ?? LanguageExt.AllPatternLanguages);
            var p = new Pattern
            {
                Data = patternNode,
                DebugInfo = pattern
            };
            var patternsConverter = new CommonPatternConverter(
                new JsonUstNodeSerializer(typeof(UstNode), typeof(PatternVarDef)));
            var patternsDataStructure = new CommonPatternsDataStructure(new List<Pattern>() { p });
            patternsRep.Add(patternsConverter.ConvertBack(patternsDataStructure));
            WorkflowResult workflowResult = workflow.Process();
            MatchingResultDto[] matchingResults = workflowResult.MatchingResults.ToDto(workflow.SourceCodeRepository)
                .OrderBy(r => r.PatternKey)
                .ToArray();

            return matchingResults;
        }
    }
}
