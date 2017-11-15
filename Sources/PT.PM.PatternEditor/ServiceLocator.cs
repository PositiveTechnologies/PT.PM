using Avalonia.Controls;
using System;
using System.IO;

namespace PT.PM.PatternEditor
{
    internal class ServiceLocator
    {
        internal static string TempDirectory = Path.GetTempPath();

        internal static Settings Settings { get; set; } = Settings.Load();

        internal static Window MainWindow { get; set; }

        internal static MainWindowViewModel MainWindowViewModel { get; set; }

        internal static PatternViewModel PatternViewModel { get; set; }
    }
}
