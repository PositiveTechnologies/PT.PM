using PT.PM.Common.Nodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common
{
    public static class ReflectionCache
    {
        private static ConcurrentDictionary<Type, PropertyInfo[]> astNodeProperties
            = new ConcurrentDictionary<Type, PropertyInfo[]>();
        
        public static PropertyInfo[] GetClassProperties(Type objectType)
        {
            PropertyInfo[] result = null;
            if (!astNodeProperties.TryGetValue(objectType, out result))
            {
                result = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanWrite && prop.CanRead).ToArray();
                astNodeProperties[objectType] = result;
            }
            return result;
        }
    }
}
