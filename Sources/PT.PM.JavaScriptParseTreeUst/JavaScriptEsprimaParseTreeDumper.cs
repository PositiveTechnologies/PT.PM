using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common;
using System.Collections.Generic;

namespace PT.PM.JavaScriptParseTreeUst
{
    public class JavaScriptEsprimaParseTreeDumper : ParseTreeDumper
    {
        public override void DumpTokens(ParseTree parseTree)
        {
        } 

        public override void DumpTree(ParseTree parseTree)
        {
            var esprimaParseTree = (JavaScriptEsprimaParseTree)parseTree;

            var serializerSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = IndentSize != -1 ? Formatting.Indented : Formatting.None,
                Converters = new List<JsonConverter>
                {
                    new EsprimaJsonConverter(parseTree.SourceCodeFile)
                    {
                        IncludeTextSpans = IncludeTextSpans,
                        IsLineColumn = IsLineColumn
                    },
                    new StringEnumConverter()
                }
            };

            string result = JsonConvert.SerializeObject(esprimaParseTree.SyntaxTree, serializerSettings);
            Dump(result, parseTree.SourceCodeFile, false);
        }
    }
}
