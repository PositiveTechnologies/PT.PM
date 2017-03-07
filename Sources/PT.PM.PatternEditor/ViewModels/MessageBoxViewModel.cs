using Avalonia.Controls;
using ReactiveUI;
using System;

namespace PT.PM.PatternEditor
{
    public class MessageBoxViewModel : ReactiveObject
    {
        private Window window;

        public MessageBoxViewModel(Window window, string messageBoxText, string title = "", MessageBoxType messageBoxType = MessageBoxType.Ok)
        {
            this.window = window;
            Title = title;
            MessageBoxText = messageBoxText;
            MessageBoxType = messageBoxType;

            if (MessageBoxType == MessageBoxType.Ok)
            {
                OkCommand.Subscribe(_ =>
                {
                    this.window.Close(true);
                });
            }
            else if (MessageBoxType == MessageBoxType.YesNo)
            {
                YesCommand.Subscribe(_ =>
                {
                    this.window.Close(true);
                });

                NoCommand.Subscribe(_ =>
                {
                    this.window.Close(false);
                });
            }
        }

        public string Title { get; set; }

        public string MessageBoxText { get; set; }

        public MessageBoxType MessageBoxType { get; set; }

        public bool OkButtonVisible => MessageBoxType == MessageBoxType.Ok;

        public bool YesNoButtonVisible => MessageBoxType == MessageBoxType.YesNo;

        public ReactiveCommand<object> OkCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> YesCommand { get; } = ReactiveCommand.Create();

        public ReactiveCommand<object> NoCommand { get; } = ReactiveCommand.Create();
    }
}
