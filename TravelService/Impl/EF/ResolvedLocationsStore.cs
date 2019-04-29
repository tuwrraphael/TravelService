using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TravelService.Models.Locations;
using TravelService.Services;

namespace TravelService.Impl.EF
{
    public class ResolvedLocationsStore : IResolvedLocationsStore
    {
        private readonly TravelServiceContext travelServiceContext;

        public ResolvedLocationsStore(TravelServiceContext travelServiceContext)
        {
            this.travelServiceContext = travelServiceContext;
        }

        public async Task DeleteAsync(string term, string userId)
        {
            foreach (var persisted in (await travelServiceContext.PersistedLocations.Where(v => v.Term == term && v.UserId == userId).ToArrayAsync()))
            {
                travelServiceContext.Remove(persisted);
            }
            await travelServiceContext.SaveChangesAsync();
        }

        public async Task<ResolvedLocation> GetAsync(string term, string userId)
        {
            return await travelServiceContext.PersistedLocations.Where(v => v.Term == term && v.UserId == userId)
                .Select(v => new ResolvedLocation(new Models.Coordinate(v.Lat, v.Lng))
                {
                    Address = v.Address,
                    Attributes = v.Attributes,
                })
                .SingleOrDefaultAsync();
        }

        public async Task PersistAsync(string term, string userId, ResolvedLocation resolvedLocation)
        {
            var persisted = await travelServiceContext.PersistedLocations.Where(v => v.Term == term && v.UserId == userId).SingleOrDefaultAsync();
            if (null == persisted)
            {
                persisted = new PersistedLocation()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Term = term
                };
                travelServiceContext.Add(persisted);
            }
            persisted.Lat = resolvedLocation.Coordinate.Lat;
            persisted.Lng = resolvedLocation.Coordinate.Lng;
            persisted.Address = resolvedLocation.Address;
            persisted.Attributes = resolvedLocation.Attributes;
            await travelServiceContext.SaveChangesAsync();
        }
    }
}
