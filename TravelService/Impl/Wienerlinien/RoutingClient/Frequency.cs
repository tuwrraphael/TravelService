namespace TravelService.Impl
{
    public class Frequency
    {
        public string hasFrequency { get; set; }
        public string tripIndex { get; set; }
        public string minTimeGap { get; set; }
        public string maxTimeGap { get; set; }
        public string avTimeGap { get; set; }
        public string minDuration { get; set; }
        public string maxDuration { get; set; }
        public string avDuration { get; set; }
        public object[] modes { get; set; }
    }
}
