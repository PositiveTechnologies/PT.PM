using PT.PM.Common.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace PT.PM.Common
{
    public class TextSpanResolver : DefaultContractResolver
    {
        public bool Ignore { get; set; }

        public TextSpanResolver()
        {
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyName == nameof(TextSpan))
            {
                if (Ignore)
                {
                    property.ShouldSerialize =
                        instance =>
                        {
                            return false;
                        };
                }
            }

            return property;
        }
    }
}
