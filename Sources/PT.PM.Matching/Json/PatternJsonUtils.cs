using PT.PM.Matching.Patterns;
using System;

namespace PT.PM.Matching.Json
{
    public static class PatternJsonUtils
    {
        public static bool CanConvert(this Type objectType)
        {
            return objectType == typeof(PatternUst) ||
                objectType.IsSubclassOf(typeof(PatternUst)) ||
                objectType == typeof(PatternRoot);
        }
    }
}
