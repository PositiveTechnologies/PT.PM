using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using PT.PM.Common.Exceptions;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Matching
{
    public class BruteForcePatternMatcher : IUstPatternMatcher<CommonPatternsDataStructure>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public CommonPatternsDataStructure PatternsData { get; set; }

        public BruteForcePatternMatcher(CommonPatternsDataStructure patternsData)
        {
            PatternsData = patternsData;
        }

        public BruteForcePatternMatcher()
        {
        }

        public List<MatchingResult> Match(Ust ust)
        {
            if (ust.Root != null)
            {
                try
                {
                    var matchingResult = new List<MatchingResult>();
                    PatternVarRefEnumerator[] patternEnumerators = PatternsData.Patterns
                        .Where(pattern => (pattern.Languages & ust.SourceLanguages) != LanguageFlags.None)
                        .Select(pattern => new PatternVarRefEnumerator(pattern)).ToArray();
                    Traverse(ust.Root, patternEnumerators, matchingResult);
                    MatchComments(ust, matchingResult, patternEnumerators);

                    return matchingResult;
                }
                catch (Exception ex)
                {
                    Logger.LogError(new MatchingException(ust.Root.FileName.Text, ex));
                    return new List<MatchingResult>();
                }
            }
            else
            {
                return new List<MatchingResult>();
            }
        }

        private void Traverse(UstNode node, PatternVarRefEnumerator[] patternsVarRef, List<MatchingResult> matchingResult)
        {
            if (node == null)
                return;

            foreach (var pattern in patternsVarRef)
            {
                if (Match(pattern, node))
                {
                    var matching = new MatchingResult(pattern.Pattern, node);
                    var patternStatements = pattern.Current as PatternStatements;
                    if (patternStatements != null)
                    {
                        matching.TextSpan = patternStatements.MatchedTextSpan;
                    }
                    Logger.LogInfo(matching);
                    matchingResult.Add(matching);
                }
            }

            foreach (var child in node.Children)
            {
                Traverse(child, patternsVarRef, matchingResult);
            }
        }

        private void MatchComments(Ust ust, List<MatchingResult> matchingResult, PatternVarRefEnumerator[] patternEnumerators)
        {
            PatternVarRefEnumerator[] commentPatterns = patternEnumerators.Where(p =>
                p.Pattern.Data.Node.NodeType == NodeType.PatternComment ||
                (p.Pattern.Data.Node.NodeType == NodeType.PatternVarDef && ((PatternVarDef)p.Pattern.Data.Node).Values.Any(v => v.NodeType == NodeType.PatternComment))).ToArray();
            foreach (var commentNode in ust.Comments)
            {
                foreach (var commentPattern in commentPatterns)
                {
                    if (Match(commentPattern, commentNode))
                    {
                        PatternComment patternComment;
                        if (commentPattern.Current.NodeType == NodeType.PatternComment)
                        {
                            patternComment = (PatternComment)commentPattern.Current;
                        }
                        else
                        {
                            patternComment = (PatternComment)((PatternVarDef)commentPattern.Current).Value;
                        }
                        var matching = new MatchingResult(commentPattern.Pattern, commentNode)
                        {
                            TextSpan = new TextSpan(commentNode.TextSpan.Start + patternComment.Offset, patternComment.Length)
                        };
                        Logger.LogInfo(matching);
                        matchingResult.Add(matching);
                    }
                }
            }
        }

        private bool Match(PatternVarRefEnumerator patternVarRefEnum, UstNode node)
        {
            try
            {
                while (patternVarRefEnum.MoveNext())
                {
                    if (patternVarRefEnum.Current.Equals(node))
                    {
                        patternVarRefEnum.Reset();
                        return true;
                    }
                }

                patternVarRefEnum.Reset();
            }
            catch (Exception ex)
            {
                Logger.LogError(new MatchingException(node.FileNode.FileName.Text, ex) { TextSpan = node.TextSpan });
            }
            return false;
        }
    }
}
