using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MessagePack;
using Newtonsoft.Json;

namespace PT.PM.Common.Reflection
{
    public static class ReflectionCache
    {
        private static readonly HashSet<Type> registeredTypes;
        private static readonly Dictionary<string, Type> kindClassType;
        private static readonly Dictionary<Type, (byte Type, PropertyInfo[] Properties)> typeSerializableProperties;
        private static readonly Type[] ustTypes;

        static ReflectionCache()
        {
            registeredTypes = new HashSet<Type>();
            kindClassType = new Dictionary<string, Type>();
            typeSerializableProperties = new Dictionary<Type, (byte NodeType, PropertyInfo[] Properties)>();

            Assembly ustAssembly = typeof(Ust).Assembly;
            var nodeTypes = (NodeType[]) Enum.GetValues(typeof(NodeType));
            ustTypes = new Type[nodeTypes.Length];
            var assemblyTypes = ustAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Ust))).ToList();

            foreach (NodeType nodeType in nodeTypes)
            {
                string nodeTypeStr = nodeType.ToString();
                Type ustType = null;
                foreach (Type assemblyType in assemblyTypes)
                {
                    if (assemblyType.Name.Equals(nodeTypeStr))
                    {
                        ustType = assemblyType;
                    }
                }

                if (ustType == null)
                {
                    throw new InvalidDataContractException($"NodeType {nodeType} does not have corresponding Ust");
                }

                ustTypes[(int) nodeType] = ustType;
            }

            RegisterTypes(typeof(Ust), true);
        }

        public static void RegisterTypes(Type baseType, bool messagePack)
        {
            lock (registeredTypes)
            {
                if (registeredTypes.Contains(baseType))
                {
                    return;
                }

                registeredTypes.Add(baseType);
            }

            foreach (Type type in baseType.Assembly.GetTypes())
            {
                if (type.IsSubclassOf(baseType) && !type.IsAbstract)
                {
                    lock (kindClassType)
                    {
                        kindClassType.Add(type.Name.ToLowerInvariant(), type);
                    }

                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    // TODO: cover with MessagePack attributes PatternUst types?
                    if (messagePack)
                    {
                        var serializableProperties = new PropertyInfo[properties.Length];
                        int actualLength = 0;

                        foreach (PropertyInfo property in properties)
                        {
                            if (property.CanRead && property.CanWrite &&
                               (property.GetCustomAttribute<JsonIgnoreAttribute>() == null ||
                                property.Name == nameof(Ust.TextSpans)))
                            {
                                KeyAttribute keyAttribute = property.GetCustomAttribute<KeyAttribute>();
                                if (keyAttribute == null)
                                {
                                    throw new InvalidDataContractException(
                                        $"Property {type.Name}.{property.Name} should be serialized both by Json and MessagePack");
                                }

                                serializableProperties[keyAttribute.IntKey.Value] = property;
                                actualLength++;
                            }
                        }

                        for (int i = 0; i < serializableProperties.Length; i++)
                        {
                            var keyAttribute = serializableProperties[i]?.GetCustomAttribute<KeyAttribute>();
                            if (i < actualLength)
                            {
                                if (keyAttribute == null)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        $"Keys of {type.Name} should be thightly packed. {i} offset is not used");
                                }
                            }
                            else
                            {
                                if (keyAttribute != null)
                                {
                                    throw new ArgumentOutOfRangeException(
                                        $"Keys of {type.Name} should be thightly packed.");
                                }
                            }
                        }

                        if (actualLength < serializableProperties.Length)
                        {
                            var newArray = new PropertyInfo[actualLength];
                            Array.Copy(serializableProperties, newArray, actualLength);
                            serializableProperties = newArray;
                        }

                        lock (typeSerializableProperties)
                        {
                            if (Enum.TryParse(type.Name, out NodeType nodeType))
                            {
                                typeSerializableProperties.Add(type, ((byte)nodeType, serializableProperties));
                            }
                            else
                            {
                                throw new InvalidDataContractException($"Type {type.Name} does not have corresponding {nameof(NodeType)}");
                            }
                        }
                    }
                    else
                    {
                        var serializableProperties = new List<PropertyInfo>(properties.Length);

                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                            {
                                serializableProperties.Add(property);
                            }
                        }

                        lock (typeSerializableProperties)
                        {
                            typeSerializableProperties.Add(type, (0, serializableProperties.ToArray()));
                        }
                    }
                }
            }
        }

        public static bool TryGetClassType(string kind, out Type type) =>
            kindClassType.TryGetValue(kind.ToLowerInvariant(), out type);

        internal static PropertyInfo[] GetSerializableProperties(this Type ustType, out byte type)
        {
            var typeProperties = typeSerializableProperties[ustType];
            type = typeProperties.Type;
            return typeProperties.Properties;
        }

        internal static Ust CreateUst(byte nodeType) => (Ust)Activator.CreateInstance(ustTypes[nodeType]);
    }
}
