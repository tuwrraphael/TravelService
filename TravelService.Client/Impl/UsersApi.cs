﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TravelService.Client
{
    internal class UsersApi : IUsers
    {
        private readonly Func<Task<HttpClient>> _clientFactory;

        public UsersApi(Func<Task<HttpClient>> clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IUserApi this[string userId] => new UserApi(_clientFactory, userId);

        public IUserApi Me => new UserApi(_clientFactory, "me");
    }
}
