using PT.PM.Common;
using PT.PM.Matching;
using System.Collections.Generic;

namespace PT.PM
{
    public class WorkflowResult : WorkflowResultBase<Stage, PatternRoot, MatchingResult>
    {
        public WorkflowResult(IEnumerable<LanguageInfo> languages, int threadCount, Stage stage, bool isIncludeIntermediateResult)
            : base(languages, threadCount, stage, isIncludeIntermediateResult)
        {
        }
    }
}
