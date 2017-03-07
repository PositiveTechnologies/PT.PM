using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common.Nodes
{
    public abstract class Node
    {
        [JsonIgnore]
        public FileNode FileNode { get; set; }

        public Node()
        {
        }
    }
}
