using CustomerAPI.Data;
using CustomersShared.Data.DataEntities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CustomerAPITests.CustomerDataProviders
{
    public abstract class BaseCustomerDataProviderTests : BaseCustomersDataProviderHelpers
    {
        //Test GetCustomers()
        [Fact]
        public void GetCustomersWithNoCustomersReturnsEmptyEnumerable()
        {
            using (ICustomersDataProvider customersDataProvider = CreateCustomersDataProvider())
            {
                //act
                var customerEntities = customersDataProvider.GetCustomers();

                //assert
                Assert.Equal(0, customerEntities.Count());
            }
        }

        [Fact]
        public void GetCustomersWithSomeCustomersReturnsEnumerableOfCustomers()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerEntities = customersDataProvider.GetCustomers();

                //assert
                Assert.Equal(SampleListOfCustomerDataTransferObjects.Count(), customerEntities.Count());
                CompareCustomerLists(SampleListOfCustomerDataTransferObjects, customerEntities);
            }
        }

        //Test CustomerExists()
        [Fact]
        public void CustomerExistsWithNonExistingCustomerReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                var id = Guid.NewGuid();
                Assert.False(customersDataProvider.CustomerExists(id));
            }
        }

        public void CustomerExistsWithValidIdReturnsCustomerTrue()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                foreach (var customer in customersDataProvider.GetCustomers())
                {
                    Assert.True(customersDataProvider.CustomerExists(customer.Id));
                }
            }
        }

        //Test FindCustomer()
        [Fact]
        public void FindCustomerWithNonExistingCustomerThrowsException()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                var id = Guid.NewGuid();
                Assert.Throws<InvalidOperationException>(() => customersDataProvider.FindCustomer(id));
            }
        }

        public void FindCustomerWithValidIdReturnsCustomer()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                foreach (var customer in customersDataProvider.GetCustomers())
                {
                    var customerEntity = customersDataProvider.FindCustomer(customer.Id);

                    //assert
                    Assert.Equal(customer.Id, (customerEntity.Id));
                }
            }
        }

        //Test AddCustomerAsync(customerDataTransferObject)
        [Fact]
        public async Task AddCustomerAsyncWithNullCustomerInfoThrowsArgumentNullException()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                CustomerDataTransferObject customerInfo = null;

                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await customersDataProvider.AddCustomerAsync(customerInfo));
            }
        }

        [Fact]
        public async Task AddCustomerAsyncWithInvalidCustomerInfoThrowsArgumentException()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                CustomerDataTransferObject customerInfo = new CustomerDataTransferObject();

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await customersDataProvider.AddCustomerAsync(customerInfo));
            }
        }

        [Fact]
        public async Task AddCustomerAsyncWithValidCustomerInfoAddsCustomer()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                var customerInfo = new CustomerDataTransferObject
                {
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var result = await customersDataProvider.AddCustomerAsync(customerInfo);

                //assert
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(customerInfo.FirstName, result.FirstName);
                Assert.Equal(customerInfo.LastName, result.LastName);
            }
        }

        //Test UpdateCustomerAsync(Guid, CustomerDataTransferObject) Method
        [Fact]
        public async Task UpdateAsyncExistingCustomerWithBadUpdateInfoThrowsArgumentException()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerEntity = customersDataProvider.GetCustomers().First();
                await Assert.ThrowsAsync<ArgumentException>(async () => await
                        customersDataProvider.UpdateCustomerAsync(customerEntity.Id, new CustomerDataTransferObject()));

                //assert
                var getCustomerResult = customersDataProvider.FindCustomer(customerEntity.Id);
                Assert.Equal(customerEntity.FirstName, getCustomerResult.FirstName);
                Assert.Equal(customerEntity.LastName, getCustomerResult.LastName);
            }
        }

        [Fact]
        public async Task UpdateAsyncNonExistingCustomerThrowsInvalidOperationException()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customerDataTransferObject = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                var guid = Guid.NewGuid();

                //assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await
                    customersDataProvider.UpdateCustomerAsync(guid, customerDataTransferObject));
            }
        }

        [Fact]
        public async Task UpdateAsyncExistingCustomerInformationUpdatesAndReturns200OK()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customerDataTransferObject = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                var customerToUpdateId = customersDataProvider.GetCustomers().First().Id;
                var customerUpdated = await customersDataProvider.UpdateCustomerAsync(customerToUpdateId,
                                                                                      customerDataTransferObject);

                //assert
                var customerToUpdate = customersDataProvider.FindCustomer(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal("Joe", customerToUpdate.FirstName);
                Assert.Equal("Smith", customerToUpdate.LastName);
            }
        }

        //Test DeleteAsync(Guid) Method
        [Fact]
        public async Task DeleteAsyncNonExistingCustomerThrowsInvalidOperationException()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var guid = Guid.NewGuid();
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await customersDataProvider.DeleteCustomerAsync(guid));
            }
        }

        [Fact]
        public async Task DeleteAsyncExistingCustomerRemovesCustomerAndReturnsCustomerEntity()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerToRemoveId = customersDataProvider.GetCustomers().First().Id;
                var customerResult = await customersDataProvider.DeleteCustomerAsync(customerToRemoveId);

                //assert
                Assert.NotNull(customerResult);
                Assert.Equal(customerToRemoveId, customerResult.Id);
                Assert.False(customersDataProvider.CustomerExists(customerToRemoveId));
            }
        }

        //Test TryFindCustomer()
        [Fact]
        public void TryFindCustomerWithNonExistingCustomerReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var id = Guid.NewGuid();

                //act
                CustomerDataActionResult customerDataActionResult = customersDataProvider.TryFindCustomer(id);

                //assert
                Assert.False(customerDataActionResult.IsSuccess);
                Assert.Null(customerDataActionResult.CustomerEntity);
            }
        }

        public void TryFindCustomerWithValidIdReturnsCustomer()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                foreach (var customer in customersDataProvider.GetCustomers())
                {
                    //act
                    CustomerDataActionResult customerDataActionResult = customersDataProvider.TryFindCustomer(customer.Id);

                    //assert
                    Assert.True(customerDataActionResult.IsSuccess);
                    Assert.Equal(customer.Id, customerDataActionResult.CustomerEntity.Id);
                }
            }
        }

        //Test TryAddCustomerAsync(customerDataTransferObject)
        [Fact]
        public async Task TryAddCustomerAsyncWithNullCustomerInfoReturnsFalse()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                CustomerDataTransferObject customerInfo = null;

                //assert
                CustomerDataActionResult result = await customersDataProvider.TryAddCustomerAsync(customerInfo);
                Assert.False(result.IsSuccess);
                Assert.Null(result.CustomerEntity);
            }
        }

        [Fact]
        public async Task TryAddCustomerAsyncWithInvalidCustomerInfoReturnsFalse()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                CustomerDataTransferObject customerDataTransferObject = new CustomerDataTransferObject();

                //Assert
                CustomerDataActionResult result = await customersDataProvider
                    .TryAddCustomerAsync(customerDataTransferObject);

                Assert.False(result.IsSuccess);
                Assert.Null(result.CustomerEntity);
            }
        }

        [Fact]
        public async Task TryAddCustomerAsyncWithValidCustomerInfoAddsCustomer()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                var customerInfo = new CustomerDataTransferObject
                {
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                CustomerDataActionResult result = await customersDataProvider.TryAddCustomerAsync(customerInfo);

                //assert

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.CustomerEntity.Id);
                Assert.Equal(customerInfo.FirstName, result.CustomerEntity.FirstName);
                Assert.Equal(customerInfo.LastName, result.CustomerEntity.LastName);
            }
        }

        //Test TryUpdateCustomer(Guid, CustomerDataTransferObject) Method
        [Fact]
        public async Task TryUpdateCustomerAsyncExistingCustomerWithBadUpdateInfoReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customerEntity = customersDataProvider.GetCustomers().First();

                //act
                CustomerDataActionResult customerDataActionResult =
                    await customersDataProvider.TryUpdateCustomerAsync(customerEntity.Id, new CustomerDataTransferObject());

                Assert.False(customerDataActionResult.IsSuccess);
                Assert.Null(customerDataActionResult.CustomerEntity);

                var getCustomerResult = customersDataProvider.FindCustomer(customerEntity.Id);
                Assert.Equal(customerEntity.FirstName, getCustomerResult.FirstName);
                Assert.Equal(customerEntity.LastName, getCustomerResult.LastName);
            }
        }

        [Fact]
        public async Task TryUpdateCustomerAsyncNonExistingCustomerReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customerDataTransferObject = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                var guid = Guid.NewGuid();

                //act
                CustomerDataActionResult customerDataActionResult =
                    await customersDataProvider.TryUpdateCustomerAsync(guid, customerDataTransferObject);

                //assert
                Assert.False(customerDataActionResult.IsSuccess);
                Assert.Null(customerDataActionResult.CustomerEntity);
            }
        }

        [Fact]
        public async Task TryUpdateCustomerAsyncExistingCustomerInformationUpdatesAndReturnsTrue()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customerToUpdateId = customersDataProvider.GetCustomers().First().Id;
                var customerDataTransferObject = new CustomerDataTransferObject
                {
                    FirstName = "Joe",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //act
                CustomerDataActionResult customerDataActionResult =
                    await customersDataProvider.TryUpdateCustomerAsync(customerToUpdateId, customerDataTransferObject);

                //assert
                Assert.True(customerDataActionResult.IsSuccess);
                Assert.Equal("Joe", customerDataActionResult.CustomerEntity.FirstName);
                Assert.Equal("Smith", customerDataActionResult.CustomerEntity.LastName);

                //assert
                var customerToUpdate = customersDataProvider.FindCustomer(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal("Joe", customerToUpdate.FirstName);
                Assert.Equal("Smith", customerToUpdate.LastName);
            }
        }

        //Test TryDeleteCustomerAsync(Guid) Method
        [Fact]
        public async Task TryDeleteCustomerAsyncNonExistingCustomerReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var guid = Guid.NewGuid();

                //act
                CustomerDataActionResult customerDataActionResult = 
                    await customersDataProvider.TryDeleteCustomerAsync(guid);

                //assert
                Assert.False(customerDataActionResult.IsSuccess);
                Assert.Null(customerDataActionResult.CustomerEntity);
            }
        }

        [Fact]
        public async Task TryDeleteCustomerAsyncExistingCustomerRemovesCustomerAndReturnsCustomerEntity()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //arrange
                var customerToRemoveId = customersDataProvider.GetCustomers().First().Id;

                //act
                CustomerDataActionResult customerDataActionResult =
                    await customersDataProvider.TryDeleteCustomerAsync(customerToRemoveId);

                //assert
                Assert.True(customerDataActionResult.IsSuccess);
                Assert.Equal(customerToRemoveId, customerDataActionResult.CustomerEntity.Id);
                Assert.False(customersDataProvider.CustomerExists(customerToRemoveId));
            }
        }
    }
}
