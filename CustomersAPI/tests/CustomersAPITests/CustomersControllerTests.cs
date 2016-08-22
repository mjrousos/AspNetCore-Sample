using CustomerAPI.Data;
using CustomersShared.Data.DataEntities;
using CustomersShared.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CustomerAPI.Controllers.Tests
{
    public class CustomersControllerTests
    {
        private readonly IEnumerable<CustomerEntity> SampleListOfCustomerEntities;

        public CustomersControllerTests()
        {
            //populate SampleListOfCustomerEntites
            SampleListOfCustomerEntities = CreateListOfCustomerEntities(10);
        }

        //Test Get() method
        [Fact]
        public void GetWithNoCustomersReturnsEmptyEnumerable()
        {
            using (var customersDbContext = CreateTestDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var result = customersController.Get();

                //assert
                Assert.Equal(0, result.Count());
            }
        }

        [Fact]
        public void GetWithSomeCustomersReturnsEnumerableOfCustomers()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var result = customersController.Get();

                //assert
                Assert.Equal(SampleListOfCustomerEntities.Count(), result.Count());
                Assert.Equal(0, SampleListOfCustomerEntities.Except(result).Count());
            }
        }

        //Test Get(Guid Id) method
        [Fact]
        public void GetByIdNonExistingCustomerReturns404NotFound()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var guid = Guid.NewGuid();
                var result = customersController.Get(guid);

                Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
                Assert.Equal(ResourceStrings.GetCustomerNotFoundText(guid), result.Value);
            }
        }

        [Fact]
        public void GetByIdReturnsCustomerWithCoorispondingId()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                foreach (var customer in SampleListOfCustomerEntities)
                {
                    var result = customersController.Get(customer.Id);
                    Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
                    Assert.Equal(customer.Id, ((CustomerEntity)result.Value).Id);
                }
            }
        }

        //Test Post(CustomerEntity) method
        [Fact]
        public void PostWithNullCustomerInfoReturnsBadRequest()
        {
            using (var customersDbContext = CreateTestDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var result = customersController.Post(null);

                //assert
                Assert.Equal(ResourceStrings.CustomerInvalidInfoText, result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            }
        }

        //Test Post(CustomerEntity) method
        [Fact]
        public void PostWithInvalidCustomerInfoReturnsBadRequest()
        {
            using (var customersDbContext = CreateTestDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var result = customersController.Post(new UpdateableCustomerInfo());

                //assert
                Assert.Equal(ResourceStrings.CustomerInvalidInfoText, result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            }
        }

        [Fact]
        public void PostWithValidCustomerInfoAddsCustomerAndReturns200OK()
        {
            using (var customersDbContext = CreateTestDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);
                var newCustomer = new UpdateableCustomerInfo
                {
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var result = (ObjectResult)customersController.Post(newCustomer);

                //assert
                Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
                var customersResult = customersController.Get();
                Assert.Equal(1, customersResult.Count());
                var customerResult = customersResult.First();
                Assert.Equal(newCustomer.FirstName, customerResult.FirstName);
                Assert.Equal(newCustomer.LastName, customerResult.LastName);
            }
        }

        //Test Put(Guid, CustomerUpdateInfo) Method
        [Fact]
        public void UpdateExistingCustomerWithBadUpdateInfoReturns400BadRequest()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var customerEntity = SampleListOfCustomerEntities.First();
                var result = customersController.Put(customerEntity.Id, new UpdateableCustomerInfo());

                //assert
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
                Assert.Equal(ResourceStrings.CustomerInvalidInfoText, result.Value);

                var getCustomerResult = customersController.Get(customerEntity.Id);
                Assert.Equal(StatusCodes.Status200OK, getCustomerResult.StatusCode);
                Assert.Equal(customerEntity.FirstName, ((CustomerEntity)getCustomerResult.Value).FirstName);
                Assert.Equal(customerEntity.LastName, ((CustomerEntity)getCustomerResult.Value).LastName);
            }
        }

        [Fact]
        public void UpdateNonExistingCustomerReturns404NotFound()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                var customerUpdateInfo = new UpdateableCustomerInfo
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var guid = Guid.NewGuid();
                var result = customersController.Put(guid, customerUpdateInfo);

                //assert
                Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
                Assert.Equal(ResourceStrings.GetCustomerNotFoundText(guid), result.Value);
            }
        }

        [Fact]
        public void UpdateExistingCustomerInformationUpdatesAndReturns200OK()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                var customerUpdateInfo = new UpdateableCustomerInfo
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var customerToUpdateId = SampleListOfCustomerEntities.First().Id;
                customersController.Put(customerToUpdateId, customerUpdateInfo);

                //assert
                var customerToUpdate = customersController.Get(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal(StatusCodes.Status200OK, customerToUpdate.StatusCode);
                Assert.Equal("Joe", ((CustomerEntity)customerToUpdate.Value).FirstName);
                Assert.Equal("Smith", ((CustomerEntity)customerToUpdate.Value).LastName);
            }
        }

        //Test Delete(Guid) Method
        [Fact]
        public void DeleteNonExistingCustomerReturns404NotFound()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var guid = Guid.NewGuid();
                var customerResult = customersController.Delete(guid);

                //assert
                Assert.Equal(StatusCodes.Status404NotFound, customerResult.StatusCode);
                Assert.Equal(ResourceStrings.GetCustomerNotFoundText(guid), customerResult.Value);
            }
        }

        [Fact]
        public void DeleteExistingCustomerRemovesCustomerAndReturns200OK()
        {
            using (var customersDbContext = CreateTestDbContext(SampleListOfCustomerEntities))
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var customerToRemoveId = SampleListOfCustomerEntities.First().Id;
                var customerResult = customersController.Delete(customerToRemoveId);

                //assert
                Assert.Equal(StatusCodes.Status200OK, customerResult.StatusCode);
                Assert.Equal(customerToRemoveId, ((CustomerEntity)customerResult.Value).Id);
                Assert.Equal(StatusCodes.Status404NotFound,
                             (customersController.Get(customerToRemoveId)).StatusCode);
            }
        }

        #region TestHelpers
        /// <summary>
        /// Creates a test CustomersDbContext
        /// </summary>
        private CustomersDbContext CreateTestDbContext()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<CustomersDbContext>();
            builder.UseInMemoryDatabase()
                   .UseInternalServiceProvider(serviceProvider);

            return new CustomersDbContext(builder.Options);
        }

        /// <summary>
        /// Creates a test CustomersDbContext with the CustomerEntities passed in
        /// </summary>
        private CustomersDbContext CreateTestDbContext(IEnumerable<CustomerEntity> customerList)
        {
            var customersDbContext = CreateTestDbContext();

            foreach (var customerEntity in customerList)
            {
                customersDbContext.Add(customerEntity);
                customersDbContext.SaveChanges();
            }

            return customersDbContext;
        }

        /// <summary>
        /// Creates a test CustomersController to test with
        /// </summary>
        private CustomersController CreateTestCustomersController(CustomersDbContext customersDbContext)
        {
            return new CustomersController(customersDbContext);
        }

        /// <summary>
        /// Creates a list of customers for using in tests
        /// </summary>
        /// <param name="count">number of customers to put in the list</param>
        private IEnumerable<CustomerEntity> CreateListOfCustomerEntities(int numberOfRecordsToCreate)
        {
            var customersList = new List<CustomerEntity>();

            for (int i = 0; i < numberOfRecordsToCreate; i++)
            {
                customersList.Add(new CustomerEntity
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"CustFirst{i}",
                    LastName = $"CustLast{i}",
                    PhoneNumber = $"CustPhone{i}"
                });
            }

            Assert.Equal(numberOfRecordsToCreate, customersList.Count());
            return customersList;
        }
        #endregion
    }
}
