// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersMVC.CustomersAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Net.Http;

namespace CustomersMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Localization: This adds the localization service. This service is also required for the ViewLocalization
            //               and DataAnnotationsLocalization. The ResourcePath sets the base location of the resources to
            //               a folder called resources.
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
              .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
              .AddDataAnnotationsLocalization();

            // Configuration: Here we add the HomeControllerOptions and set it to the Configurations HomeControllerOptions section
            //                which in this case came from the appsettings.json files. This is a convenient way to pass configuration
            //                options around using dependency injection. It also helps scope just the options that apply to logical
            //                parts of the application to those parts of the application versus passing all configuration to all logical
            //                parts of the application.
            services.Configure<HomeControllerOptions>(Configuration.GetSection("HomeControllerOptions"));

            // Register a ResiliencePoliciesFactory and then use it to create
            // and register the Policy[] to be used
            services.AddTransient<ResiliencePoliciesFactory>();
            services.AddSingleton(sp =>
                sp.GetRequiredService<ResiliencePoliciesFactory>().CreatePolicies());

            // Add the HttpClient used to access other services
            services.AddSingleton(CreateHttpClient());

            // Add the CustomersApiService into the dependency container
            services.AddSingleton<CustomersAPIService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // It shouldn't be necessary to add logging providers in startup.cs anymore (they belong in program.cs)
            // but this one is required due to a bug in Application Insights
            // https://github.com/Microsoft/ApplicationInsights-aspnetcore/issues/536
            loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Information);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            // Middleware: Here, we add our custom middleware type to the HTTP processing pipeline
            app.UseRequestCorrelation();

            // Localization: Here we are building a list of supported cultures which will be used in
            //               the RequestLocalizationOptions in the app.UseRequestLocalization call below.
            var supportedCultures = new[]
              {
                    new CultureInfo("en-US"),
                    new CultureInfo("es-MX"),
                    new CultureInfo("fr-FR"),
              };

            // Localization: Here we are configuring the RequstLocalization including setting the supported cultures from above
            //               in the RequestLocalizationOptions. We are also setting the default request culture to be used
            //               for current culture. These options will be used wherever we request localized strings.
            //               For more information see https://docs.asp.net/en/latest/fundamentals/localization.html
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),

                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,

                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// Get's the URL from CustomersAPIService:URL setting in appsettings.json
        /// </summary>
        private string GetCustomersAPIUrl()
        {
            var endpoint = Configuration["CustomersAPIService:Url"];
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException("CustomerAPIService",
                                                "Need to specify CustomerAPIService in appsettings.json");
            }

            return endpoint;
        }

        /// <summary>
        /// Creates an HTTPClient with the appsettings.json Url
        /// </summary>
        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(GetCustomersAPIUrl())
            };

            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "AnalyzerStatusCheck");

            return client;
        }
    }
}
