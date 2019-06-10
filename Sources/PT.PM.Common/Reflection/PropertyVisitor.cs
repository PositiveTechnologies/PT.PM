using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PT.PM.Common.Reflection
{
    public class PropertyVisitor<TInput, TOutput>
    {
        public HashSet<string> IgnoredProperties = new HashSet<string>();

        public virtual bool Clone => false;

        public TOutput VisitProperties(TInput node, Func<TInput, TOutput> visit)
        {
            if (node == null)
            {
                return default;
            }

            Type type = node.GetType();
            PropertyInfo[] properties = type.GetSerializableProperties();

            TOutput result = Clone ? (TOutput)Activator.CreateInstance(type) : default;
            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;
                if (IgnoredProperties.Contains(prop.Name))
                {
                    continue;
                }

                if (propType.IsValueType || propType == typeof(string) || propType == typeof(Regex))
                {
                    if (Clone)
                    {
                        prop.SetValue(result, prop.GetValue(node));
                    }
                }
                else if (typeof(TInput).IsAssignableFrom(propType))
                {
                    TOutput setValue = visit((TInput)prop.GetValue(node));
                    if (Clone)
                    {
                        prop.SetValue(result, setValue);
                    }
                }
                else if (propType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    Type itemType = propType.GetGenericArguments()[0];
                    IList destCollection = null;
                    var collection = prop.GetValue(node);
                    if (collection != null)
                    {
                        IEnumerable<object> sourceCollection;
                        if (collection is IEnumerable<object> refCollection)
                        {
                            sourceCollection = refCollection;
                        }
                        else
                        {
                            sourceCollection = ((IEnumerable)collection).Cast<object>();
                        }

                        destCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                        foreach (object item in sourceCollection)
                        {
                            object destItem = item is TInput sourceItem
                                ? visit(sourceItem)
                                : item;
                            if (Clone)
                            {
                                destCollection.Add(destItem);
                            }
                        }
                    }
                    if (Clone)
                    {
                        prop.SetValue(result, destCollection);
                    }
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
