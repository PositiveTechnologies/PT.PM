using PT.PM.Common;
using PT.PM.Matching;
using ReactiveUI;

namespace PT.PM.PatternEditor.ViewModels
{
    public class MatchResultViewModel : ReactiveObject
    {
        private PrettyPrinter textPrinter = new PrettyPrinter() { MaxMessageLength = 32 };

        public MatchResultDto MatchResult { get; set; }

        public MatchResultViewModel(MatchResultDto matchResult)
        {
            MatchResult = matchResult;
        }

        public override string ToString()
        {
            var codeFragment = textPrinter.Print(MatchResult.MatchedCode);
            return $"\"{codeFragment}\" matched at {MatchResult.LineColumnTextSpan}";
        }
    }
}
