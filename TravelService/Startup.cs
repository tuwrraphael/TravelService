using System;
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
using TravelService.Impl.EF;
using Microsoft.EntityFrameworkCore;
using TravelService.Impl.WienerLinien.RoutingClient;
using TravelService.Impl.OpenRouteService.Client;
using TravelService.Impl.GoogleMaps;
using TravelService.Impl.OpenRouteService;
using TravelService.Impl.OpenTripPlanner;

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
            services.AddDbContext<TravelServiceContext>(options =>
                options.UseSqlite($"Data Source={HostingEnvironment.WebRootPath}\\App_Data\\travelService.db")
            );
            services.AddTransient<IDirectionService, DirectionService>();
            services.AddTransient<ITransitDirectionProvider, OpenTripPlannerProvider>();
            services.AddTransient<IGeocodeProvider, GoogleMapsGeocodeProvider>();
            services.AddTransient<ILocationsProvider, OpenRouteServiceLocationsProvider>();
            services.AddTransient<ILocationsService, LocationsService>();
            services.AddTransient<IResolvedLocationsStore, ResolvedLocationsStore>();
            services.AddTransient<IDirectionsCache, DirectionsCache>();

            services.Configure<ApiOptions>(Configuration);

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
            services.AddHttpClient<IWienerLinienRoutingClient, WienerLinienRoutingClient>("WienerlinienClient",
                c => { c.BaseAddress = new Uri("https://www.wienerlinien.at"); });
            services.AddHttpClient<IOpenRouteServiceClient, OpenRouteServiceClient>("OpenRouteServiceClient",
                c => { c.BaseAddress = new Uri("https://api.openrouteservice.org/"); });
            services.AddHttpClient<IOpenTripPlannerClient, OpenTripPlannerClient>("OpenTripPlannerClient",
                c => { c.BaseAddress = new Uri(Configuration["OpenTripPlannerUrl"]); });
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<TravelServiceContext>())
                {
                    context.Database.Migrate();
                }
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
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
