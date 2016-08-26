using CustomersShared.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CustomerAPI.Data
{
    public class EFCustomersDataProvider : DbContext, ICustomersDataProvider
    {
        private DbSet<CustomerEntity> _customers { get; set; }
        private readonly CustomerDataActionResult _failedCustomerDataActionResult = new CustomerDataActionResult(false, null);

        public EFCustomersDataProvider(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Returns the list of CustomerEntities
        /// </summary>
        public IEnumerable<CustomerEntity> GetCustomers()
        {
            return _customers;
        }

        /// <summary>
        /// Returns whether the customer Id exists in the customers
        /// </summary>
        public bool CustomerExists(Guid id)
        {
            //TODO: Use DBSet.FindAsync when EF 1.1 comes out
            return (_customers.FirstOrDefault(c => c.Id == id) != null);
        }

        /// <summary>
        /// Adds a customer using the customerDataTransferObject
        /// </summary>
        public async Task<CustomerEntity> AddCustomerAsync(CustomerDataTransferObject customerDataTransferObject)
        {
            if (customerDataTransferObject == null)
            {
                throw new ArgumentNullException();
            }

            if (!customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                throw new ArgumentException();
            }

            var newCustomerEntity = new CustomerEntity();
            UpdateCustomerInfo(newCustomerEntity, customerDataTransferObject);
            var customerEntityAdded = _customers.Add(newCustomerEntity);

            if (customerEntityAdded?.Entity == null)
            {
                return null;
            }

            await this.SaveChangesAsync();
            return customerEntityAdded.Entity;
        }

        /// <summary>
        /// Deletes a customer from the customers list
        /// </summary>
        public async Task<CustomerEntity> DeleteCustomerAsync(Guid id)
        {
            var customerEntity = _customers.First(c => c.Id == id);
            var customerRemoved = _customers.Remove(customerEntity);

            await this.SaveChangesAsync();
            return customerRemoved.Entity;
        }

        /// <summary>
        /// Finds a customer by ID. Throws exception if the customer Id does not exist
        /// </summary>
        public CustomerEntity FindCustomer(Guid id)
        {
            //TODO: When EF 1.1 comes in change this to use Customers.FindAsync method instead of Customers.First
            return _customers.First(c => c.Id == id);
        }

        /// <summary>
        /// Updates an existing customer's information to the customerDataTransferObject
        /// </summary>
        public async Task<CustomerEntity> UpdateCustomerAsync(Guid id, CustomerDataTransferObject customerDataTransferObject)
        {
            var customerEntity = FindCustomer(id);
            UpdateCustomerInfo(customerEntity, customerDataTransferObject);

            await this.SaveChangesAsync();
            return customerEntity;
        }

        /// <summary>
        /// Tries to add a customer. If the customer is added, then returns true
        /// </summary>
        public async Task<CustomerDataActionResult> TryAddCustomerAsync(CustomerDataTransferObject customerDataTransferObject)
        {
            try
            {
                var customerEntity = await AddCustomerAsync(customerDataTransferObject);

                if (customerEntity == null)
                {
                    return _failedCustomerDataActionResult;
                }

                return new CustomerDataActionResult(true, customerEntity);
            }
            catch
            {
                return _failedCustomerDataActionResult;
            }
        }

        /// <summary>
        /// Tries to delete a customer from the customer list
        /// </summary>
        public async Task<CustomerDataActionResult> TryDeleteCustomerAsync(Guid id)
        {
            if (!CustomerExists(id))
            {
                return _failedCustomerDataActionResult;
            }

            try
            {
                var customerEntity = await DeleteCustomerAsync(id);

                if (customerEntity == null)
                {
                    return _failedCustomerDataActionResult;
                }

                await this.SaveChangesAsync();
                return new CustomerDataActionResult(true, customerEntity);
            }
            catch
            {
                return _failedCustomerDataActionResult;
            }
        }

        /// <summary>
        /// Tries to find a customer by ID. If the customer Id is not found then returns false
        /// </summary>
        public CustomerDataActionResult TryFindCustomer(Guid id)
        {
            try
            {
                var customerEntity = FindCustomer(id);

                if (customerEntity == null)
                {
                    return _failedCustomerDataActionResult;
                }

                return new CustomerDataActionResult(true, customerEntity);
            }
            catch
            {
                return _failedCustomerDataActionResult;
            }
        }

        /// <summary>
        /// Tries to update an existing customer with the customerDataTransferObject
        /// </summary>
        public async Task<CustomerDataActionResult> TryUpdateCustomerAsync(Guid id, CustomerDataTransferObject customerDataTransferObject)
        {
            if (!CustomerExists(id))
            {
                return _failedCustomerDataActionResult;
            };

            try
            {
                var customerEntity = await UpdateCustomerAsync(id, customerDataTransferObject);

                if (customerEntity == null)
                {
                    return _failedCustomerDataActionResult;
                }

                return new CustomerDataActionResult(true, customerEntity);
            }
            catch
            {
                return _failedCustomerDataActionResult;
            }
        }

        /// <summary>
        /// Updates an existing customer's information with the customerDataTransferObject
        /// </summary>
        private void UpdateCustomerInfo(CustomerEntity customerEntity,
                                        CustomerDataTransferObject customerDataTransferObject)
        {
            if (customerEntity == null
                || customerDataTransferObject == null
                || !customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                throw new ArgumentException();
            }

            foreach (var item in customerDataTransferObject.GetType().GetTypeInfo().GetProperties())
            {
                var desinationPropertyInfo = typeof(CustomerEntity).GetProperty(item.Name);
                desinationPropertyInfo?.SetValue(customerEntity, item.GetValue(customerDataTransferObject));
            }
        }
    }
}
