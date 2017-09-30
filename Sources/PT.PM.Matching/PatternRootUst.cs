using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using PT.PM.Matching.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching
{
    public class PatternRootUst : RootUst, ILoggable
    {
        private HashSet<Language> languages = new HashSet<Language>();
        private Regex pathWildcardRegex;

        [JsonIgnore]
        public ILogger Logger { get; set; }

        public string Key { get; set; } = "";

        public string FilenameWildcard { get; set; }

        public HashSet<Language> Languages
        {
            get
            {
                return languages;
            }
            set
            {
                if (languages.Contains(Language.Aspx))
                {
                    throw new ArgumentException($"Unable to create pattern for Aspx");
                }
                languages = value;
            }
        }

        public UstFormat DataFormat { get; set; } = UstFormat.Json;

        [JsonIgnore]
        public string DebugInfo { get; set; } = "";

        [JsonIgnore]
        public Regex FilenameWildcardRegex
        {
            get
            {
                if (!string.IsNullOrEmpty(FilenameWildcard) && pathWildcardRegex == null)
                {
                    pathWildcardRegex = new WildcardConverter().Convert(FilenameWildcard);
                }
                return pathWildcardRegex;
            }
        }

        public PatternRootUst(string key, string debugInfo, IEnumerable<Language> languages, string filenameWildcard)
            : this(null)
        {
            Key = key;
            DebugInfo = debugInfo;
            Languages = new HashSet<Language>(languages);
            FilenameWildcard = filenameWildcard;
        }

        public PatternRootUst()
            : this(null)
        {
        }

        public PatternRootUst(SourceCodeFile sourceCodeFile)
            : base(sourceCodeFile, Language.Universal)
        {
        }

        public override Ust[] GetChildren() => Nodes;

        public override string ToString()
        {
            return (!string.IsNullOrEmpty(DebugInfo) ? DebugInfo : Key) ?? "";
        }

        public List<MatchingResult> Match(Ust ust)
        {
            var context = new MatchingContext(this) { Logger = Logger };
            var results = new List<MatchingResult>();

            if (ust is RootUst rootUst)
            {
                var patternUst = (PatternBase)Node;

                if (patternUst is PatternCommentRegex ||
                   (patternUst is PatternOr patternOr && patternOr.Patterns.Any(v => v is PatternCommentRegex)))
                {
                    foreach (CommentLiteral commentNode in rootUst.Comments)
                    {
                        MatchAndAddResult(patternUst, commentNode, context, results);
                    }
                }
                else
                {
                    TraverseChildren(patternUst, rootUst, context, results);
                }
            }

            return results;
        }

        private static void TraverseChildren(PatternBase patternUst, Ust ust, MatchingContext context, List<MatchingResult> results)
        {
            MatchAndAddResult(patternUst, ust, context, results);

            if (ust != null)
            {
                foreach (Ust child in ust.Children)
                {
                    TraverseChildren(patternUst, child, context, results);
                }
            }
        }

        private static void MatchAndAddResult(
            PatternBase patternUst, Ust ust, MatchingContext context, List<MatchingResult> results)
        {
            if (patternUst.Match(ust, context).Success)
            {
                if (context.Locations.Count == 0)
                {
                    context.Locations.Add(ust.TextSpan);
                }
                var matching = new MatchingResult(ust.Root, context.PatternUst, context.Locations);
                results.Add(matching);
                context.Logger.LogInfo(matching);
            }
            context.Locations.Clear();
        }
    }
}
