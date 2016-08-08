using System;
using CustomerAPI.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CustomerAPI.Data
{
    public class CustomersDbContext : DbContext
    {
        private string _DatabaseName = null;

        public virtual DbSet<CustomerEntity> Customers { get; set; }

        public CustomersDbContext()
        {
        }

        public CustomersDbContext(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName can't be null");
            }

            _DatabaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (_DatabaseName == null)
            {
                options.UseInMemoryDatabase();
            }
            else
            {
                options.UseInMemoryDatabase(_DatabaseName);
            }
        }

        internal void UpdateCustomer(CustomerEntity customer, CustomerUpdateInfo customerUpdateInfo)
        {
            customer.Address = customerUpdateInfo.Address;
            customer.City = customerUpdateInfo.City;
            customer.FirstName = customerUpdateInfo.FirstName;
            customer.LastName = customerUpdateInfo.LastName;
            customer.PhoneNumber = customerUpdateInfo.PhoneNumber;
            customer.State = customerUpdateInfo.State;
            customer.ZipCode = customerUpdateInfo.ZipCode;
        }

        internal bool ValidateCustomerUpdateInfo(CustomerUpdateInfo customerInfo)
        {
            if (customerInfo == null ||
                string.IsNullOrEmpty(customerInfo.FirstName) || 
                string.IsNullOrEmpty(customerInfo.LastName) ||
                string.IsNullOrEmpty(customerInfo.PhoneNumber))
            {
                return false;
            }

            return true;
        }

        internal bool ValidateCustomerEntityInfo(CustomerEntity customerInfo)
        {
            if (customerInfo == null ||
                customerInfo.Id == null ||
                string.IsNullOrEmpty(customerInfo.FirstName) ||
                string.IsNullOrEmpty(customerInfo.LastName) ||
                string.IsNullOrEmpty(customerInfo.PhoneNumber))
            {
                return false;
            }

            return true;
        }
    }
}
