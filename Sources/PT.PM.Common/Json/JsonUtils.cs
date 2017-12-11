using Newtonsoft.Json.Linq;
using System;

namespace PT.PM.Common.Json
{
    public static class JsonUtils
    {
        public static JToken GetValueIgnoreCase(this JObject jObject, string propertyName)
        {
            return jObject.GetValue(propertyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
