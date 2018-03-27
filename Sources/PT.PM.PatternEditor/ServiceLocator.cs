using Avalonia.Controls;
using PT.PM.PatternEditor.Pattern;
using System.IO;

namespace PT.PM.PatternEditor
{
    internal class ServiceLocator
    {
        internal static string TempDirectory = Path.GetTempPath() + "PT.PM.Dump";

        internal static Settings Settings { get; set; } = Settings.Load();

        internal static Window MainWindow { get; set; }

        internal static MainWindowViewModel MainWindowViewModel { get; set; }

        internal static PatternsViewModel PatternsViewModel { get; set; }
    }
}
