using CustomersShared.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomerAPI.Data
{
    public class EFCustomersDataProvider : DbContext, ICustomersDataProvider
    {
        private DbSet<CustomerEntity> Customers { get; set; }

        public EFCustomersDataProvider(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Returns the list of CustomerEntities
        /// </summary>
        public IEnumerable<CustomerEntity> GetCustomers()
        {
            return Customers;
        }

        /// <summary>
        /// Returns whether the customer Id exists in the customers
        /// </summary>
        public bool CustomerExists(Guid id)
        {
            return (Customers.FirstOrDefault(c => c.Id == id) != null);
        }

        /// <summary>
        /// Adds a customer using the customerDataTransferObject
        /// </summary>
        public CustomerEntity AddCustomer(CustomerDataTransferObject customerDataTransferObject)
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
            var customerEntityAdded = Customers.Add(newCustomerEntity);

            if (customerEntityAdded?.Entity != null)
            {
                this.SaveChanges();
                return customerEntityAdded.Entity;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a customer from the customers list
        /// </summary>
        public CustomerEntity DeleteCustomer(Guid id)
        {
            var customerEntity = Customers.First(c => c.Id == id);
            var customerRemoved = Customers.Remove(customerEntity);

            this.SaveChanges();
            return customerRemoved.Entity;
        }

        /// <summary>
        /// Finds a customer by ID. Throws exception if the customer Id does not exist
        /// </summary>
        public CustomerEntity FindCustomer(Guid id)
        {
            //TODO: When EF 1.1 comes in use Customers.Find instead of Customers.First
            return Customers.First(c => c.Id == id);
        }

        /// <summary>
        /// Updates an existing customer's information to the customerDataTransferObject
        /// </summary>
        public CustomerEntity UpdateCustomer(Guid id, CustomerDataTransferObject customerDataTransferObject)
        {
            var customerEntity = FindCustomer(id);

            UpdateCustomerInfo(customerEntity, customerDataTransferObject);
            this.SaveChanges();

            return customerEntity;
        }

        /// <summary>
        /// Tries to add a customer. If the customer is added, then returns true
        /// </summary>
        public bool TryAddCustomer(CustomerDataTransferObject customerDataTransferObject,
                                   out CustomerEntity customerEntity)
        {
            if (customerDataTransferObject == null)
            {
                customerEntity = null;
                return false;
            }

            if (!customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                customerEntity = null;
                return false;
            }

            try
            {
                customerEntity = AddCustomer(customerDataTransferObject);

                if (customerEntity != null)
                {
                    this.SaveChanges();
                    return true;
                }
                else
                {
                    customerEntity = null;
                    return false;
                }
            }
            catch
            {
                customerEntity = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to delete a customer from the customer list
        /// </summary>
        public bool TryDeleteCustomer(Guid id, out CustomerEntity customerEntity)
        {
            CustomerEntity customerToRemove;
            if (!TryFindCustomer(id, out customerToRemove))
            {
                customerEntity = null;
                return false;
            }

            try
            {
                var result = Customers.Remove(customerToRemove);

                if (result?.Entity != null)
                {
                    this.SaveChanges();
                    customerEntity = result.Entity;
                    return true;
                }
                else
                {
                    customerEntity = null;
                    return false;
                }
            }
            catch
            {
                customerEntity = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to find a customer by ID. If the customer Id is not found then returns false
        /// </summary>
        public bool TryFindCustomer(Guid id, out CustomerEntity customerEntity)
        {
            try
            {
                customerEntity = Customers.First(c => c.Id == id);
                return customerEntity != null;
            }
            catch
            {
                customerEntity = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to update an existing customer with the customerDataTransferObject
        /// </summary>
        public bool TryUpdateCustomer(Guid id,
                                      CustomerDataTransferObject customerDataTransferObject,
                                      out CustomerEntity customerEntity)
        {
            if (!TryFindCustomer(id, out customerEntity))
            {
                customerEntity = null;
                return false;
            };

            try
            {
                UpdateCustomerInfo(customerEntity, customerDataTransferObject);
                this.SaveChanges();

                return true;
            }
            catch
            {
                customerEntity = null;
                return false;
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
