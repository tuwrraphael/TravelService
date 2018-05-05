using CalendarService.Models;
using System.Threading.Tasks;

namespace CalendarService.Client
{
    public interface ICalendarServiceClient
    {
        Task<Event> GetCurrentEvent(string userId);
    }
}
