namespace TravelService.Models
{
    public class Coordinate
    {
        public Coordinate()
        {

        }
        public Coordinate(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public override string ToString()
        {
            return $"lat:{Lat}|lng:{Lng}";
        }
    }
}
