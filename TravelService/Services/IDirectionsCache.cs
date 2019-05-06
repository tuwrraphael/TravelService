﻿using System.Threading.Tasks;
using TravelService.Models;

namespace TravelService.Services
{
    public interface IDirectionsCache
    {
        Task<string> PutAsync(Plan directions);
        Task<Plan> GetAsync(string key);
    }
}
