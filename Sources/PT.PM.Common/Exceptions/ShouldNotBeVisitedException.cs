namespace PT.PM.Common.Exceptions
{
    public class ShouldNotBeVisitedException : PMException
    {
        private readonly string _ruleName = "";

        public ShouldNotBeVisitedException()
        {
        }

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