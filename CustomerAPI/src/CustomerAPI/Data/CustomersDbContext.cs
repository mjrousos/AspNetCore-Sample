using System;
using CustomerAPI.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CustomerAPI.Data
{
    public class CustomersDbContext : DbContext
    {
        private readonly string _databaseName = null;

        public virtual DbSet<CustomerEntity> Customers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="databaseName">Name of database. If null or empty, a default database will be used.</param>
        public CustomersDbContext(string databaseName = null)
        {
            _databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (_databaseName == null)
            {
                options.UseInMemoryDatabase();
            }
            else
            {
                options.UseInMemoryDatabase(_databaseName);
            }
        }

        internal void UpdateCustomer(CustomerEntity customer, CustomerUpdateInfo customerUpdateInfo)
        {
            foreach (var item in customerUpdateInfo.GetType().GetTypeInfo().GetProperties())
            {
                var desinationPropertyInfo = typeof(CustomerEntity).GetProperty(item.Name);
                desinationPropertyInfo?.SetValue(customer, item.GetValue(customerUpdateInfo));
            }
        }

        internal bool ValidateCustomerUpdateInfo(CustomerUpdateInfo customerInfo)
        {
            return customerInfo != null
                && !string.IsNullOrEmpty(customerInfo.FirstName)
                && !string.IsNullOrEmpty(customerInfo.LastName)
                && !string.IsNullOrEmpty(customerInfo.PhoneNumber);
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
