using Avalonia.Controls;
using System;

namespace PT.PM.PatternEditor
{
    internal class ServiceLocator
    {
        internal static string TempDirectory = AppDomain.CurrentDomain.BaseDirectory;

        internal static Settings Settings { get; set; } = new Settings();

        internal static Window MainWindow { get; set; }

        internal static MainWindowViewModel MainWindowViewModel { get; set; }

        internal static PatternViewModel PatternViewModel { get; set; }
    }
}
