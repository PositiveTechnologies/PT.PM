using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly string[] propertyNamesToExclude;

        public DynamicContractResolver(params string[] propertyNameToExclude)
        {
            IgnoreSerializableInterface = true;
            this.propertyNamesToExclude = propertyNameToExclude;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            
            properties = properties.Where(p => propertyNamesToExclude
                .All(p2 => string.Compare(p.PropertyName, p2, true) != 0)).ToList();

            return properties;
        }
    }
}
