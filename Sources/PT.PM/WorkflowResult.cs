using PT.PM.Matching;
using PT.PM.Patterns;

namespace PT.PM
{
    public class WorkflowResult : WorkflowResultBase<Stage, Pattern, MatchingResult>
    {
        public WorkflowResult(Stage stage, bool isIncludeIntermediateResult)
            : base(stage, isIncludeIntermediateResult)
        {
        }
    }
}
