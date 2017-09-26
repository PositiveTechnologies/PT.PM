using PT.PM.Common.CodeRepository;
using PT.PM.Matching;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM
{
    public static class Extensions
    {
        public static MatchingResultDto[] ToDto(this IEnumerable<MatchingResult> matchingResults,
            ISourceCodeRepository sourceCodeRepository)
        {
            return matchingResults
                .Where(result => result != null)
                .Select(result => MatchingResultDto.CreateFromMatchingResult(result, sourceCodeRepository))
                .ToArray();
        }
    }
}
