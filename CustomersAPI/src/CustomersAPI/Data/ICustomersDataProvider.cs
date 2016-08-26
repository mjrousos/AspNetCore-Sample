using CustomersShared.Data.DataEntities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerAPI.Data
{
    public interface ICustomersDataProvider : IDisposable
    {
        IEnumerable<CustomerEntity> GetCustomers();

        bool CustomerExists(Guid id);

        Task<CustomerEntity> AddCustomerAsync(CustomerDataTransferObject customerDataTransferObject);

        Task<CustomerEntity> DeleteCustomerAsync(Guid id);

        CustomerEntity FindCustomer(Guid id);

        Task<CustomerEntity> UpdateCustomerAsync(Guid id, CustomerDataTransferObject customerDataTransferObject);

        CustomerDataActionResult TryFindCustomer(Guid id);

        Task<CustomerDataActionResult> TryDeleteCustomerAsync(Guid id);

        Task<CustomerDataActionResult> TryAddCustomerAsync(CustomerDataTransferObject customerDataTransferObject);

        Task<CustomerDataActionResult> TryUpdateCustomerAsync(Guid id, CustomerDataTransferObject customerDataTransferObject);
    }
}
