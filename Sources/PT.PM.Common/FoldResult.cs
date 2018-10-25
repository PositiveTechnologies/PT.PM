using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common
{
    public class FoldResult
    {
#if DEBUG
        public CodeFile CodeFile { get; set; }
#endif
        
        public object Value { get; }
        
        public List<TextSpan> TextSpans { get; }

        public FoldResult(object value, TextSpan textSpan)
            : this(value, new List<TextSpan> {textSpan})
        {
        }

        public FoldResult(object value, List<TextSpan> textSpans)
        {
            Value = value;
            TextSpans = textSpans ?? throw new ArgumentNullException(nameof(textSpans));
        }

        public override string ToString()
        {
            return Value + " at " +
#if DEBUG
                string.Join("; ", TextSpans.Select(textSpan => CodeFile?.GetLineColumnTextSpan(textSpan).ToString() ?? textSpan.ToString()));
#else
                string.Join("; ", TextSpans);
#endif
        }
    }
}