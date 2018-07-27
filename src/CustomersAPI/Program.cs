// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using ApplicationInsightsInitializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;

namespace CustomersAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Sets the service's name in AppInsights telemetry
            CloudRoleTelemetryInitializer.SetRoleName("CustomersAPI");

            var host = new WebHostBuilder()

                // Enables automatic per-request diagnostics in AppInsights
                .UseApplicationInsights()

                // Selects Kestrel as the web host (as opposed to Http.Sys)
                .UseKestrel()

                // Dependency Injection: Services can be registered with the ASP.NET Core DI container at IWebHost build-time
                // by calling ConfigureServices on the WebHostBuilder.
                //
                // Dependency Injection: This is an easy way of injecting services that wouldn't otherwise be available in
                // the web application's Startup class (for example, web apps running in a Service Fabric
                // application could have their service context injected here).
                //
                // Localization: Here we are adding a singleton instance of the ResourceManager into the ServicesCollection for the
                //               CustomersController. The naming convention for the resource files for the CustomersController follows
                //               the path pattern by locating the resources in Resources/Controllers/CustomersController.<language>.resx.
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton(new ResourceManager("CustomersAPI.Resources.Controllers.CustomersController",
                                                   typeof(Startup).GetTypeInfo().Assembly));
                })
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }

        /// <summary>
        /// Sets up the applications IConfiguration object
        /// </summary>
        /// <param name="context">Application hosting context (with access to items like environment)</param>
        /// <param name="configBuilder">The IConfigurationBuilder to add providers to</param>
        private static void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configBuilder)
        {
            var env = context.HostingEnvironment;

            // Configuration: In this method we are adding configuration from different sources including environment
            //                variables, .json files and a List of KeyValue pairs from memory. The configuration will be
            //                added in order which means that any duplicate settings in the environment variables
            //                will override any already added settings. This allows options like the environment to be used
            //                to alter configuration based on environment.
            configBuilder

                // Configuration: add some configuration from an in memory collection
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Setting1", "Setting1Value"),
                    new KeyValuePair<string, string>("Setting2", "Setting2Value")
                })

                // Configuration: add configuration from some optional .json files
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

                // Configuration: add some configuration from environment variables
                .AddEnvironmentVariables();
        }

        /// <summary>
        /// Setup application logging
        /// </summary>
        /// <param name="context">App's host context (including environment and configuration)</param>
        /// <param name="loggingBuilder">The ILoggingBuilde to add providers to</param>
        private static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder loggingBuilder)
        {
            var env = context.HostingEnvironment;
            var config = context.Configuration;

            // Logging: The ILoggerFactory is used to keep track of all the different loggers that have been added. When
            //          using dependency injection to registered and can be accessed through dependency injection using the
            //          ILogger interface. Here there are three loggers that are added to the loggerFactory instance.
            loggingBuilder.AddDebug();

            // Logging: Some third-party logger's like Serilog also have logging support for ASP.NET Core. The Serilog.Extensions.Logging,
            //          Serilog.Settings.Configuration and Serilog.Sinks.Console packages have been added to this project. These
            //          packages allow the application to use Serilog, set the configuration and then display it using Literate in
            //          the console output. The below code creates a logger using the Serilog configuration in the AppSettings.json
            //          file and then adds the logger to the LoggerFactory.
            if (!env.IsDevelopment())
            {
                // Logging: Console loggers are very useful for debugging, but not performant and should be omitted in
                //          production. See https://blogs.msdn.microsoft.com/webdev/2017/04/26/asp-net-core-logging/ for
                //          logging into Azure App Service
            }
            else
            {
                var serilogLogger = new LoggerConfiguration()

                                    // This loads serilog sink information from configuration
                                    .ReadFrom.Configuration(config)

                                    // This will send serilog diagnostics to AppInsights as traces (correlated with whichever request
                                    // was being processed when the diagnostic was logged). This could also be done via a config file,
                                    // but is done here since it is easier to use an instrumentation key from configuration this way.
                                    // The CustomersMVC project demonstrates how to send logs directly to AppInsights without using Serilog.
                                    .WriteTo.ApplicationInsightsTraces(config["ApplicationInsights:InstrumentationKey"])
                                    .CreateLogger();

                loggingBuilder.AddSerilog(serilogLogger);
            }
        }
    }
}
