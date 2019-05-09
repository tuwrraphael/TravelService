using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TravelService.Client.ApiDefinition;
using TravelService.Models;

namespace TravelService.Client.Impl
{
    internal class LocationApi : ILocationApi
    {
        private readonly Func<Task<HttpClient>> _clientFactory;
        private readonly string _userId;
        private readonly string _name;

        public LocationApi(Func<Task<HttpClient>> clientFactory,string userId, string name)
        {
            _clientFactory = clientFactory;
            _userId = userId;
            _name = name;
        }

        public async Task Put(Coordinate c)
        {
            var url = $"api/{_userId}/locations/{HttpUtility.UrlEncode(_name)}";
            var res = await (await _clientFactory()).PutAsync(url, new StringContent(
                JsonConvert.SerializeObject(c), Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode)
            {
                throw new TravelServiceException($"Could not put location: {res.StatusCode}");
            }
        }
    }
}