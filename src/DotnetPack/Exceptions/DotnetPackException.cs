using System;
using System.Runtime.Serialization;

namespace DotnetPack.Exceptions
{
    [Serializable]
    public class DotnetPackException : Exception
    {
        public DotnetPackException()
        {
        }

        protected DotnetPackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DotnetPackException(string message) : base(message)
        {
        }

        public DotnetPackException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}