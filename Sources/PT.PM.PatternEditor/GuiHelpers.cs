using Avalonia.Controls;
using PT.PM.Common.Exceptions;

namespace PT.PM.PatternEditor
{
    internal class GuiHelpers
    {
        internal static void ProcessErrorOnDoubleClick(ListBox errorsListBox, TextBox inputTextBox)
        {
            errorsListBox.Focus();
            ParsingException parsingException;
            ConversionException conversionException;
            MatchingException matchException;
            int selectionStart = -1, selectionEnd = -1;
            if ((parsingException = errorsListBox.SelectedItem as ParsingException) != null)
            {
                selectionStart = parsingException.TextSpan.Start;
                selectionEnd = parsingException.TextSpan.End;
            }
            else if ((conversionException = errorsListBox.SelectedItem as ConversionException) != null)
            {
                var textSpan = conversionException.TextSpan;
                if (textSpan != null)
                {
                    selectionStart = textSpan.Start;
                    selectionEnd = textSpan.End;
                }
            }
            else if ((matchException = errorsListBox.SelectedItem as MatchingException) != null)
            {
                var textSpan = matchException.TextSpan;
                if (textSpan != null)
                {
                    selectionStart = textSpan.Start;
                    selectionEnd = textSpan.End;
                }
            }
            if (selectionStart != -1)
            {
                inputTextBox.SelectionStart = selectionStart;
                inputTextBox.SelectionEnd = selectionEnd;
                inputTextBox.CaretIndex = inputTextBox.SelectionEnd + 1;
            }
        }
    }
}
