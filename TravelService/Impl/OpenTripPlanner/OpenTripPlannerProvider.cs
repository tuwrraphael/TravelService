using System.Threading.Tasks;
using TravelService.Models;
using TravelService.Services;

namespace TravelService.Impl.OpenTripPlanner
{

    public class OpenTripPlannerProvider : ITransitDirectionProvider
    {
        private readonly IOpenTripPlannerClient _openTripPlannerClient;

        public OpenTripPlannerProvider(IOpenTripPlannerClient openTripPlannerClient)
        {
            _openTripPlannerClient = openTripPlannerClient;
        }

        public async Task<Models.Plan> GetDirectionsAsync(TransitDirectionsRequest request)
        {
            var res = await _openTripPlannerClient.Plan(new OpenTripPlannerRequest()
            {
                FromPlace = request.From,
                ToPlace = request.To,
                ArriveBy = request.ArriveBy,
                DateTime = request.DateTime
            });
            return res.ToPlan();
        }
    }
}
