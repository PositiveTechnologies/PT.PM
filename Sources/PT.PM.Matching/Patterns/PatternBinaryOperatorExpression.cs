﻿using System;
using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternBinaryOperatorExpression : PatternUst, IPatternExpression
    {
        public PatternUst Left { get; set; }

        public PatternBinaryOperatorLiteral Operator { get; set; }

        public PatternUst Right { get; set; }

        public PatternBinaryOperatorExpression()
        {
        }

        public PatternBinaryOperatorExpression(PatternUst left, PatternBinaryOperatorLiteral op, PatternUst right,
            TextSpan textSpan = default)
            : base(textSpan)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        public override string ToString() => $"{Left} {Operator} {Right}";

        protected override MatchContext Match(Ust ust, MatchContext context)
        {
            var binaryOperatorExpression = ust as BinaryOperatorExpression;
            if (binaryOperatorExpression == null)
            {
                return context.Fail();
            }
            
            MatchContext newContext = Left.MatchUst(binaryOperatorExpression.Left, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Operator.MatchUst(binaryOperatorExpression.Operator, newContext);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Right.MatchUst(binaryOperatorExpression.Right, newContext);

            return newContext.AddUstIfSuccess(binaryOperatorExpression);
        }

        public PatternUst[] GetArgs()
        {
            var result = new List<PatternUst>();
            result.Add(Left);
            result.Add(Right);
            // FIXME
            // result.Add(Operator);
            return result.ToArray();
        }

        public Type UstType => typeof(BinaryOperatorExpression);
    }
}
