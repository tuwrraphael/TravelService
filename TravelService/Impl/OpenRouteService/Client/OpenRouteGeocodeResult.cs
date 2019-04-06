namespace TravelService.Impl
{
    public class OpenRouteGeocodeResult
    {
        public Geocoding geocoding { get; set; }
        public string type { get; set; }
        public Feature[] features { get; set; }
        public float[] bbox { get; set; }
    }
}
