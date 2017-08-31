using System.Collections.Generic;
using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using System;
using PT.PM.Common;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;

namespace PT.PM.Patterns.Nodes
{
    public class PatternRootNode : RootNode
    {
        private HashSet<Language> languages = new HashSet<Language>();
        private Regex pathWildcardRegex;

        public override NodeType NodeType => NodeType.PatternRootNode;

        public string Key { get; set; }

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

        [JsonConverter(typeof(StringEnumConverter))]
        public UstNodeSerializationFormat DataFormat { get; set; } = UstNodeSerializationFormat.Json;

        [JsonIgnore]
        public string DebugInfo { get; set; }

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

        [JsonIgnore]
        public List<PatternVarDef> Vars { get; set; } = new List<PatternVarDef>();

        public PatternRootNode(string key, string debugInfo, IEnumerable<Language> languages, string filenameWildcard)
            : this(null)
        {
            Key = key;
            DebugInfo = debugInfo;
            Languages = new HashSet<Language>(languages);
            FilenameWildcard = filenameWildcard;
        }

        public PatternRootNode()
            : this(null)
        {
        }

        public PatternRootNode(SourceCodeFile sourceCodeFile)
            : base(sourceCodeFile, Language.Universal)
        {
        }

        public override UstNode[] GetChildren()
        {
            var result = new List<UstNode>();
            result.AddRange(Nodes);
            return result.ToArray();
        }

        public override int CompareTo(UstNode other)
        {
            if (other is PatternRootNode otherRoot)
            {
                return UstNodeHelper.CompareTo<UstNode>(Nodes, otherRoot.Nodes);
            }

            return 1;
        }

        public override string ToString()
        {
            return DebugInfo;
        }
    }
}
