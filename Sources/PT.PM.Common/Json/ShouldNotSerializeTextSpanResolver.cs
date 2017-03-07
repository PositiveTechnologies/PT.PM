using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PT.PM.Common
{
    public class ShouldNotSerializeTextSpanResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName == nameof(TextSpan))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        return false;
                    };
            }

            return property;
        }
    }
}
