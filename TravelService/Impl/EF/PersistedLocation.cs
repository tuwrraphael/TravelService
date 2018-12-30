using System.Collections.Generic;

namespace TravelService.Impl.EF
{
    public class PersistedLocation
    {
        public string Id { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public string UserId { get; set; }
        public string Term { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Address { get; set; }
    }
}