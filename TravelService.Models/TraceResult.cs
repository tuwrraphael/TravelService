using System;

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
        public PositionOnRoute PositionOnRoute { get; set; }
    }

    public class PositionOnRoute
    {
        public int LegIndex { get; set; }
        public Coordinate LocationOnRoute { get; set; }
        public TimeSpan Delay { get; set; }
    }
}
