using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PT.PM.Common.Reflection
{
    public class PropertyComparer<T>
    {
        public HashSet<string> IgnoredProperties = new HashSet<string>();

        public int Compare(object node, object other)
        {
            if (other == null)
            {
                return node == null ? 0 : 1;
            }
            
            if (node == null)
            {
                return -1;
            }

            Type type1 = node.GetType();
            Type type2 = other.GetType();

            if (!(type1.GetInterfaces().Contains(typeof(IList)) && type2.GetInterfaces().Contains(typeof(IList))))
            {
                if (type1 != type2)
                {
                    return type1.Name.CompareTo(type2.Name);
                }
            }

            int result;
            var type1Interfaces = type1.GetInterfaces();
            if (type1Interfaces.Contains(typeof(IComparable)))
            {
                var comparable1 = (IComparable)node;
                var comparable2 = (IComparable)other;
                result = comparable1.CompareTo(comparable2);
                if (result != 0)
                {
                    return result;
                }
            }
            else if (type1 == typeof(Regex))
            {
                var comparable1 = (Regex)node;
                var comparable2 = (Regex)other;
                result = comparable1.ToString().CompareTo(comparable2.ToString());
                if (result != 0)
                {
                    return result;
                }
            }
            else if (type1.IsSubclassOf(typeof(T)) || type1 == typeof(T))
            {
                PropertyInfo[] properties1 = type1.GetReadWriteClassProperties();
                PropertyInfo[] properties2 = type2.GetReadWriteClassProperties();

                if (properties1.Length != properties2.Length)
                {
                    return properties1.Length - properties2.Length;
                }

                for (int i = 0; i < properties1.Length; i++)
                {
                    if (!IgnoredProperties.Contains(properties1[i].Name))
                    {
                        result = Compare(properties1[i].GetValue(node), properties2[i].GetValue(other));
                        if (result != 0)
                        {
                            return result;
                        }
                    }
                }
            }
            else if (type1Interfaces.Contains(typeof(IList)))
            {
                var node1List = (IList)node;
                var node2List = (IList)other;

                if (node1List.Count != node2List.Count)
                {
                    return node1List.Count - node2List.Count;
                }

                for (int i = 0; i < node1List.Count; i++)
                {
                    result = Compare(node1List[i], node2List[i]);
                    if (result != 0)
                    {
                        return result;
                    }
                }
            }
            else
            {
                throw new NotImplementedException($"Type \"{type1}\" comparison is not implemented via reflection");
            }

            return 0;
        }
    }
}
