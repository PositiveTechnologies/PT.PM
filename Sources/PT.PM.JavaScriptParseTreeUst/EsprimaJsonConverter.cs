using Esprima;
using Esprima.Ast;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common;
using System;
using System.Reflection;
using PT.PM.Common.Files;

namespace PT.PM.JavaScriptParseTreeUst
{
    internal class EsprimaJsonConverter : JsonConverter<INode>
    {
        public TextFile SourceFile { get; }

        public bool IncludeTextSpans { get; set; }

        public bool IsLineColumn { get; set; }

        public EsprimaJsonConverter(TextFile sourceFile)
        {
            SourceFile = sourceFile ?? throw new ArgumentNullException(nameof(sourceFile));
        }

        public override INode ReadJson(JsonReader reader, Type objectType, INode existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, INode value, JsonSerializer serializer)
        {
            MemberInfo[] members = value.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);

            var jObject = new JObject();

            foreach (MemberInfo member in members)
            {
                Type memberType;
                string memberName = member.Name;
                object memberValue = null;

                if (member is FieldInfo fieldInfo)
                {
                    memberType = fieldInfo.FieldType;
                    memberValue = fieldInfo.GetValue(value);
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    memberType = propertyInfo.PropertyType;
                    memberValue = propertyInfo.GetValue(value);
                }
                else
                {
                    continue;
                }

                JToken resultToken;

                if (memberName == nameof(INode.Range) && memberType == typeof(int[]))
                {
                    if (!IncludeTextSpans)
                    {
                        continue;
                    }

                    var range = (int[])memberValue;
                    var textSpan = TextSpan.FromBounds(range[0], range[1]);

                    resultToken = IsLineColumn
                         ? SourceFile.GetLineColumnTextSpan(textSpan).ToString()
                         : textSpan.ToString();
                }
                else if (memberName == nameof(INode.Location) && memberType == typeof(Location))
                {
                    continue;
                }
                else
                {
                    object propValue = memberValue;
                    resultToken = propValue != null ? JToken.FromObject(propValue, serializer) : null;
                }

                if (resultToken != null)
                {
                    jObject.Add(member.Name, resultToken);
                }
            }

            jObject.WriteTo(writer);
        }
    }
}
