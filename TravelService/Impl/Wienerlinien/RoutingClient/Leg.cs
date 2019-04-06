namespace TravelService.Impl
{
    public class Leg
    {
        public string timeMinute { get; set; }
        public Point[] points { get; set; }
        public Mode mode { get; set; }
        public Frequency frequency { get; set; }
        public string path { get; set; }
        public Turninst[] turnInst { get; set; }
        public Stopseq[] stopSeq { get; set; }
        public Interchange interchange { get; set; }
    }
}
