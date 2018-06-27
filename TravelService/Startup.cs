﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using CalendarService.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DigitService.Client;
using OAuthApiClient;
using TravelService.Impl;
using TravelService.Services;
using Microsoft.AspNetCore.Mvc;

namespace TravelService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            if (string.IsNullOrWhiteSpace(hostingEnvironment.WebRootPath))
            {
                hostingEnvironment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<IDirectionService, DirectionService>();
            services.AddTransient<ITransitDirectionProvider, GoogleMapsTransitDirectionProvider>();
            services.AddTransient<ILocationProvider, LocationProvider>();
            services.AddTransient<IGeocodeProvider, GoogleMapsGeocodeProvider>();

            services.Configure<GoogleMapsApiOptions>(Configuration);

            var authProviderBuilder = services.AddBearerTokenAuthenticationProvider("travelService")
                .UseMemoryCacheTokenStore()
                .UseClientCredentialsTokenStrategy(new ClientCredentialsConfig()
            {
                ClientSecret = Configuration["TravelServiceSecret"],
                ClientId = Configuration["TravelServiceClientId"],
                Scopes = "calendar.service digit.service",
                ServiceIdentityBaseUrl = new Uri(Configuration["ServiceIdentityUrl"])
            });

            services.AddCalendarServiceClient(new Uri(Configuration["CalendarServiceBaseUri"]), authProviderBuilder);
            services.AddDigitServiceClient(new Uri(Configuration["DigitServiceBaseUri"]), authProviderBuilder);

            services.AddMvc().AddJsonOptions(v =>
            {
                v.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["ServiceIdentityUrl"];
                    options.Audience = "travel";
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("User", builder =>
               {
                   builder.RequireAuthenticatedUser();
                   builder.RequireClaim("scope", "travel.user");
               });
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Service", builder =>
                {
                    builder.RequireClaim("scope", "travel.service");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }else
            {
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors("CorsPolicy");
            app.UseMvc();
        }
    }
}
