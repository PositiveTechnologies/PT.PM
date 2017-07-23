using PT.PM.Common;
using PT.PM.Matching;
using PT.PM.Patterns;

namespace PT.PM
{
    public class WorkflowResult : WorkflowResultBase<Stage, Pattern, MatchingResult>
    {
        public WorkflowResult(Language[] languages, int threadCount, Stage stage, bool isIncludeIntermediateResult)
            : base(languages, threadCount, stage, isIncludeIntermediateResult)
        {
        }
    }
}
