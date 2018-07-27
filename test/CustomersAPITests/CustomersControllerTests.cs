// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersAPI.CustomerDataProviders.Tests;
using CustomersAPI.Data;
using CustomersDemo.Tests.Helpers;
using CustomersShared.Data.DataEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using Xunit;

namespace CustomersAPI.Controllers.Tests
{
    public class CustomersControllerTests : BaseCustomersDataProviderHelpers
    {
        private const string CustomerInfoInvalidErrorText = "CustomerInfo is not valid!";
        private const string UnexpectedServerErrorText = "Something unexpected went wrong during the request!";
        private const string CustomerNotFoundErrorText = "Could not find customer with Id of {0}";
        private const string LoggingAddedCustomerText = "PostAsync: Successfully added customer {0}";
        private const string LoggingAddingCustomerText = "PostAsync: Adding customer {0}";
        private const string LoggingDeletedCustomerText = "DeleteAsync: Successfully deleted customer {0}";
        private const string LoggingDeletingCustomerText = "DeleteAsync: Deleting customer {0}";
        private const string LoggingGetCustomerText = "Get: Getting customer {0}";
        private const string LoggingGetCustomersText = "Get: Getting Customers";
        private const string LoggingUpdatedCustomerText = "PutAsync: Successfully updated customer {0}";
        private const string LoggingUpdatingCustomerText = "PutAsync: Updating customer {0}";
        private readonly IServiceProvider _serviceProvider;

        public CustomersControllerTests()
        {
            // Setup mock services
            var services = new ServiceCollection();

            // Add EF service
            var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
            services.AddDbContext<EFCustomersDataProvider>(options =>
                options.UseInMemoryDatabase("CustomersDB").UseInternalServiceProvider(efServiceProvider));
            services.AddScoped<ICustomersDataProvider, EFCustomersDataProvider>();

            // Add resources services
            var resourceManager = new ResourceManager("CustomersAPI.Resources.Controllers.CustomersController",
                      typeof(Startup).GetTypeInfo().Assembly);
            services.AddSingleton(resourceManager);
            services.AddSingleton(typeof(IStringLocalizer<CustomersController>),
                                  new StringLocalizer<CustomersController>(new TestStringLocalizerFactory(resourceManager, CreateTestLogger())));

            _serviceProvider = services.BuildServiceProvider();
        }

        // Test Get() method
        [Fact]
        public void GetWithNoCustomersReturnsEmptyEnumerable()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                // arrange
                // Logging: passing in a test logger to verify in unit tests that the logging is correct
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                var result = customersController.Get();

