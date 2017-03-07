using System;

namespace PT.PM.Common.Exceptions
{
    public class ShouldNotBeVisitedException : Exception
    {
        public override string ToString()
        {
            return "Should not be visited";
        }
    }
}