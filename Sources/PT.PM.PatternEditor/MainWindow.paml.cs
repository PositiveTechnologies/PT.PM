using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace PT.PM.PatternEditor
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string productVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            Title = "Pattern Editor " + productVersion;

            ServiceLocator.MainWindow = this;
            ServiceLocator.MainWindowViewModel = new MainWindowViewModel(this);
            this.DataContext = ServiceLocator.MainWindowViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
