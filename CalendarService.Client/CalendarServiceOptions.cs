using System;

namespace CalendarService.Client
{
    public class CalendarServiceOptions
    {
        public Uri CalendarServiceBaseUri { get; set; }
        public string CalendarServiceClientId { get; set; }
        public string CalendarServiceClientSecret { get; set; }
        public Uri ServiceIdentityUrl { get; set; }
    }
}
