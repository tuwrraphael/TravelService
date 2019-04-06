using System;
using System.Runtime.Serialization;

namespace TravelService.Impl.OpenRouteService.Client
{
    [Serializable]
    internal class OpenRouteServiceException : Exception
    {
        public OpenRouteServiceException()
        {
        }

        public OpenRouteServiceException(string message) : base(message)
        {
        }

        public OpenRouteServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OpenRouteServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
