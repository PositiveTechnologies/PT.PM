using System;
using System.Collections.Generic;
using System.Linq;
using PT.PM.Common;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using PT.PM.Patterns;
using PT.PM.Common.Exceptions;
using PT.PM.Patterns.Nodes;

namespace PT.PM.Matching
{
    public class BruteForcePatternMatcher : IUstPatternMatcher<Ust, Pattern, MatchingResult>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Pattern[] Patterns { get; set; }

        public bool IsIgnoreFilenameWildcards { get; set; }

        public BruteForcePatternMatcher(Pattern[] patterns)
        {
            Patterns = patterns;
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
                    IEnumerable<Pattern> patterns = Patterns
                        .Where(pattern => (pattern.Languages & ust.SourceLanguages) != LanguageFlags.None);
                    if (!IsIgnoreFilenameWildcards)
                    {
                        patterns = patterns.Where(pattern => pattern.FilenameWildcardRegex?.IsMatch(ust.FileName) ?? true);
                    }
                    PatternVarRefEnumerator[] patternEnumerators = patterns
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

            foreach (PatternVarRefEnumerator pattern in patternsVarRef)
            {
                if (Match(pattern, node))
                {
                    AddMatchingResults(matchingResult, pattern.Pattern, pattern.Current, node);
                }
            }

            foreach (var child in node.Children)
            {
                Traverse(child, patternsVarRef, matchingResult);
            }
        }

        private void MatchComments(Ust ust, List<MatchingResult> matchingResults, PatternVarRefEnumerator[] patternEnumerators)
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
                        AddMatchingResults(matchingResults, commentPattern.Pattern, commentPattern.Current, commentNode);
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

        private void AddMatchingResults(List<MatchingResult> matchingResults, Pattern pattern, UstNode patternNode, UstNode codeNode)
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

        private void AddMathingResult(List<MatchingResult> matchingResults, Pattern pattern, UstNode codeNode, TextSpan textSpan)
        {
            var matching = new MatchingResult(pattern, codeNode, textSpan);
            Logger.LogInfo(matching);
            matchingResults.Add(matching);
        }
    }
}
