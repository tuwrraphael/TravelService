using System;

namespace TravelService.Impl
{
    public class ApiOptions
    {
        public string GoogleMapsApiKey { get; set; }
        public string OpenRouteApiKey { get; set; }
        /// <summary>
        /// provides the tile38-wienerlinien service https://github.com/tuwrraphael/tile38-wienerlinien
        /// </summary>
        public Uri Tile38WLApiUrl { get; set; }
    }
}