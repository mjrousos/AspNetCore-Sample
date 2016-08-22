using CustomersShared.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace CustomerAPI.Data
{
    public class CustomersDbContext : DbContext
    {
        public virtual DbSet<CustomerEntity> Customers { get; set; }

        public CustomersDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Adds a customer to the customers dbset
        /// </summary>
        /// <param name="customerInfo">Information for the customer being added</param>
        internal CustomerEntity AddCustomer(UpdateableCustomerInfo customerInfo)
        {
            var customerEntity = new CustomerEntity
            {
                Id = Guid.NewGuid()
            };

            UpdateCustomerInfo(customerEntity, customerInfo);
            Customers.Add(customerEntity);
            this.SaveChanges();

            return customerEntity;
        }

        /// <summary>
        /// Updates an existing customer's information with the customerUpdateInfo
        /// </summary>
        /// <param name="customer">Existing customer to update</param>
        /// <param name="customerUpdateInfo">Information to update information</param>
        internal void UpdateCustomerInfo(CustomerEntity customer, UpdateableCustomerInfo customerUpdateInfo)
        {
            foreach (var item in customerUpdateInfo.GetType().GetTypeInfo().GetProperties())
            {
                var desinationPropertyInfo = typeof(CustomerEntity).GetProperty(item.Name);
                desinationPropertyInfo?.SetValue(customer, item.GetValue(customerUpdateInfo));
            }
        }

        /// <summary>
        /// Validates customer information passed in to ensure that it meets the requirements
        /// </summary>
        internal bool ValidateUpdateableCustomerInfo(UpdateableCustomerInfo customerInfo)
        {
            return customerInfo != null
                && !string.IsNullOrEmpty(customerInfo.FirstName)
                && !string.IsNullOrEmpty(customerInfo.LastName);
        }
    }
}
