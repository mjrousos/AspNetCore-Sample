// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersAPI.Data;
using CustomersShared.Data.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace CustomersAPI.CustomerDataProviders.Tests
{
    public abstract class BaseCustomersDataProviderHelpers
    {
        public BaseCustomersDataProviderHelpers()
        {
            // populate SampleListOfCustomerDataTransferObjects
            SampleListOfCustomerDataTransferObjects = CreateListOfCustomers(10);
        }

        internal IEnumerable<CustomerDataTransferObject> SampleListOfCustomerDataTransferObjects { get; }

        /// <summary>
        /// Creates a test customersDataProvider
        /// </summary>
        internal virtual ICustomersDataProvider CreateCustomersDataProvider()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a test EFCustomersDataProvider with the CustomerEntities passed in
        /// </summary>
        internal ICustomersDataProvider CreateTestCustomerDataProvider(IEnumerable<CustomerDataTransferObject> customerList)
        {
            var customersDataProvider = CreateCustomersDataProvider();

            foreach (var customerInfo in customerList)
            {
                customersDataProvider.AddCustomerAsync(customerInfo);
            }

            return customersDataProvider;
        }

        /// <summary>
        /// Ensures all customer properties except the ID for each customer in both lists are equal
        /// </summary>
        internal void CompareCustomerLists(IEnumerable<CustomerDataTransferObject> listOfCustomerInfo,
                                          IEnumerable<CustomerEntity> listOfCustomerEntities)
        {
            var compareList = listOfCustomerEntities.Select(c => new CustomerDataTransferObject
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                City = c.City,
                State = c.State,
                ZipCode = c.ZipCode
            }).AsEnumerable<CustomerDataTransferObject>();

            Assert.Empty(listOfCustomerInfo.Except(compareList));
        }

        /// <summary>
        /// Ensures each customer property except id are equal in both customer objects
        /// </summary>
        internal void CompareCustomers(CustomerDataTransferObject customerInfo, CustomerEntity customerEntity)
        {
            foreach (var item in customerInfo.GetType().GetTypeInfo().GetProperties())
            {
                var customerEntityPropertyInfo = GetCustomerEntityPropertyInfo(item);
                Assert.Equal(customerEntityPropertyInfo.GetValue(customerEntity), item.GetValue(customerInfo));
            }
        }

        /// <summary>
        /// Creates a list of customers for using in tests
        /// </summary>
        /// <param name="numberOfRecordsToCreate">number of customers to put in the list</param>
        private IEnumerable<CustomerDataTransferObject> CreateListOfCustomers(int numberOfRecordsToCreate)
        {
            var customersList = new List<CustomerDataTransferObject>();

            for (var i = 0; i < numberOfRecordsToCreate; i++)
            {
                customersList.Add(new CustomerDataTransferObject
                {
                    FirstName = $"CustFirst{i}",
                    LastName = $"CustLast{i}",
                    PhoneNumber = $"CustPhone{i}"
                });
            }

            Assert.Equal(numberOfRecordsToCreate, customersList.Count());
            return customersList;
        }

        /// <summary>
        /// Gets CustomerEntity propertyInfo object
        /// </summary>
        private PropertyInfo GetCustomerEntityPropertyInfo(PropertyInfo item)
        {
            return typeof(CustomerEntity).GetProperty(item.Name);
        }
    }
}
