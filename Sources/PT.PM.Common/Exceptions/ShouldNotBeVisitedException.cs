namespace PT.PM.Common.Exceptions
{
    public class ShouldNotBeVisitedException : PMException
    {
        public override PMExceptionType ExceptionType => PMExceptionType.ShouldNotBeVisited;

        private readonly string _ruleName;

        public ShouldNotBeVisitedException(string ruleName)
        {
            _ruleName = ruleName;
        }

        public override string ToString()
        {
            return $"Node {_ruleName} should not be visited";
        }
    }
}