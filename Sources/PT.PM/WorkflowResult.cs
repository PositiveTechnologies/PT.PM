using PT.PM.Common;
using PT.PM.Matching;
using System.Collections.Generic;

namespace PT.PM
{
    public class WorkflowResult : WorkflowResultBase<Stage, PatternRoot, MatchResult, Stage>
    {
        public WorkflowResult(List<Language> languages, int threadCount, Stage stage)
            : base(languages, threadCount, stage)
        {
        }
    }
}
