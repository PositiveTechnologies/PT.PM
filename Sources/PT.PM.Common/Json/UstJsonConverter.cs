using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace PT.PM.Common
{
    public class UstJsonConverter : JsonConverter
    {
        private static ICollection<Assembly> cachedAssemblies = new HashSet<Assembly>();
        private static IDictionary<UstKind, Type> nodeTypes = new ConcurrentDictionary<UstKind, Type>();
        private static readonly object assemblyLock = new object();

        public UstJsonConverter(params Type[] ustNodeAssemblyTypes)
        {
            var types = ustNodeAssemblyTypes == null || ustNodeAssemblyTypes.Length == 0
                ? new[] { typeof(Ust) } : ustNodeAssemblyTypes;

            IEnumerable<KeyValuePair<UstKind, Type>> keyValuePairs = types.SelectMany(type =>
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
            .Where(t => t.IsSubclassOf(typeof(Ust)) && !t.IsAbstract)
            .Select(t => new KeyValuePair<UstKind, Type>((UstKind)Enum.Parse(typeof(UstKind), t.Name), t));

            foreach (var keyValuePair in keyValuePairs)
            {
                nodeTypes[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Null)
            {
                JObject jObject = JObject.Load(reader);

                object target = null;
                if (objectType == typeof(Ust) || objectType.IsSubclassOf(typeof(Ust)))
                {
                    var obj = jObject[nameof(Ust.Kind)];
                    var nodeType = (UstKind)Enum.Parse(typeof (UstKind), obj.ToString());
                    target = Activator.CreateInstance(nodeTypes[nodeType]);
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
