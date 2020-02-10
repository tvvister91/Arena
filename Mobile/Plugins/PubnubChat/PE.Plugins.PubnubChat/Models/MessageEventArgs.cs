using System;

namespace PE.Plugins.PubnubChat.Models
{
    public class MessageEventArgs : EventArgs
    {
        #region Constructors

        public MessageEventArgs()
        {
            Message = string.Empty;
        }

        public MessageEventArgs(string message)
        {
            Message = message;
        }

        #endregion Constructors

        #region Properties

        public object State { get; set; }

        public string Message { get; set; }

        public bool Success { get; set; } = true;

        public DateTime Timestamp { get; set; }

        #endregion Properties
    }
}
