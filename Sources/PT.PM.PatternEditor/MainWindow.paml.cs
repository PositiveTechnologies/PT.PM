using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PT.PM.PatternEditor
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools();

            Title = "Pattern Editor " + Utils.GetVersionString();

            ServiceLocator.MainWindow = this;
            ServiceLocator.MainWindowViewModel = new MainWindowViewModel(this);
            DataContext = ServiceLocator.MainWindowViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
