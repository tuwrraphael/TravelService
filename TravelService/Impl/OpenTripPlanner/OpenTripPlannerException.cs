using System;
using System.Runtime.Serialization;

namespace TravelService.Impl.OpenTripPlanner
{
    [Serializable]
    internal class OpenTripPlannerException : Exception
    {
        public OpenTripPlannerException()
        {
        }

        public OpenTripPlannerException(string message) : base(message)
        {
        }

        public OpenTripPlannerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OpenTripPlannerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}