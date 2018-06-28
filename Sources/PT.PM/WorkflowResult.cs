using PT.PM.Common;
using PT.PM.Matching;
using System.Collections.Generic;

namespace PT.PM
{
    public class WorkflowResult : WorkflowResultBase<Stage, PatternRoot, MatchResult, Stage>
    {
        public WorkflowResult(IEnumerable<Language> languages, int threadCount, Stage stage, bool isIncludeIntermediateResult)
            : base(languages, threadCount, stage, isIncludeIntermediateResult)
        {
        }
    }
}
