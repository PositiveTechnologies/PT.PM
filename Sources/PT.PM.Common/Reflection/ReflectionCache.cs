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
        private static readonly Dictionary<Type, PropertyInfo[]> typeSerializableProperties;
        private static readonly Type[] ustTypes;

        static ReflectionCache()
        {
            registeredTypes = new HashSet<Type>();
            kindClassType = new Dictionary<string, Type>();
            typeSerializableProperties = new Dictionary<Type, PropertyInfo[]>();

            Assembly ustAssembly = typeof(Ust).Assembly;
            var types = (UstType[]) Enum.GetValues(typeof(UstType));
            ustTypes = new Type[types.Length];
            var assemblyTypes = ustAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Ust))).ToList();

            foreach (UstType type in types)
            {
                string typeStr = type.ToString();
                Type ustType = null;
                foreach (Type assemblyType in assemblyTypes)
                {
                    if (assemblyType.Name.Equals(typeStr))
                    {
                        ustType = assemblyType;
                        break;
                    }
                }

                if (ustType == null)
                {
                    throw new InvalidDataContractException($"NodeType {type} does not have corresponding Ust");
                }

                ustTypes[(int) type] = ustType;
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
                        const int startIndex = 1;
                        var serializableProperties = new PropertyInfo[properties.Length];
                        int actualLength = 0;

                        foreach (PropertyInfo property in properties)
                        {
                            if (property.CanRead && property.CanWrite &&
                                (property.GetCustomAttribute<JsonIgnoreAttribute>() == null ||
                                 property.Name == nameof(Ust.TextSpan)))
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

                        for (int i = startIndex; i < serializableProperties.Length; i++)
                        {
                            var keyAttribute = serializableProperties[i]?.GetCustomAttribute<KeyAttribute>();
                            if (i - startIndex < actualLength)
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
                            Array.Copy(serializableProperties, startIndex, newArray, 0, actualLength);
                            serializableProperties = newArray;
                        }

                        lock (typeSerializableProperties)
                        {
                            typeSerializableProperties.Add(type, serializableProperties);
                        }
                    }
                    else
                    {
                        var serializableProperties = new List<PropertyInfo>(properties.Length);

                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetCustomAttribute<JsonIgnoreAttribute>() == null ||
                                property.Name == nameof(Ust.TextSpan))
                            {
                                serializableProperties.Add(property);
                            }
                        }

                        lock (typeSerializableProperties)
                        {
                            typeSerializableProperties.Add(type, serializableProperties.ToArray());
                        }
                    }
                }
            }
        }

        public static bool TryGetClassType(string kind, out Type type) =>
            kindClassType.TryGetValue(kind.ToLowerInvariant(), out type);

        internal static PropertyInfo[] GetSerializableProperties(this Type ustType)
            => typeSerializableProperties[ustType];

        internal static Ust CreateUst(byte nodeType) => (Ust)Activator.CreateInstance(ustTypes[nodeType]);
    }
}
