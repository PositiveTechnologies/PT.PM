using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common;
using System.Collections.Generic;

namespace PT.PM.CSharpParseTreeUst
{
    public class RoslynDumper : ParseTreeDumper
    {
        public override void DumpTokens(ParseTree parseTree)
        {
        }

        public override void DumpTree(ParseTree parseTree)
        {
            var roslynParseTree = (CSharpRoslynParseTree)parseTree;

            var serializerSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = IndentSize != -1 ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };

            string result = JsonConvert.SerializeObject(roslynParseTree.SyntaxTree.GetRoot(), serializerSettings);
            Dump(result, parseTree.SourceFile, false);
        }
    }
}
