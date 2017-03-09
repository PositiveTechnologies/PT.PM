using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PT.PM.Common.Ust;
using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace PT.PM.Common
{
    public class UstJsonConverter : JsonConverter
    {
        private static ICollection<Assembly> cachedAssemblies = new HashSet<Assembly>();
        private static IDictionary<NodeType, Type> nodeTypes = new ConcurrentDictionary<NodeType, Type>();
        private static readonly object assemblyLock = new object();
        private FileNode currentFileNode;

        public UstJsonConverter(params Type[] ustNodeAssemblyTypes)
        {
            var types = ustNodeAssemblyTypes == null || ustNodeAssemblyTypes.Length == 0
                ? new[] { typeof(UstNode) } : ustNodeAssemblyTypes;

            IEnumerable<KeyValuePair<NodeType, Type>> keyValuePairs = types.SelectMany(type =>
            {
                Assembly assembly = Assembly.GetAssembly(type);
                lock (assemblyLock)
                {
                    if (cachedAssemblies.Contains(assembly))
                    {
                        return Enumerable.Empty<Type>();
                    }
                    else
                    {
                        cachedAssemblies.Add(assembly);
                        return assembly.GetTypes();
                    }
                }
            })
            .Where(t => t.IsSubclassOf(typeof(UstNode)) && !t.IsAbstract)
            .Select(t => new KeyValuePair<NodeType, Type>((NodeType)Enum.Parse(typeof(NodeType), t.Name), t));

            foreach (var keyValuePair in keyValuePairs)
            {
                nodeTypes[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UstNode) || objectType.IsSubclassOf(typeof(UstNode)) ||
                objectType == typeof(Ust.Ust);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);

                object target = null;
                if (objectType == typeof(UstNode) || objectType.IsSubclassOf(typeof(UstNode)))
                {
                    var obj = jObject[nameof(NodeType)];
                    var nodeType = (NodeType) Enum.Parse(typeof (NodeType), obj.ToString());
                    target = Activator.CreateInstance(nodeTypes[nodeType]);

                    if (objectType == typeof(FileNode))
                    {
                        currentFileNode = (FileNode)target;
                    }
                    else
                    {
                        ((UstNode)target).FileNode = currentFileNode;
                    }
                }
                else if (objectType == typeof(Ust.Ust))
                {
                    var obj = jObject[nameof(Type)];
                    var astType = (UstType) Enum.Parse(typeof(UstType), obj.ToString());
                    if (astType == UstType.Common)
                    {
                        target = new MostCommonUst();
                    }
                    else
                    {
                        target = new MostDetailUst();
                    }
                }
                else
                {
                    throw new FormatException("Invalid JSON");
                }

                serializer.Populate(jObject.CreateReader(), target);
                return target;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException($"Do not use {nameof(UstJsonConverter)} for serialization.");
        }
    }
}
