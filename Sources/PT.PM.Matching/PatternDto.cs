using PT.PM.Common;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class PatternDto
    {
        public string Name { get; set; } = "";

        public string Key { get; set; } = "";

        public HashSet<string> Languages { get; set; } = new HashSet<string>();

        public string DataFormat { get; set; } = "";

        public string Value { get; set; } = "";

        public string CweId { get; set; } = "";

        public string Description { get; set; } = "";

        public string FilenameWildcard { get; set; } = "";

        public PatternDto()
        {
        }

        public override string ToString()
        {
            return $"{Name} ({(string.Join(", ", Languages))})";
        }
    }
}
