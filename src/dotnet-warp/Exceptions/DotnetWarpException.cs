using System;
using System.Runtime.Serialization;

namespace DotnetWarp.Exceptions
{
    [Serializable]
    public class DotnetWarpException : Exception
    {
        public DotnetWarpException()
        {
        }

        protected DotnetWarpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DotnetWarpException(string message) : base(message)
        {
        }

        public DotnetWarpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}