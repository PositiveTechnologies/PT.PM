using System;

namespace PT.PM
{
    public class MessageEventArgs : EventArgs
    {
        public MessageType MessageType { get; set; }

        public string FileName { get; set; }

        public MessageEventArgs(MessageType messageType, string fileName)
        {
            MessageType = messageType;
            FileName = fileName;
        }
    }
}
