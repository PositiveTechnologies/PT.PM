using Avalonia.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PT.PM.Common.Utils;
using PT.PM.JavaScriptParseTreeUst;
using System;
using System.IO;
using PT.PM.Common;

namespace PT.PM.PatternEditor
{
    public class Settings
    {
        private static string defaultPath = "%localappdata%/PT.PM"
            .Replace("%localappdata%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        private static string settingsFileName = Path.Combine(defaultPath, "settings.json");

        private static readonly object saveLock = new object();
        private static JsonConverter[] jsonConverters = new JsonConverter[] { new StringEnumConverter() };

        public static string DefaultPatternsFileName = Path.Combine(defaultPath, "custom_patterns.json");

        public WindowState WindowState { get; set; } = WindowState.Normal;

        public int Left { get; set; } = -1;

        public int Top { get; set; } = -1;

        public double Width { get; set; } = -1;

        public double Height { get; set; } = -1;

        public double PatternsPanelWidth { get; set; } = 350;

        public string PatternsFileName { get; set; } = "";

        public string SelectedPatternKey { get; set; } = "";

        public bool LogExpanded { get; set; } = false;

        public string SourceFile { get; set; } = "";

        public string Source { get; set; } = "";

        public Stage SelectedStage { get; set; } = Stage.Match;

        public Language SourceLanguage { get; set; } = Language.Uncertain;

        public JavaScriptType JavaScriptType { get; set; } = JavaScriptType.Undefined;

        public bool IsErrorsExpanded { get; set; } = false;

        public bool IsTokensExpanded { get; set; } = false;

        public bool IsParseTreeExpanded { get; set; } = false;

        public bool IsUstExpanded { get; set; } = false;

        public bool IsMatchingsExpanded { get; set; } = false;

        public bool IsPatternErrorsExpanded { get; set; } = false;

        public bool IsPatternExtraInfoExpanded { get; set; } = false;

        public bool IsIncludeTextSpans { get; set; } = false;

        public bool IsLinearTextSpans { get; set; } = false;

        public bool IsIncludeCode { get; set; } = false;

        public bool IsLeftRightDir { get; set; } = false;

        public static Settings Load()
        {
            if (FileExt.Exists(settingsFileName))
            {
                try
                {
                    var settings = JsonConvert.DeserializeObject<Settings>(FileExt.ReadAllText(settingsFileName), jsonConverters) ?? new Settings();
                    return settings;
                }
                catch
                {
                    DirectoryExt.CreateDirectory(Path.GetDirectoryName(settingsFileName));
                    return new Settings();
                }
            }
            else
            {
                DirectoryExt.CreateDirectory(Path.GetDirectoryName(settingsFileName));
                return new Settings();
            }
        }

        public void Save()
        {
            lock (saveLock)
            {
                DirectoryExt.CreateDirectory(Path.GetDirectoryName(settingsFileName));
                FileExt.WriteAllText(settingsFileName, JsonConvert.SerializeObject(this, Formatting.Indented, jsonConverters));
            }
        }
    }
}
