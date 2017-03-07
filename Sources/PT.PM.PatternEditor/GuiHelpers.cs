using PT.PM.Common;
using PT.PM.Common.Exceptions;
using Avalonia.Controls;

namespace PT.PM.PatternEditor
{
    internal class GuiHelpers
    {
        internal static void ProcessErrorOnDoubleClick(ListBox errorsListBox, TextBox inputTextBox)
        {
            errorsListBox.Focus();
            ParsingException parsingException;
            ConversionException conversionException;
            MatchingException matchingException;
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
            else if ((matchingException = errorsListBox.SelectedItem as MatchingException) != null)
            {
                var textSpan = matchingException.TextSpan;
                if (textSpan != null)
                {
                    selectionStart = textSpan.Start;
                    selectionEnd = textSpan.End;
                }
            }
            if (selectionStart != -1)
            {
                inputTextBox.SelectionStart = selectionStart + 1;
                inputTextBox.SelectionEnd = selectionEnd + 1;
                inputTextBox.CaretIndex = inputTextBox.SelectionEnd + 1;
            }
        }
    }
}
