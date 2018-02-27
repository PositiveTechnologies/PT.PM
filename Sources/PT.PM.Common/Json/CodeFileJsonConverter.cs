using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PT.PM.Common.Json
{
    public class CodeFileJsonConverter : JsonConverter, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeCode { get; set; } = true;

        public bool ExcludeDefaults { get; set; } = true;

        public CodeFile CodeFile { get; private set; } = CodeFile.Empty;

        public CodeFile JsonFile { get; set; } = CodeFile.Empty;

        public TextSpanJsonConverter TextSpanJsonConverter { get; set; }

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

            string code = (string)obj.GetValueIgnoreCase(nameof(CodeFile.Code));
            string rootPath = (string)obj.GetValueIgnoreCase(nameof(CodeFile.RootPath)) ?? "";
            string relativePath = (string)obj.GetValueIgnoreCase(nameof(CodeFile.RelativePath)) ?? "";
            string name = (string)obj.GetValueIgnoreCase(nameof(CodeFile.Name)) ?? "";

            if (code == null)
            {
                string fullName = Path.Combine(rootPath, relativePath, name);
                try
                {
                    code = fullName != "" ? File.ReadAllText(fullName) : "";
                }
                catch
                {
                    code = "";
                    Logger.LogError(JsonFile, obj, $"File {fullName} can not be read");
                }
            }

            CodeFile result = new CodeFile(code)
            {
                RootPath = rootPath,
                RelativePath = relativePath,
                Name = name
            };

            CodeFile = result;
            if (TextSpanJsonConverter != null)
            {
                TextSpanJsonConverter.CodeFile = result;
            }
            return result;
        }
    }
}
