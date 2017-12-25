using System;

namespace PT.PM
{
    public class MessageEventArgs : EventArgs
    {
        public MessageType MessageType { get; set; }

        public string FullFileName { get; set; }

        public MessageEventArgs(MessageType messageType, string fullFileName)
        {
            MessageType = messageType;
            FullFileName = fullFileName;
        }

        public override string ToString()
        {
            return $"{MessageType}: {FullFileName}";
        }
    }
}
