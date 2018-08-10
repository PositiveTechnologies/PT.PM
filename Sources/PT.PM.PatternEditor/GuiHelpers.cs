using Avalonia.Controls;
using PT.PM.Common.Exceptions;
using PT.PM.PatternEditor.ViewModels;

namespace PT.PM.PatternEditor
{
    internal class GuiHelpers
    {
        internal static void ProcessErrorOnDoubleClick(ListBox errorsListBox, TextBox inputTextBox)
        {
            errorsListBox.Focus();

            if (errorsListBox.SelectedItem is ErrorViewModel errorViewModel &&
                errorViewModel.Exception is PMException pmException &&
                !pmException.TextSpan.IsZero)
            {
                inputTextBox.SelectionStart = pmException.TextSpan.Start;
                inputTextBox.SelectionEnd = pmException.TextSpan.End;
                inputTextBox.CaretIndex = inputTextBox.SelectionEnd + 1;
            }
        }
    }
}
