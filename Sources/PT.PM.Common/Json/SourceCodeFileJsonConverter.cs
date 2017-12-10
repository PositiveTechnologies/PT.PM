using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PT.PM.Common.Json
{
    public class SourceCodeFileJsonConverter : JsonConverter
    {
        public bool IncludeCode { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CodeFile);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            var sourceCodeFile = (CodeFile)value;

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.RootPath))
                jObject.Add(nameof(sourceCodeFile.RootPath), sourceCodeFile.RootPath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.RelativePath))
                jObject.Add(nameof(sourceCodeFile.RelativePath), sourceCodeFile.RelativePath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.Name))
                jObject.Add(nameof(sourceCodeFile.Name), sourceCodeFile.Name);

            if (IncludeCode &&
                (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.Code)))
            {
                jObject.Add(nameof(sourceCodeFile.Code), sourceCodeFile.Code);
            }

            jObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            CodeFile result = new CodeFile((string)obj[nameof(CodeFile.Code)] ?? "")
            {
                RootPath = (string)obj[nameof(CodeFile.RootPath)] ?? "",
                RelativePath = (string)obj[nameof(CodeFile.RelativePath)] ?? "",
                Name = (string)obj[nameof(CodeFile.Name)] ?? "",
            };

            return result;
        }
    }
}
