// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace CustomersMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://+:5001")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
