using PT.PM.Common;
using Avalonia.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

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

        public double Left { get; set; } = -1;

        public double Top { get; set; } = -1;

        public double Width { get; set; } = -1;

        public double Height { get; set; } = -1;

        public double PatternsPanelWidth { get; set; } = 350;

        public string PatternsFileName { get; set; } = "";

        public string SelectedPatternKey { get; set; } = "";

        public bool LogExpanded { get; set; } = false;

        public string SourceCodeFile { get; set; } = "";

        public string SourceCode { get; set; } = "";
        
        public Stage SelectedStage { get; set; } = Stage.Match;

        public Language SourceCodeLanguage { get; set; } = Language.CSharp;

        public bool IsDeveloperMode { get; set; } = true;

        public bool IsErrorsExpanded { get; set; } = false;

        public bool IsTokensExpanded { get; set; } = false;

        public bool IsParseTreeExpanded { get; set; } = false;

        public bool IsUstExpanded { get; set; } = false;

        public bool IsMatchingsExpanded { get; set; } = false;

        public bool IsPatternErrorsExpanded { get; set; } = false;

        public bool IsPatternJsonExpanded { get; set; } = false;

        public static Settings Load()
        { 
            if (File.Exists(settingsFileName))
            {
                try
                {
                    var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFileName), jsonConverters) ?? new Settings();
                    return settings;
                }
                catch
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(settingsFileName));
                    return new Settings();
                }
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFileName));
                return new Settings();
            }
        }

        public void Save()
        {
            lock (saveLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFileName));
                File.WriteAllText(settingsFileName, JsonConvert.SerializeObject(this, Formatting.Indented, jsonConverters));
            }
        }
    }
}
