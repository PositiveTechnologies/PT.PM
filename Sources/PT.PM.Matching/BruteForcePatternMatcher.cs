using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class BruteForcePatternMatcher : IUstPatternMatcher<RootUst, PatternRootUst, MatchingResult>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public PatternRootUst[] Patterns { get; set; }

        public bool IsIgnoreFilenameWildcards { get; set; }

        public BruteForcePatternMatcher(PatternRootUst[] patterns)
        {
            Patterns = patterns;
        }

        public BruteForcePatternMatcher()
        {
        }

        public List<MatchingResult> Match(RootUst ust)
        {
            if (ust.Nodes != null)
            {
                try
                {
                    var matchingResult = new List<MatchingResult>();
                    IEnumerable<PatternRootUst> patterns = Patterns
                        .Where(pattern => pattern.Languages.Any(patternLang => ust.Sublanguages.Contains(patternLang)));
                    if (!IsIgnoreFilenameWildcards)
                    {
                        patterns = patterns.Where(pattern => pattern.FilenameWildcardRegex?.IsMatch(ust.SourceCodeFile.FullPath) ?? true);
                    }
                    foreach (Ust node in ust.Nodes)
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

        private void Traverse(Ust node, IEnumerable<PatternRootUst> patterns, List<MatchingResult> matchingResult)
        {
            if (node == null)
                return;

            foreach (PatternRootUst pattern in patterns)
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

        private void MatchComments(RootUst ust, List<MatchingResult> matchingResults, IEnumerable<PatternRootUst> patterns)
        {
            PatternRootUst[] commentPatterns = patterns.Where(p =>
                p.Node.Kind == UstKind.PatternComment ||
                (p.Node.Kind == UstKind.PatternVarDef && ((PatternVarDef)p.Node).Values.Any(v => v.Kind == UstKind.PatternComment))).ToArray();
            foreach (CommentLiteral commentNode in ust.Comments)
            {
                foreach (PatternRootUst commentPattern in commentPatterns)
                {
                    if (commentPattern.Node.Equals(commentNode))
                    {
                        PatternComment patternComment = null;
                        if (commentPattern.Node.Kind == UstKind.PatternComment)
                        {
                            patternComment = (PatternComment)commentPattern.Node;
                        }
                        else if (commentPattern.Node.Kind == UstKind.PatternVarDef)
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

        private void AddMatchingResults(List<MatchingResult> matchingResults, PatternRootUst pattern, Ust patternNode, Ust codeNode)
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

        private void AddMathingResult(List<MatchingResult> matchingResults, PatternRootUst pattern, Ust codeNode, TextSpan textSpan)
        {
            var matching = new MatchingResult(pattern, codeNode, textSpan);
            Logger.LogInfo(matching);
            matchingResults.Add(matching);
        }
    }
}
