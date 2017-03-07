using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace PT.PM.PatternEditor
{
    public class MessageBox : Window
    {
        public static async Task<bool> ShowDialog(string message, string title = "", MessageBoxType messageBoxType = MessageBoxType.Ok)
        {
            var messageBox = new MessageBox(message, title, messageBoxType);
            return await messageBox.ShowDialog<bool>();
        }

        public MessageBox(string message, string title = "", MessageBoxType messageBoxType = MessageBoxType.Ok)
        {
            this.InitializeComponent();
            DataContext = new MessageBoxViewModel(this, message, title, messageBoxType);
            App.AttachDevTools(this);
            Activate();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
