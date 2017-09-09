// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomersAPI.CustomerDataProviders.Tests
{
    public class EFCustomersDataProviderTests : BaseCustomerDataProviderTests
    {
        internal override ICustomersDataProvider CreateCustomersDataProvider()
        {
            // Create a fresh service provider, and therefore a fresh
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<EFCustomersDataProvider>();
            builder.UseInMemoryDatabase("CustomersDB")
                       .UseInternalServiceProvider(serviceProvider);

            return new EFCustomersDataProvider(builder.Options);
        }
    }
}
