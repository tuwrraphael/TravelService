namespace TravelService.Models
{
    public class TraceResult
    {
        public TraceMeasures MeasuresAtTimestamp { get; set; }
        public TraceMeasures ExtrapolatedMeasures { get; set; }
    }

    public class TraceMeasures
    {
        public double RouteWidth { get; set; }
        public double ConfidenceOnRoute { get; set; }
    }
}
