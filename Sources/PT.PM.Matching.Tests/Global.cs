using NUnit.Framework;
using PT.PM.Matching.PatternsRepository;
using PT.PM.Patterns.PatternsRepository;

namespace PT.PM.Matching.Tests
{
    [SetUpFixture]
    public class Global
    {
        internal static IPatternsRepository PatternsRepository;

        [OneTimeSetUp]
        public static void AssemblyInitalize()
        {
            PatternsRepository = new DefaultPatternRepository();
        }
    }
}
