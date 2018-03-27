using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PT.PM.PatternEditor.Pattern;

namespace PT.PM.PatternEditor
{
    public class PatternUserControl : UserControl
    {
        public PatternUserControl()
        {
            this.InitializeComponent();
            ServiceLocator.PatternsViewModel = new PatternsViewModel(this);
            this.DataContext = ServiceLocator.PatternsViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
