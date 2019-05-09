namespace TravelService.Models
{
    public class DirectionsNotFoundResult
    {
        public DirectionsNotFoundReason Reason { get; set; }
        public bool StartAddressFound { get; set; }
        public bool EndAddressFound { get; set; }
    }

    public enum DirectionsNotFoundReason
    {
        AddressNotFound,
        RouteNotFound
    }
}
