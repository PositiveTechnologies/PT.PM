using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Json
{
    public abstract class JsonBaseSerializer<T> : ILoggable
    {
        private static readonly JsonConverter stringEnumConverter = new StringEnumConverter();

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public bool IncludeTextSpans { get; set; } = true;

        public bool Indented { get; set; } = false;

        public bool ExcludeDefaults { get; set; } = true;

        public bool IncludeCode { get; set; } = true;

        public bool ShortTextSpans { get; set; } = true;

        public bool LineColumnTextSpans { get; set; } = false;

        public string EmptyTextSpanFormat { get; set; } = null;

        public CodeFile JsonFile { get; protected set; } = CodeFile.Empty;

        protected abstract JsonConverterBase CreateConverterBase(CodeFile jsonFile);

        public virtual T Deserialize(CodeFile jsonFile)
        {
            JsonFile = jsonFile;
            JsonSerializerSettings jsonSettings = PrepareSettings();
            return JsonConvert.DeserializeObject<T>(jsonFile.Code, jsonSettings);
        }

        public virtual string Serialize(T node)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings((node as Ust).GetCodeFile());
            return JsonConvert.SerializeObject(node, jsonSettings);
        }

        public string Serialize(IEnumerable<T> nodes)
        {
            JsonSerializerSettings jsonSettings = PrepareSettings((nodes.FirstOrDefault() as Ust).GetCodeFile());
            return JsonConvert.SerializeObject(nodes, jsonSettings);
        }

        public JsonSerializerSettings PrepareSettings(CodeFile codeFile = null)
        {
            JsonConverterBase jsonConverterBase = CreateConverterBase(JsonFile);
            jsonConverterBase.IncludeTextSpans = IncludeTextSpans;
            jsonConverterBase.ExcludeDefaults = ExcludeDefaults;
            jsonConverterBase.Logger = Logger;

            var textSpanJsonConverter = new TextSpanJsonConverter
            {
                ShortFormat = ShortTextSpans,
                EmptyTextSpanFormat = EmptyTextSpanFormat,
                IsLineColumn = LineColumnTextSpans,
                CodeFile = codeFile
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Indented ? Formatting.Indented : Formatting.None,
                Converters = new List<JsonConverter>
                {
                    stringEnumConverter,
                    jsonConverterBase,
                    LanguageJsonConverter.Instance,
                    textSpanJsonConverter,
                    new CodeFileJsonConverter
                    {
                        TextSpanJsonConverter = textSpanJsonConverter,
                        ExcludeDefaults = ExcludeDefaults,
                        IncludeCode = IncludeCode,
                    }
                }
            };

            return jsonSettings;
        }
    }
}
