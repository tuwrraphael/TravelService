using System;
using System.Runtime.Serialization;

namespace TravelService
{
    [Serializable]
    internal class UserLocationNotFoundException : Exception
    {
        public UserLocationNotFoundException()
        {
        }

        public UserLocationNotFoundException(string message) : base(message)
        {
        }

        public UserLocationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserLocationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}