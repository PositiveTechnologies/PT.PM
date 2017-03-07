using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PT.PM.PatternEditor
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            
            var productVersion = System.Windows.Forms.Application.ProductVersion;
            Title = "Pattern Editor " + productVersion;
            ServiceLocator.MainWindow = this;
            ServiceLocator.MainWindowViewModel = new MainWindowViewModel(this);
            this.DataContext = ServiceLocator.MainWindowViewModel;
            App.AttachDevTools(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
