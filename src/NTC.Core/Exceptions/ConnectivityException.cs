using System;

namespace NTC.Core.Exceptions
{
    public class ConnectivityException : Exception
    {
        public ConnectivityException(string message) : base(message) { }
        public ConnectivityException(string message, Exception innerException) : base(message, innerException) { }
    }
}
