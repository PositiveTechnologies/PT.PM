namespace PT.PM
{
    public class WorkflowResult : WorkflowResultBase<Stage>
    {
        public WorkflowResult(Stage stage, bool isIncludeIntermediateResult)
            : base(stage, isIncludeIntermediateResult)
        {
        }
    }
}
