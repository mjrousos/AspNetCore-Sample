using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CustomerAPI.Data;
using Swashbuckle.Swagger.Model;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.Resources;
using System.Reflection;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using RequestCorrelation;

namespace CustomerAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            /* This section adds in configuration from different configuration sources including
               .json files and environment variables
            */
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add Swashbuckle service
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Customer Demo API",
                    Description = "Customer Demo API",
                    TermsOfService = "None"
                });
                options.DescribeAllEnumsAsStrings();
            });

            // Add Entity Framework Customers data provider
            services.AddSingleton<ICustomersDataProvider>(CreateCustomersDataProvider());

            //Add ResourceManager singleton
            services.AddSingleton(new ResourceManager("CustomersAPI.Resources.StringResources",
                                                      typeof(Startup).GetTypeInfo().Assembly));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //Add some logging options
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Middleware: Trivial middleware registration to demonstrate how
            //             to add a step to the app's HTTP request processing pipeline.
            // Middleware: Note that middleware will execute in the pipeline in the order it is registered.
            //             Since this trivial middleware is meant to time how long the entire processing
            //             of request takes, it is included very early in the middleware pipeline.
            var timingLogger = loggerFactory.CreateLogger("Timing Middleware");
            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                // Middleware: Logic to run regarding the request prior to invoking more middleware.
                //             This logic should be followed by either a call to next() (to invoke the next
                //             piece of middleware) or to context.Response.WriteAsync/SendFileAsync
                //             without calling next() (which will end the middleware pipeline and begin
                //             travelling back up the middleware stack.
                var timer = new Stopwatch();
                timer.Start();

                // Middleware: Calling the next delegate will invoke the next piece of middleware
                await next();

                // Middleware: Code after 'next' will usually run after another piece of middleware
                //             has written a response, so context.Response should not be written to here.
                timer.Stop();
                timingLogger.LogInformation($"Request to {context.Request.Method}:{context.Request.Path} processed in {timer.ElapsedMilliseconds} ms");
            });

            /* Add Global/Localization support for more information see 
               https://docs.asp.net/en/latest/fundamentals/localization.html */
            var supportedCultures = new[]
              {
                    new CultureInfo("en-US"),
                    new CultureInfo("es-MX"),
                    new CultureInfo("fr-FR"),
              };
            
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseStaticFiles();

            // Middleware: More complex middleware can be added with the app.UseMiddleware method
            app.UseMiddleware<RequestCorrelationMiddleware>();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        /// <summary>
        /// Creates a new Customers data provider with the options to create a new in memory database
        /// </summary>
        private ICustomersDataProvider CreateCustomersDataProvider()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<EFCustomersDataProvider>();
            builder.UseInMemoryDatabase()
                   .UseInternalServiceProvider(serviceProvider);

            return new EFCustomersDataProvider(builder.Options);
        }

    }
}
