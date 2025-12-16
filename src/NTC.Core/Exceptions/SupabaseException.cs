using System;

namespace NTC.Core.Exceptions
{
    public class SupabaseException : Exception
    {
        public SupabaseException(string message) : base(message) { }
        public SupabaseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
