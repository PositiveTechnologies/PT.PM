using PT.PM.Common.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PT.PM.Common.Reflection
{
    public class PropertyEnumerator<T>
    {
        public HashSet<string> IgnoredProperties = new HashSet<string>();

        public T Clone(T node, Func<T, T> visit)
        {
            if (node == null)
            {
                return default(T);
            }

            Type type = node.GetType();
            PropertyInfo[] properties = ReflectionCache.GetClassProperties(type);

            var result = (T)Activator.CreateInstance(type);
            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;
                if (propType.IsValueType || propType == typeof(string) || propType == typeof(Regex))
                {
                    prop.SetValue(result, prop.GetValue(node));
                }
                else if (IgnoredProperties.Contains(prop.Name))
                {
                    continue;
                }
                else if (typeof(T).IsAssignableFrom(propType))
                {
                    var getValue = (T)prop.GetValue(node);
                    if (getValue != null)
                    {
                        T setValue = visit(getValue);
                        prop.SetValue(result, setValue);
                    }
                }
                else if (propType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    Type itemType = propType.GetGenericArguments()[0];
                    var sourceCollection = (IEnumerable<object>)prop.GetValue(node);
                    IList destCollection = null;
                    if (sourceCollection != null)
                    {
                        destCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                        foreach (object item in sourceCollection)
                        {
                            if (item is T ustNodeItem)
                            {
                                var destUstNodeItem = visit(ustNodeItem);
                                destCollection.Add(destUstNodeItem);
                            }
                            else
                            {
                                destCollection.Add(item);
                            }
                        }
                    }
                    prop.SetValue(result, destCollection);
                }
                else
                {
                    throw new NotImplementedException($"Property \"{prop}\" processing is not implemented via reflection");
                }
            }

            return result;
        }
    }
}
