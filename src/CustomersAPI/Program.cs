// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using System.Resources;

namespace CustomerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()

                // Dependency Injection: Services can be registered with the ASP.NET Core DI container at IWebHost build-time
                // by calling ConfigureServices on the WebHostBuilder.
                //
                // Dependency Injection: This is an easy way of injecting services that wouldn't otherwise be available in
                // the web application's Startup class (for example, web apps running in a Service Fabric
                // application could have their service context injected here).
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton(new ResourceManager("CustomersAPI.Resources.StringResources",
                                                      typeof(Startup).GetTypeInfo().Assembly));
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
