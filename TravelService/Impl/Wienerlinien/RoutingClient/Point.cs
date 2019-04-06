namespace TravelService.Impl
{
    public class Point
    {
        public string name { get; set; }
        public string place { get; set; }
        public string nameWithPlace { get; set; }
        public string usage { get; set; }
        public string omc { get; set; }
        public string placeID { get; set; }
        public string desc { get; set; }
        public WLDateTime dateTime { get; set; }
        public Stamp stamp { get; set; }
        public Link[] links { get; set; }
        public Ref _ref { get; set; }
    }
}
