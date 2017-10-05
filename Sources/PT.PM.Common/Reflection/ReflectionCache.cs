using PT.PM.Common.Nodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.Common.Reflection
{
    public static class ReflectionCache
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> ustNodeProperties
            = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static Lazy<Dictionary<string, Type>> UstKindFullClassName =
            new Lazy<Dictionary<string, Type>>(() =>
            {
                var result = new Dictionary<string, Type>();
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.IsActual())
                    {
                        foreach (Type type in assembly.GetTypes()
                            .Where(myType => myType.IsClass && !myType.IsAbstract &&
                            myType.GetInterfaces().Contains(typeof(IUst))))
                        {
                            result.Add(type.Name, type);
                        }
                    }
                }
                return result;
            });

        public static PropertyInfo[] GetClassProperties(this Type objectType)
        {
            PropertyInfo[] result = null;
            if (!ustNodeProperties.TryGetValue(objectType, out result))
            {
                result = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanWrite && prop.CanRead).ToArray();
                ustNodeProperties.TryAdd(objectType, result);
            }
            return result;
        }
    }
}
