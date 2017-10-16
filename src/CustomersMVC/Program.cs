// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace CustomersMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // WebHost.CreateDefaultBuilder is a convenient helper method that
            // will configure an IWebHostBuilder with common configuration
            // (Kestrel, typical logging and config settings, etc.).
            // It is still possible to modify the IWebHostBuilder after it has
            // been created with CreateDefaultBuiler, as shown here.
            var host = WebHost
                .CreateDefaultBuilder()
                .UseUrls("http://+:5001")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
