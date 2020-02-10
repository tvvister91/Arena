using System;

namespace PE.Plugins.Network.Exceptions
{
    public class ConnectivityException : Exception
    {
        public ConnectivityException()
            : base()
        {
        }

        public ConnectivityException(string message)
            : base(message)
        {
        }

        public ConnectivityException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
