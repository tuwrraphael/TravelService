namespace TravelService.Models
{
    public class UserLocation
    {
        public UserLocation(Coordinate coordinate)
        {
            Coordinate = coordinate;
        }
        public UserLocation(string address)
        {
            Address = address;
        }
        public Coordinate Coordinate { get; }
        public string Address { get;  }
    }
}
