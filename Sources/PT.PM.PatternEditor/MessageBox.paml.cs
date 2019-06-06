using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace PT.PM.PatternEditor
{
    public class MessageBox : Window
    {
        public static async Task<bool> ShowDialog(Window owner, string message, string title = "", MessageBoxType messageBoxType = MessageBoxType.Ok)
        {
            var messageBox = new MessageBox(message, title, messageBoxType);
            return await messageBox.ShowDialog<bool>(owner);
        }

        public MessageBox(string message, string title = "", MessageBoxType messageBoxType = MessageBoxType.Ok)
        {
            InitializeComponent();
            this.AttachDevTools();

            DataContext = new MessageBoxViewModel(this, message, title, messageBoxType);
            Activate();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
