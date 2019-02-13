using PT.PM.Matching.Patterns;
using System;
using PT.PM.Common.Reflection;

namespace PT.PM.Matching.Json
{
    public static class PatternJsonUtils
    {
        static PatternJsonUtils()
        {
            ReflectionCache.RegisterTypes(typeof(PatternUst), false);
        }

        public static bool CanConvert(this Type objectType)
        {
            return objectType == typeof(PatternUst) ||
                objectType.IsSubclassOf(typeof(PatternUst)) ||
                objectType == typeof(PatternRoot);
        }
    }
}
