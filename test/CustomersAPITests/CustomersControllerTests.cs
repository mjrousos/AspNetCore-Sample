using CustomerAPI.Data;
using CustomerAPITests.CustomerDataProviders;
using CustomersShared.Data.DataEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Xunit;

namespace CustomerAPI.Controllers.Tests
{
    public class CustomersControllerTests: BaseCustomersDataProviderHelpers
    {
        private const string CustomerInfoInvalidErrorText = "CustomerInfo is not valid!";
        private const string InternalServerErrorText = "Something unexpected went wrong during the request!";
        private const string CustomerNotFoundErrorText = "Could not find customer with Id of {0}";

        private readonly IServiceProvider _serviceProvider;

        public CustomersControllerTests()
        {
            // Setup mock services
            var services = new ServiceCollection();
            
            var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
            services.AddDbContext<EFCustomersDataProvider>(options =>
                options.UseInMemoryDatabase().UseInternalServiceProvider(efServiceProvider));
            services.AddScoped<ICustomersDataProvider, EFCustomersDataProvider>();
            services.AddSingleton(new ResourceManager("CustomersAPI.Resources.StringResources",
                                          typeof(Startup).GetTypeInfo().Assembly));

            _serviceProvider = services.BuildServiceProvider();
        }

        //Test Get() method
        [Fact]
        public void GetWithNoCustomersReturnsEmptyEnumerable()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                var result = customersController.Get();

                //assert
                Assert.Equal(0, result.Count());
            }
        }

        [Fact]
        public void GetWithSomeCustomersReturnsEnumerableOfCustomers()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                var result = customersController.Get();

                //assert
                Assert.Equal(SampleListOfCustomerDataTransferObjects.Count(), result.Count());
                CompareCustomerLists(SampleListOfCustomerDataTransferObjects, result);
            }
        }

        //Test Get(Guid Id) method
        [Fact]
        public void GetByIdNonExistingCustomerReturns404NotFound()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                var guid = Guid.NewGuid();
                var result = customersController.Get(guid);

                Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
                Assert.Equal(String.Format(CustomerNotFoundErrorText, guid), result.Value);
            }
        }

        [Fact]
        public void GetByIdReturnsCustomerWithCoorispondingId()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                foreach (var customer in customersDataProvider.GetCustomers())
                {
                    var result = customersController.Get(customer.Id);
                    Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
                    Assert.Equal(customer.Id, ((CustomerEntity)result.Value).Id);
                }
            }
        }

        //Test PostAsync(CustomerEntity) method
        [Fact]
        public async Task PostAsyncWithNullCustomerInfoReturnsBadRequest()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                // Pass the ResourceManager explicitly since there's no global service provider to provide it
                var result = await customersController.PostAsync(null, _serviceProvider.GetRequiredService<ResourceManager>());

                //assert
                Assert.Equal(CustomerInfoInvalidErrorText, result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            }
        }

        [Fact]
        public async Task PostAsyncWithInvalidCustomerInfoReturnsBadRequest()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                // Pass the ResourceManager explicitly since there's no global service provider to provide it
                var result = await customersController.PostAsync(new CustomerDataTransferObject(), _serviceProvider.GetRequiredService<ResourceManager>());

                //assert
                Assert.Equal(CustomerInfoInvalidErrorText, result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            }
        }

        [Fact]
        public async Task PostAsyncWithValidCustomerInfoAddsCustomerAndReturns200OK()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);
                var newCustomer = new CustomerDataTransferObject
                {
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                // Pass the ResourceManager explicitly since there's no global service provider to provide it
                var result = ((ObjectResult)await customersController.PostAsync(newCustomer, _serviceProvider.GetRequiredService<ResourceManager>()));

                //assert
                Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
                var customersResult = customersController.Get();
                Assert.Equal(1, customersResult.Count());
                var customerResult = customersResult.First();
                Assert.Equal(newCustomer.FirstName, customerResult.FirstName);
                Assert.Equal(newCustomer.LastName, customerResult.LastName);
            }
        }

        //Test PutAsync(Guid, CustomerUpdateInfo) Method
        [Fact]
        public async Task PutAsyncExistingCustomerWithBadUpdateInfoReturns400BadRequest()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                var customerEntity = customersDataProvider.GetCustomers().First();
                var result = await customersController.PutAsync(customerEntity.Id, new CustomerDataTransferObject());

                //assert
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
                Assert.Equal(CustomerInfoInvalidErrorText, result.Value);

                var getCustomerResult = customersController.Get(customerEntity.Id);
                Assert.Equal(StatusCodes.Status200OK, getCustomerResult.StatusCode);
                Assert.Equal(customerEntity.FirstName, ((CustomerEntity)getCustomerResult.Value).FirstName);
                Assert.Equal(customerEntity.LastName, ((CustomerEntity)getCustomerResult.Value).LastName);
            }
        }

        [Fact]
        public async Task PutAsyncNonExistingCustomerReturns404NotFound()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                var customerUpdateInfo = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var guid = Guid.NewGuid();
                var result = await customersController.PutAsync(guid, customerUpdateInfo);

                //assert
                Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
                Assert.Equal(String.Format(CustomerNotFoundErrorText, guid), result.Value);
            }
        }

        [Fact]
        public async Task PutAsyncExistingCustomerInformationUpdatesAndReturns200OK()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                var customerUpdateInfo = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var customerToUpdateId = customersDataProvider.GetCustomers().First().Id;
                await customersController.PutAsync(customerToUpdateId, customerUpdateInfo);

                //assert
                var customerToUpdate = customersController.Get(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal(StatusCodes.Status200OK, customerToUpdate.StatusCode);
                Assert.Equal("Joe", ((CustomerEntity)customerToUpdate.Value).FirstName);
                Assert.Equal("Smith", ((CustomerEntity)customerToUpdate.Value).LastName);
            }
        }

        //Test DeleteAsync(Guid) Method
        [Fact]
        public async Task DeleteAsyncNonExistingCustomerReturns404NotFound()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                var guid = Guid.NewGuid();
                var customerResult = await customersController.DeleteAsync(guid);

                //assert
                Assert.Equal(StatusCodes.Status404NotFound, customerResult.StatusCode);
                Assert.Equal(String.Format(CustomerNotFoundErrorText, guid), customerResult.Value);
            }
        }

        [Fact]
        public async Task DeleteAsyncExistingCustomerRemovesCustomerAndReturns200OK()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customersController = CreateTestCustomersController(customersDataProvider);

                //act
                var customerToRemoveId = customersDataProvider.GetCustomers().First().Id;
                var customerResult = await customersController.DeleteAsync(customerToRemoveId);

                //assert
                Assert.Equal(StatusCodes.Status200OK, customerResult.StatusCode);
                Assert.Equal(customerToRemoveId, ((CustomerEntity)customerResult.Value).Id);
                Assert.Equal(StatusCodes.Status404NotFound,
                             (customersController.Get(customerToRemoveId)).StatusCode);
            }
        }

        #region TestHelpers
        internal override ICustomersDataProvider CreateCustomersDataProvider() => _serviceProvider.GetRequiredService<ICustomersDataProvider>();

        /// <summary>
        /// Creates a test CustomersController to test with
        /// </summary>
        private CustomersController CreateTestCustomersController(ICustomersDataProvider customersDataProvider)
        {
            var controller = new CustomersController(customersDataProvider, _serviceProvider.GetRequiredService<ResourceManager>());

            // Set mock HTTP context (including DI service provider)
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.RequestServices = _serviceProvider;            

            return controller;
        }
        #endregion
    }
}
