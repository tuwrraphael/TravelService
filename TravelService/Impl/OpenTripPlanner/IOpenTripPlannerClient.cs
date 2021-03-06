﻿using System;
using System.Threading.Tasks;

namespace TravelService.Impl.OpenTripPlanner
{
    public interface IOpenTripPlannerClient
    {
        Task<OpenTripPlannerResponse> Plan(OpenTripPlannerRequest request);
        Task Subscribe(string routeId, Uri callback);
    }
}