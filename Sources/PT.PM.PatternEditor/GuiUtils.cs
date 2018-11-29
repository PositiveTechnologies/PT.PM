using Avalonia.Controls;
using AvaloniaEdit;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Utils;
using PT.PM.PatternEditor.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PT.PM.PatternEditor
{
    internal class GuiUtils
    {
        internal static void ProcessErrorOnDoubleClick(ListBox errorsListBox, TextEditor inputTextBox)
        {
            errorsListBox.Focus();

            if (errorsListBox.SelectedItem is ErrorViewModel errorViewModel &&
                errorViewModel.Exception is PMException pmException &&
                !pmException.TextSpan.IsZero)
            {
                inputTextBox.SelectionStart = pmException.TextSpan.Start;
                inputTextBox.SelectionLength = pmException.TextSpan.Length;
                inputTextBox.CaretOffset = pmException.TextSpan.End;
            }
        }

        public static void OpenDirectory(string directoryName)
        {
            if (DirectoryExt.Exists(directoryName))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{directoryName}\"" });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start(new ProcessStartInfo { FileName = "xdg-open", Arguments = directoryName, CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo { FileName = "open", Arguments = directoryName, CreateNoWindow = true });
                }
            }
        }
    }
}
