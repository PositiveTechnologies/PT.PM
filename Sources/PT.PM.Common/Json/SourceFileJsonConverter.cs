using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PT.PM.Common.Files;
using PT.PM.Common.Utils;

namespace PT.PM.Common.Json
{
    public class SourceFileJsonConverter : JsonConverter<TextFile>, ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeCode { get; set; } = true;

        public bool ExcludeDefaults { get; set; } = true;

        public TextFile SerializedFile { get; set; } = TextFile.Empty;

        public Action<TextFile> SetCurrentSourceFileAction { get; set; }

        public Action<(IFile, TimeSpan)> ReadSourceFileAction { get; set; }

        public override void WriteJson(JsonWriter writer, TextFile sourceFile, JsonSerializer serializer)
        {
            if (ExcludeDefaults && sourceFile.IsEmpty)
            {
                return;
            }

            JObject jObject = new JObject();

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceFile.RootPath))
                jObject.Add(nameof(sourceFile.RootPath), sourceFile.RootPath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceFile.RelativePath))
                jObject.Add(nameof(sourceFile.RelativePath), sourceFile.RelativePath);

            if (!ExcludeDefaults || !string.IsNullOrEmpty(sourceFile.Name))
                jObject.Add(nameof(sourceFile.Name), sourceFile.Name);

            if (IncludeCode &&
                (!ExcludeDefaults || !string.IsNullOrEmpty(sourceFile.Data)))
            {
                jObject.Add(nameof(IFile.Content), sourceFile.Data);
            }

            jObject.WriteTo(writer);
        }

        public override TextFile ReadJson(JsonReader reader, Type objectType, TextFile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            string code = (string)obj[nameof(IFile.Content)];
            string rootPath = (string)obj[nameof(IFile.RootPath)] ?? "";
            string relativePath = (string)obj[nameof(IFile.RelativePath)] ?? "";
            string name = (string)obj[nameof(IFile.Name)] ?? "";

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
                    Logger.LogError(SerializedFile, obj, $"File {fullName} can not be read");
                }
            }
            stopwatch.Stop();

            var result = new TextFile(code)
            {
                RootPath = rootPath,
                RelativePath = relativePath,
                Name = name
            };

            ReadSourceFileAction?.Invoke((result, stopwatch.Elapsed));
            SetCurrentSourceFileAction?.Invoke(result);

            return result;
        }
    }
}
