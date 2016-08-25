using CustomerAPI.Data;
using CustomersShared.Data.DataEntities;
using System;
using System.Linq;
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

        //Test AddCustomer(customerDataTransferObject)
        [Fact]
        public void AddCustomerWithNullCustomerInfoThrowsArgumentNullException()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                CustomerDataTransferObject customerInfo = null;
                Assert.Throws<ArgumentNullException>(() => customersDataProvider.AddCustomer(customerInfo));
            }
        }

        [Fact]
        public void AddCustomerWithInvalidCustomerInfoThrowsArgumentException()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                CustomerDataTransferObject customerInfo = new CustomerDataTransferObject();
                Assert.Throws<ArgumentException>(() => customersDataProvider.AddCustomer(customerInfo));
            }
        }

        [Fact]
        public void AddCustomerWithValidCustomerInfoAddsCustomer()
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
                var result = customersDataProvider.AddCustomer(customerInfo);

                //assert
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(customerInfo.FirstName, result.FirstName);
                Assert.Equal(customerInfo.LastName, result.LastName);
            }
        }

        //Test UpdateCustomer(Guid, CustomerDataTransferObject) Method
        [Fact]
        public void UpdateExistingCustomerWithBadUpdateInfoThrowsArgumentException()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerEntity = customersDataProvider.GetCustomers().First();
                Assert.Throws<ArgumentException>(() =>
                        customersDataProvider.UpdateCustomer(customerEntity.Id, new CustomerDataTransferObject()));

                //assert
                var getCustomerResult = customersDataProvider.FindCustomer(customerEntity.Id);
                Assert.Equal(customerEntity.FirstName, getCustomerResult.FirstName);
                Assert.Equal(customerEntity.LastName, getCustomerResult.LastName);
            }
        }

        [Fact]
        public void UpdateNonExistingCustomerThrowsInvalidOperationException()
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
                Assert.Throws<InvalidOperationException>(() =>
                    customersDataProvider.UpdateCustomer(guid, customerDataTransferObject));
            }
        }

        [Fact]
        public void UpdateExistingCustomerInformationUpdatesAndReturns200OK()
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
                var customerUpdated = customersDataProvider.UpdateCustomer(customerToUpdateId,
                                                                           customerDataTransferObject);

                //assert
                var customerToUpdate = customersDataProvider.FindCustomer(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal("Joe", customerToUpdate.FirstName);
                Assert.Equal("Smith", customerToUpdate.LastName);
            }
        }

        //Test Delete(Guid) Method
        [Fact]
        public void DeleteNonExistingCustomerThrowsInvalidOperationException()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var guid = Guid.NewGuid();
                Assert.Throws<InvalidOperationException>(() => customersDataProvider.DeleteCustomer(guid));
            }
        }

        [Fact]
        public void DeleteExistingCustomerRemovesCustomerAndReturnsCustomerEntity()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerToRemoveId = customersDataProvider.GetCustomers().First().Id;
                var customerResult = customersDataProvider.DeleteCustomer(customerToRemoveId);

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
                CustomerEntity customerResult;

                //assert
                Assert.False(customersDataProvider.TryFindCustomer(id, out customerResult));
                Assert.Null(customerResult);
            }
        }

        public void TryFindCustomerWithValidIdReturnsCustomer()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                foreach (var customer in customersDataProvider.GetCustomers())
                {
                    CustomerEntity customerResult;

                    //assert
                    Assert.True(customersDataProvider.TryFindCustomer(customer.Id, out customerResult));
                    Assert.Equal(customer.Id, (customerResult.Id));
                }
            }
        }

        //Test TryAddCustomer(customerDataTransferObject)
        [Fact]
        public void TryAddCustomerWithNullCustomerInfoReturnsFalse()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                CustomerDataTransferObject customerInfo = null;
                CustomerEntity result;

                //assert
                Assert.False(customersDataProvider.TryAddCustomer(customerInfo, out result));
                Assert.Null(result);
            }
        }

        [Fact]
        public void TryAddCustomerWithInvalidCustomerInfoReturnsFalse()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                CustomerDataTransferObject customerDataTransferObject = new CustomerDataTransferObject();
                CustomerEntity result;

                //Assert
                Assert.False(customersDataProvider.TryAddCustomer(customerDataTransferObject, out result));
                Assert.Null(result);
            }
        }

        [Fact]
        public void TryAddCustomerWithValidCustomerInfoAddsCustomer()
        {
            using (var customersDataProvider = CreateCustomersDataProvider())
            {
                //arrange
                CustomerEntity result;
                var customerInfo = new CustomerDataTransferObject
                {
                    FirstName = "Jon",
                    LastName = "Smith",
                    PhoneNumber = "555-555-5555"
                };

                //assert
                Assert.True(customersDataProvider.TryAddCustomer(customerInfo, out result));
                Assert.NotEqual(Guid.Empty, result.Id);
                Assert.Equal(customerInfo.FirstName, result.FirstName);
                Assert.Equal(customerInfo.LastName, result.LastName);
            }
        }

        //Test TryUpdateCustomer(Guid, CustomerDataTransferObject) Method
        [Fact]
        public void TryUpdateExistingCustomerWithBadUpdateInfoReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerEntity = customersDataProvider.GetCustomers().First();
                CustomerEntity updatedCustomerEntity;
                Assert.False(customersDataProvider.TryUpdateCustomer(customerEntity.Id, 
                                                                     new CustomerDataTransferObject(),
                                                                     out updatedCustomerEntity));

                //assert
                Assert.Null(updatedCustomerEntity);
                var getCustomerResult = customersDataProvider.FindCustomer(customerEntity.Id);
                Assert.Equal(customerEntity.FirstName, getCustomerResult.FirstName);
                Assert.Equal(customerEntity.LastName, getCustomerResult.LastName);
            }
        }

        [Fact]
        public void UpdateNonExistingCustomerReturnsFalse()
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
                CustomerEntity addedCustomerEntity;
                Assert.False(customersDataProvider.TryUpdateCustomer(guid, 
                                                                     customerDataTransferObject,
                                                                     out addedCustomerEntity));
                Assert.Null(addedCustomerEntity);
            }
        }

        [Fact]
        public void UpdateExistingCustomerInformationUpdatesAndReturnsTrue()
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
                CustomerEntity customerUpdated;
                Assert.True(customersDataProvider.TryUpdateCustomer(customerToUpdateId,
                                                                 customerDataTransferObject,
                                                                 out customerUpdated));

                //assert
                Assert.NotNull(customerUpdated);
                Assert.Equal("Joe", customerUpdated.FirstName);
                Assert.Equal("Smith", customerUpdated.LastName);

                //assert
                var customerToUpdate = customersDataProvider.FindCustomer(customerToUpdateId);
                Assert.NotNull(customerToUpdate);
                Assert.Equal("Joe", customerToUpdate.FirstName);
                Assert.Equal("Smith", customerToUpdate.LastName);
            }
        }

        //Test Delete(Guid) Method
        [Fact]
        public void TryDeleteNonExistingCustomerReturnsFalse()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var guid = Guid.NewGuid();
                CustomerEntity customerResult;
                Assert.False(customersDataProvider.TryDeleteCustomer(guid, out customerResult));
                Assert.Null(customerResult);
            }
        }

        [Fact]
        public void TryDeleteExistingCustomerRemovesCustomerAndReturnsCustomerEntity()
        {
            using (var customersDataProvider = CreateTestCustomerDataProvider(SampleListOfCustomerDataTransferObjects))
            {
                //act
                var customerToRemoveId = customersDataProvider.GetCustomers().First().Id;
                CustomerEntity customerResult;
                Assert.True(customersDataProvider.TryDeleteCustomer(customerToRemoveId, out customerResult));

                //assert
                Assert.NotNull(customerResult);
                Assert.Equal(customerToRemoveId, customerResult.Id);
                Assert.False(customersDataProvider.CustomerExists(customerToRemoveId));
            }
        }
    }
}
