using System;
using System.Runtime.Serialization;
using TravelService.Models;

namespace TravelService
{
    [Serializable]
    internal class LocationNotFoundException : Exception
    {
        public LocationNotFoundException(UnresolvedLocation unresolvedLocation):
            base($"Cannot resolve {unresolvedLocation.Address}/{unresolvedLocation.Coordinate}")
        {
        }
    }
}