using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PT.PM.PatternEditor
{
    public class PatternUserControl: UserControl
    {
        public PatternUserControl()
        {
            this.InitializeComponent();
            ServiceLocator.PatternViewModel = new PatternViewModel(this);
            this.DataContext = ServiceLocator.PatternViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
