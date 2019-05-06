namespace TravelService.Models
{
    public class UnresolvedLocation
    {
        public const string Home = "#home";
        public UnresolvedLocation(Coordinate coordinate)
        {
            Coordinate = coordinate;
        }
        public UnresolvedLocation(string address)
        {
            Address = address;
        }
        public Coordinate Coordinate { get; }
        public string Address { get;  }
    }
}
