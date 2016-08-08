using CustomerAPI.Data;
using CustomerAPI.Data.DataEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CustomerAPI.Controllers.Tests
{
    public class CustomersControllerTests
    {
        const int DefaultCustomerCount = 10;

        //Test Get() method
        [Fact]
        public void CustomersController_GetWithNoCustomersReturnsEmptyEnumerable()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                var result = customersController.Get();

                //assert
                Assert.Equal(0, result.Count());

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }

        [Fact]
        public void CustomersController_GetWithSomeCustomersReturnsEnumerableOfCustomers()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customers = CreateCustomerList(DefaultCustomerCount);
                PopulateTestCustomers(customersDbContext, customers);
                var customersController = new CustomersController(customersDbContext);

                //act
                var result = customersController.Get();

                //assert
                Assert.Equal(DefaultCustomerCount, result.Count());
                Assert.Equal(0, customers.Except(result).Count());

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }

        //Test Get(Guid Id) method
        [Fact]
        public void CustomersController_GetByIdNonExistingCustomerReturns()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customerList = CreateCustomerList(DefaultCustomerCount);
                PopulateTestCustomers(customersDbContext, customerList);
                var customersController = new CustomersController(customersDbContext);

                //act
                foreach (var customer in customerList)
                {
                    var result = customersController.Get(Guid.NewGuid());
                    Assert.Null(result);
                }

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }

        [Fact]
        public void CustomersController_GetByIdReturnsCustomerWithCoorispondingId()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customerList = CreateCustomerList(DefaultCustomerCount);
                PopulateTestCustomers(customersDbContext, customerList);
                var customersController = new CustomersController(customersDbContext);

                //act
                foreach (var customer in customerList)
                {
                    var result = customersController.Get(customer.Id);
                    Assert.Equal(customer.Id, result.Id);
                }

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }

        //Test Post(CustomerEntity) method
        [Fact]
        public void CustomersController_PostWithNullCustomerReturnsBadRequestActionResult()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);

                //act
                BadRequestObjectResult result = (BadRequestObjectResult)customersController.Post(null);

                //assert
                Assert.Equal("CustomerInfo is not valid!", result.Value);
                Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

                //Cleanup
                TestCleanUp(customersDbContext);
            }
        }

        [Fact]
        public void CustomersController_PostWithValidCustomerInfoAddsCustomerAndReturnsStatusOK200()
        {
            using (var customersDbContext = new CustomersDbContext())
            {
                //arrange
                var customersController = new CustomersController(customersDbContext);
                var newCustomer = new CustomerEntity
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                OkResult result = (OkResult)customersController.Post(newCustomer);

                //assert
                Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
                var customersResult = customersController.Get();
                Assert.Equal(1, customersResult.Count());
                var customerResult = customersResult.First(c => c.Id == newCustomer.Id);
                Assert.NotNull(customerResult);
                Assert.Equal("Jon", customerResult.FirstName);

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }


        //Test Put(Guid, CustomerUpdateInfo) Method
        [Fact]
        public void CustomersController_UpdateExistingCustomerInformationUpdates()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customers = CreateCustomerList(DefaultCustomerCount).ToList();
                PopulateTestCustomers(customersDbContext, customers);
                var customersController = new CustomersController(customersDbContext);
                var customerUpdateInfo = new CustomerUpdateInfo
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var customerToValidateId = customersDbContext.Customers.First().Id;
                customersController.Put(customerToValidateId, customerUpdateInfo) ;

                //assert
                var customerToValidate = customersDbContext.Customers.FirstOrDefault(c => c.Id == customerToValidateId);
                Assert.NotNull(customerToValidate);
                Assert.Equal("Joe", customerToValidate.FirstName);
                Assert.Equal("Smith", customerToValidate.LastName);

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }

        //Test Delete(Guid) Method
        [Fact]
        public void CustomersController_DeleteCustomerRemovesCustomer()
        {
            using (var customersDbContext = CreateDbContext())
            {
                //arrange
                var customers = CreateCustomerList(DefaultCustomerCount);
                PopulateTestCustomers(customersDbContext, customers);
                var customersController = new CustomersController(customersDbContext);

                //act
                var customerToRemoveId = customersDbContext.Customers.First().Id;
                customersController.Delete(customerToRemoveId);

                //assert
                Assert.Equal(DefaultCustomerCount - 1, customersController.Get().Count());
                Assert.Null(customersController.Get(customerToRemoveId));

                //cleanup
                TestCleanUp(customersDbContext);
            }
        }

        #region TestHelpers
        /// <summary>
        /// Creates a new EntityFramework database context
        /// </summary>
        private CustomersDbContext CreateDbContext()
        {
            return new CustomersDbContext($"TestDatabase{Guid.NewGuid()}");
        }

        /// <summary>
        /// Cleans up after each test. Right now just deleting the database that was created.
        /// </summary>
        /// <param name="dbContext"></param>
        private void TestCleanUp(DbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
        }

        /// <summary>
        /// Creates a list of customers for using in tests
        /// </summary>
        /// <param name="count">number of customers to put in the list</param>
        private IEnumerable<CustomerEntity> CreateCustomerList(int count = DefaultCustomerCount)
        {
            var customers = new List<CustomerEntity>();

            for (int i = 0; i < count; i++)
            {
                customers.Add(new CustomerEntity
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"CustFirst{i}",
                    LastName = $"CustLast{i}",
                    PhoneNumber = $"CustPhone{i}"
                });
            }

            Assert.Equal(count, customers.Count());
            return customers;
        }

        /// <summary>
        /// Populates a CustomerDbContext from a list of customers passed in
        /// </summary>
        /// <param name="customersDbContext">CustomerDbContext to use</param>
        /// <param name="customerList"></param>
        public void PopulateTestCustomers(CustomersDbContext customersDbContext, IEnumerable<CustomerEntity> customerList)
        {
            foreach (var customer in customerList)
            {
                customersDbContext.Add(customer);
                customersDbContext.SaveChanges();
            }

            Assert.Equal(0, customersDbContext.Customers.ToList().Except(customerList).Count());
        }
        #endregion
    }
}
