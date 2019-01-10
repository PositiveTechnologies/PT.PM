using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;

namespace PT.PM.Common.Json
{
    public class CodeFileJsonConverter : JsonConverter<CodeFile>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeCode { get; set; } = true;

        public bool ExcludeDefaults { get; set; } = true;

        public CodeFile JsonFile { get; set; } = CodeFile.Empty;

        public Action<CodeFile> SetCurrentCodeFileAction { get; set; }

        public Action<(IFile, TimeSpan)> ReadCodeFileAction { get; set; }

        public override void WriteJson(JsonWriter writer, CodeFile sourceCodeFile, JsonSerializer serializer)
        {
            if (ExcludeDefaults && sourceCodeFile.IsEmpty)
            {
                return;
            }

            JObject jObject = new JObject();

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.RootPath))
                jObject.Add(nameof(sourceCodeFile.RootPath), sourceCodeFile.RootPath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.RelativePath))
                jObject.Add(nameof(sourceCodeFile.RelativePath), sourceCodeFile.RelativePath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.Name))
                jObject.Add(nameof(sourceCodeFile.Name), sourceCodeFile.Name);

            if (IncludeCode &&
                (!ExcludeDefaults || !string.IsNullOrEmpty(sourceCodeFile.Data)))
            {
                jObject.Add(nameof(CodeFile.Data), sourceCodeFile.Data);
            }

            jObject.WriteTo(writer);
        }

        public override CodeFile ReadJson(JsonReader reader, Type objectType, CodeFile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            string code = (string)obj[nameof(CodeFile.Data)];
            string rootPath = (string)obj[nameof(CodeFile.RootPath)] ?? "";
            string relativePath = (string)obj[nameof(CodeFile.RelativePath)] ?? "";
            string name = (string)obj[nameof(CodeFile.Name)] ?? "";

            var stopwatch = Stopwatch.StartNew();
            if (code == null)
            {
                string fullName = Path.Combine(rootPath, relativePath, name);
                try
                {
                    code = fullName != "" ? FileExt.ReadAllText(fullName) : "";
                }
                catch
                {
                    code = "";
                    Logger.LogError(JsonFile, obj, $"File {fullName} can not be read");
                }
            }
            stopwatch.Stop();

            CodeFile result = new CodeFile(code)
            {
                RootPath = rootPath,
                RelativePath = relativePath,
                Name = name
            };

            ReadCodeFileAction?.Invoke((result, stopwatch.Elapsed));
            SetCurrentCodeFileAction?.Invoke(result);

            return result;
        }
    }
}
