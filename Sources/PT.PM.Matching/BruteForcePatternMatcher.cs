using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Patterns.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class BruteForcePatternMatcher : IUstPatternMatcher<RootNode, PatternRootNode, MatchingResult>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public IEnumerable<PatternRootNode> Patterns { get; set; }

        public bool IsIgnoreFilenameWildcards { get; set; }

        public BruteForcePatternMatcher(IEnumerable<PatternRootNode> patterns)
        {
            Patterns = patterns;
        }

        public BruteForcePatternMatcher()
        {
        }

        public List<MatchingResult> Match(RootNode ust)
        {
            if (ust.Nodes != null)
            {
                try
                {
                    var matchingResult = new List<MatchingResult>();
                    IEnumerable<PatternRootNode> patterns = Patterns
                        .Where(pattern => pattern.Languages.Any(patternLang => ust.Sublanguages.Contains(patternLang)));
                    if (!IsIgnoreFilenameWildcards)
                    {
                        patterns = patterns.Where(pattern => pattern.FilenameWildcardRegex?.IsMatch(ust.SourceCodeFile.FullPath) ?? true);
                    }
                    foreach (UstNode node in ust.Nodes)
                    {
                        Traverse(node, patterns, matchingResult);
                    }
                    MatchComments(ust, matchingResult, patterns);

                    return matchingResult;
                }
                catch (Exception ex)
                {
                    Logger.LogError(new MatchingException(ust.SourceCodeFile.FullPath, ex));
                    return new List<MatchingResult>();
                }
            }
            else
            {
                return new List<MatchingResult>();
            }
        }

        private void Traverse(UstNode node, IEnumerable<PatternRootNode> patterns, List<MatchingResult> matchingResult)
        {
            if (node == null)
                return;

            foreach (PatternRootNode pattern in patterns)
            {
                if (pattern.Node.Equals(node))
                {
                    AddMatchingResults(matchingResult, pattern, pattern.Node, node);
                }
            }

            foreach (var child in node.Children)
            {
                Traverse(child, patterns, matchingResult);
            }
        }

        private void MatchComments(RootNode ust, List<MatchingResult> matchingResults, IEnumerable<PatternRootNode> patterns)
        {
            PatternRootNode[] commentPatterns = patterns.Where(p =>
                p.Node.NodeType == NodeType.PatternComment ||
                (p.Node.NodeType == NodeType.PatternVarDef && ((PatternVarDef)p.Node).Values.Any(v => v.NodeType == NodeType.PatternComment))).ToArray();
            foreach (CommentLiteral commentNode in ust.Comments)
            {
                foreach (PatternRootNode commentPattern in commentPatterns)
                {
                    if (commentPattern.Node.Equals(commentNode))
                    {
                        PatternComment patternComment = null;
                        if (commentPattern.Node.NodeType == NodeType.PatternComment)
                        {
                            patternComment = (PatternComment)commentPattern.Node;
                        }
                        else if (commentPattern.Node.NodeType == NodeType.PatternVarDef)
                        {
                            patternComment = (PatternComment)((PatternVarDef)commentPattern.Node).Value;
                        }
                        if (patternComment != null)
                        {
                            AddMatchingResults(matchingResults, commentPattern, patternComment, commentNode);
                        }
                    }
                }
            }
        }

        private void AddMatchingResults(List<MatchingResult> matchingResults, PatternRootNode pattern, UstNode patternNode, UstNode codeNode)
        {
            var absoluteLocationPattern = patternNode as IAbsoluteLocationMatching;
            if (absoluteLocationPattern != null)
            {
                AddMathingResult(matchingResults, pattern, codeNode, absoluteLocationPattern.MatchedLocation);
            }
            else if (codeNode != null)
            {
                var relativeLocationPattern = patternNode as IRelativeLocationMatching;
                if (relativeLocationPattern != null)
                {
                    foreach (TextSpan location in relativeLocationPattern.MatchedLocations)
                    {
                        AddMathingResult(matchingResults, pattern, codeNode,
                            new TextSpan(codeNode.TextSpan.Start + location.Start, location.Length));
                    }
                }
                else
                {
                    AddMathingResult(matchingResults, pattern, codeNode, codeNode.TextSpan);
                }
            }
        }

        private void AddMathingResult(List<MatchingResult> matchingResults, PatternRootNode pattern, UstNode codeNode, TextSpan textSpan)
        {
            var matching = new MatchingResult(pattern, codeNode, textSpan);
            Logger.LogInfo(matching);
            matchingResults.Add(matching);
        }
    }
}
