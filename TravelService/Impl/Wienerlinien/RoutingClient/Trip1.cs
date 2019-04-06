namespace TravelService.Impl
{
    public class Trip1
    {
        public string duration { get; set; }
        public string interchange { get; set; }
        public string desc { get; set; }
        public Leg[] legs { get; set; }
        public object[] attrs { get; set; }
    }
}
