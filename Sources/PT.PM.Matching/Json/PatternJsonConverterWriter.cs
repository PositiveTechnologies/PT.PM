using PT.PM.Common.Json;
using System;

namespace PT.PM.Matching.Json
{
    public class PatternJsonConverterWriter : UstJsonConverterWriter
    {
        public override bool CanConvert(Type objectType) => objectType.CanConvert();
    }
}
