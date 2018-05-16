using System;
using System.Runtime.Serialization;

namespace TravelService.Client
{
    [Serializable]
    internal class TravelServiceException : Exception
    {
        public TravelServiceException()
        {
        }

        public TravelServiceException(string message) : base(message)
        {
        }

        public TravelServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TravelServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}