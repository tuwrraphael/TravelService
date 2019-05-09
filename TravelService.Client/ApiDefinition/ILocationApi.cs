using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Client.ApiDefinition
{
    public interface ILocationApi
    {
        Task Put(Coordinate c);
    }
}