                // assert
                Assert.Empty(result);
                VerifyOnlyLogMessage(testLogger, LoggingGetCustomersText);
            }
        }

        [Fact]
        public void GetWithSomeCustomersReturnsEnumerableOfCustomers()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                var result = customersController.Get();

                // assert
                Assert.Equal(SampleListOfCustomerDataTransferObjects.Count(), result.Count());
                CompareCustomerLists(SampleListOfCustomerDataTransferObjects, result);
                VerifyOnlyLogMessage(testLogger, LoggingGetCustomersText);
            }
        }

        // Test Get(Guid Id) method
        [Fact]
        public void GetByIdNonExistingCustomerReturns404NotFound()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                var guid = Guid.NewGuid();
                var result = customersController.Get(guid);

                // assert
                var expectedErrorMessage = string.Format(CustomerNotFoundErrorText, guid);
                Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
                Assert.Equal(expectedErrorMessage, result.Value);

                // assert logging
                Assert.Equal(2, testLogger.LoggedMessages.Count);
                VerifyLogMessage(testLogger, string.Format(LoggingGetCustomerText, guid), LogLevel.Information, 0);
                VerifyLogMessage(testLogger, $"Get: {expectedErrorMessage}", LogLevel.Error, 1);
            }
        }

        [Fact]
        public void GetByIdReturnsCustomerWithCoorispondingId()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                foreach (var customer in customersDataProvider.GetCustomers())
                {
                    testLogger.ClearMessages();
                    var result = customersController.Get(customer.Id);
                    Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
                    Assert.Equal(customer.Id, ((CustomerEntity)result.Value).Id);
                    VerifyOnlyLogMessage(testLogger, string.Format(LoggingGetCustomerText, customer.Id));
                }
            }
        }

        // Test PostAsync(CustomerEntity) method
        [Fact]
        public async Task PostAsyncWithNullCustomerInfoReturnsBadRequest()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                // Pass the ResourceManager explicitly since there's no global service provider to provide it
                var result = await customersController.PostAsync(null, _serviceProvider.GetRequiredService<ResourceManager>());

                // assert
                Assert.Equal(CustomerInfoInvalidErrorText, result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
                VerifyOnlyLogMessage(testLogger, $"PostAsync: {CustomerInfoInvalidErrorText}", LogLevel.Error);
            }
        }

        [Fact]
        public async Task PostAsyncWithInvalidCustomerInfoReturnsBadRequest()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                // Pass the ResourceManager explicitly since there's no global service provider to provide it
                var result = await customersController.PostAsync(new CustomerDataTransferObject(), _serviceProvider.GetRequiredService<ResourceManager>());

                // assert
                Assert.Equal(CustomerInfoInvalidErrorText, result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
                VerifyOnlyLogMessage(testLogger, $"PostAsync: {CustomerInfoInvalidErrorText}", LogLevel.Error);
            }
        }

        [Fact]
        public async Task PostAsyncWithValidCustomerInfoAddsCustomerAndReturns200OK()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);
                var newCustomer = new CustomerDataTransferObject
                {
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                // act
                // Pass the ResourceManager explicitly since there's no global service provider to provide it
                var result = await customersController.PostAsync(newCustomer, _serviceProvider.GetRequiredService<ResourceManager>());

                // assert status code
                Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

                // assert logging
                var customerName = $"{newCustomer.FirstName} {newCustomer.LastName}";
                Assert.Equal(2, testLogger.LoggedMessages.Count);
                VerifyLogMessage(testLogger, string.Format(LoggingAddingCustomerText, customerName));
                VerifyLogMessage(testLogger, string.Format(LoggingAddedCustomerText, customerName), LogLevel.Information, 1);

                // assert customer was added
                var customersResult = customersController.Get();
                Assert.Single(customersResult);
                var customerResult = customersResult.First();
                Assert.Equal(newCustomer.FirstName, customerResult.FirstName);
                Assert.Equal(newCustomer.LastName, customerResult.LastName);
            }
        }

        // Test PutAsync(Guid, CustomerUpdateInfo) Method
        [Fact]
        public async Task PutAsyncExistingCustomerWithBadUpdateInfoReturns400BadRequest()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                var customerEntity = customersDataProvider.GetCustomers().First();
                var result = await customersController.PutAsync(customerEntity.Id, new CustomerDataTransferObject());

                // assert
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
                Assert.Equal(CustomerInfoInvalidErrorText, result.Value);
                VerifyOnlyLogMessage(testLogger, $"PutAsync: {CustomerInfoInvalidErrorText}", LogLevel.Error);

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
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                var customerUpdateInfo = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                // act
                var guid = Guid.NewGuid();
                var result = await customersController.PutAsync(guid, customerUpdateInfo);

                // assert
                var expectedErrorMessage = string.Format(CustomerNotFoundErrorText, guid);
                Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
                Assert.Equal(expectedErrorMessage, result.Value);
                VerifyLogMessage(testLogger, $"PutAsync: {expectedErrorMessage}", LogLevel.Error, 1);
            }
        }

        [Fact]
        public async Task PutAsyncExistingCustomerInformationUpdatesAndReturns200OK()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                var customerUpdateInfo = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                // act
                var customerToUpdateId = customersDataProvider.GetCustomers().First().Id;
                await customersController.PutAsync(customerToUpdateId, customerUpdateInfo);

                // assert logging
                Assert.Equal(2, testLogger.LoggedMessages.Count);
                VerifyLogMessage(testLogger, string.Format(LoggingUpdatingCustomerText, customerToUpdateId));
                VerifyLogMessage(testLogger, string.Format(LoggingUpdatedCustomerText, customerToUpdateId), LogLevel.Information, 1);

                // assert information updated
                var customerToUpdate = customersController.Get(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal(StatusCodes.Status200OK, customerToUpdate.StatusCode);
                Assert.Equal("Joe", ((CustomerEntity)customerToUpdate.Value).FirstName);
                Assert.Equal("Smith", ((CustomerEntity)customerToUpdate.Value).LastName);
            }
        }

        // Test DeleteAsync(Guid) Method
        [Fact]
        public async Task DeleteAsyncNonExistingCustomerReturns404NotFound()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                var guid = Guid.NewGuid();
                var customerResult = await customersController.DeleteAsync(guid);

                // assert
                var expectedErrorMessage = string.Format(CustomerNotFoundErrorText, guid);
                Assert.Equal(StatusCodes.Status404NotFound, customerResult.StatusCode);
                Assert.Equal(expectedErrorMessage, customerResult.Value);
                VerifyLogMessage(testLogger, $"DeleteAsync: {expectedErrorMessage}", LogLevel.Error, 1);
            }
        }

        [Fact]
        public async Task DeleteAsyncExistingCustomerRemovesCustomerAndReturns200OK()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                // arrange
                var testLogger = CreateTestLogger();
                var customersController = CreateTestCustomersController(customersDataProvider, testLogger);

                // act
                var customerToRemoveId = customersDataProvider.GetCustomers().First().Id;
                var customerResult = await customersController.DeleteAsync(customerToRemoveId);

                // assert logging
                Assert.Equal(2, testLogger.LoggedMessages.Count);
                VerifyLogMessage(testLogger, string.Format(LoggingDeletingCustomerText, customerToRemoveId));
                VerifyLogMessage(testLogger, string.Format(LoggingDeletedCustomerText, customerToRemoveId), LogLevel.Information, 1);

                // assert customer removed
                Assert.Equal(StatusCodes.Status200OK, customerResult.StatusCode);
                Assert.Equal(customerToRemoveId, ((CustomerEntity)customerResult.Value).Id);
                Assert.Equal(StatusCodes.Status404NotFound,
                             customersController.Get(customerToRemoveId).StatusCode);
            }
        }

        internal override ICustomersDataProvider CreateCustomersDataProvider() => _serviceProvider.GetRequiredService<ICustomersDataProvider>();

        /// <summary>
        /// Creates a test instance of the CustomersController passing in a ICustomersDataProvider, ResourceManager and ILogger
        /// </summary>
        private CustomersController CreateTestCustomersController(ICustomersDataProvider customersDataProvider, TestLogger<CustomersController> testLogger)
        {
            var controller = new CustomersController(customersDataProvider,
                                                     _serviceProvider.GetRequiredService<ResourceManager>(),
                                                     _serviceProvider.GetRequiredService<IStringLocalizer<CustomersController>>(),
                                                     testLogger);

            // Set mock HTTP context (including DI service provider)
            controller.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                RequestServices = _serviceProvider
            };

            return controller;
        }

        /// <summary>
        /// Logging: An example of asserting logging information from a controller.
        /// Verifies there is only one logged message in the TestLogger and verifies the information in the logged message
        /// </summary>
        /// <param name="testLogger"></param>
        /// <param name="expectedMessage"></param>
        /// <param name="expectedLogLevel"></param>
        private void VerifyOnlyLogMessage(TestLogger<CustomersController> testLogger,
                                          string expectedMessage,
                                          LogLevel expectedLogLevel = LogLevel.Information)
        {
            VerifyLogMessage(testLogger, expectedMessage, expectedLogLevel, 0);
        }

        /// <summary>
        /// Verifies a single log message in the TestLogger
        /// </summary>
        /// <param name="testLogger">TestLogger object</param>
        /// <param name="expectedMessage">Expected message of the indexed log message</param>
        /// <param name="expectedLogLevel">Expected log level of the indexed log message</param>
        /// <param name="logMessageIndexToVerify">Which index in the logged messages to verify</param>
        private void VerifyLogMessage(TestLogger<CustomersController> testLogger,
                                      string expectedMessage,
                                      LogLevel expectedLogLevel = LogLevel.Information,
                                      int logMessageIndexToVerify = 0)
        {
            Assert.Equal(testLogger.BuildLogString(expectedLogLevel, expectedMessage), testLogger.LoggedMessages[logMessageIndexToVerify]);
        }

        /// <summary>
        /// Returns a TestLogger to be used by the tests
        /// </summary>
        private TestLogger<CustomersController> CreateTestLogger()
        {
            return new TestLogger<CustomersController>();
        }

        /// <summary>
        /// Returns an IStringLocalizerFactory to be used in adding the mocked IStringLocalizer service
        /// </summary>
        private class TestStringLocalizerFactory : IStringLocalizerFactory
        {
            private readonly ResourceManager _resourceManager;
            private readonly string _resourcePath;
            private readonly ILogger _testLogger;

            public TestStringLocalizerFactory(ResourceManager resManager, ILogger testLogger, string resourcePath = "Resources")
            {
                _resourceManager = resManager;
                _resourcePath = resourcePath;
                _testLogger = testLogger;
            }

            public IStringLocalizer Create(Type resourceSource)
            {
                return new ResourceManagerStringLocalizer(_resourceManager,
                                                          resourceSource.GetTypeInfo().Assembly,
                                                          _resourcePath,
                                                          new ResourceNamesCache(),
                                                          _testLogger);
            }

            public IStringLocalizer Create(string baseName, string location)
            {
                throw new NotImplementedException("Not used");
            }
        }
    }
}
