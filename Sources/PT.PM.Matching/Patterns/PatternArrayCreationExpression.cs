using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternArrayCreationExpression : PatternUst, IPatternExpression
    {
        [JsonIgnore]
        public Type UstType => typeof(ArrayCreationExpression);

        public PatternUst Type { get; set; }

        public bool StackAllocated { get; set; } = false;

        public List<PatternUst> Sizes { get; set; }

        public List<PatternUst> Initializers { get; set; }

        public PatternArrayCreationExpression()
        {
            Type = new PatternAny();
            Sizes = new List<PatternUst>();
            Initializers = new List<PatternUst>();
        }

        public PatternArrayCreationExpression(PatternUst type, List<PatternUst> sizes, List<PatternUst> initializers,
            TextSpan textSpan = default) : base(textSpan)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Sizes = sizes ?? throw new ArgumentNullException(nameof(sizes));
            Initializers = initializers ?? throw new ArgumentNullException(nameof(initializers));
        }

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var arrayCreationExpression = ust as ArrayCreationExpression;
            if (arrayCreationExpression == null)
            {
                return context.Fail();
            }

            // If we're looking for a stack allocated array there should be NO `new` keyword
            if (StackAllocated && arrayCreationExpression.KeywordNew != null)
            {
                return context.Fail();
            }

            MatchContext newContext = Type.MatchUst(arrayCreationExpression.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            if (Sizes.Count != arrayCreationExpression.Sizes.Count)
            {
                return context.Fail();
            }
            for (int i = 0; i < Sizes.Count; i++)
            {
                newContext = Sizes[i].MatchUst(arrayCreationExpression.Sizes[i], newContext);
                if (!newContext.Success)
                {
                    return newContext;
                }
            }

            if (Initializers.Count != arrayCreationExpression.Initializers.Count)
            {
                return context.Fail();
            }
            for (int i = 0; i < Initializers.Count; i++)
            {
                newContext = Initializers[i].MatchUst(arrayCreationExpression.Initializers[i], newContext);
                if (!newContext.Success)
                {
                    return newContext;
                }
            }

            return newContext.AddUstIfSuccess(arrayCreationExpression);
        }

        public PatternUst[] GetArgs()
        {
            var result = new List<PatternUst> { Type };
            result.AddRange(Sizes);
            result.AddRange(Initializers);

            return result.ToArray();
        }
    }
}