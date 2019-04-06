namespace TravelService.Impl
{
    public class Query
    {
        public string text { get; set; }
        public int size { get; set; }
        public bool _private { get; set; }
        public int focuspointlat { get; set; }
        public int focuspointlon { get; set; }
        public Lang lang { get; set; }
        public int querySize { get; set; }
        public string parser { get; set; }
        public Parsed_Text parsed_text { get; set; }
    }
}
