using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace PT.PM.Common.Json
{
    public static class JsonUtils
    {
        public static JToken GetValueIgnoreCase(this JObject jObject, string propertyName)
        {
            return jObject.GetValue(propertyName, StringComparison.OrdinalIgnoreCase);
        }

        public static JToken GetValueIgnoreCase(this JToken jToken, string propertyName)
        {
            return ((jToken.Children()
                .FirstOrDefault(token =>
                 token is JProperty jProperty && jProperty.Name.EqualsIgnoreCase(propertyName))) as JProperty)
                 ?.Value;
        }
    }
}
