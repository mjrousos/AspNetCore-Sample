using CustomersShared.Data.DataEntities;
using System;
using System.Collections.Generic;

namespace CustomerAPI.Data
{
    public interface ICustomersDataProvider : IDisposable
    {
        IEnumerable<CustomerEntity> GetCustomers();

        bool CustomerExists(Guid id);

        CustomerEntity AddCustomer(CustomerDataTransferObject customerDataTransferObject);

        CustomerEntity DeleteCustomer(Guid id);

        CustomerEntity FindCustomer(Guid id);

        CustomerEntity UpdateCustomer(Guid id, CustomerDataTransferObject customerDataTransferObject);

        bool TryFindCustomer(Guid id, out CustomerEntity customerEntity);

        bool TryDeleteCustomer(Guid id, out CustomerEntity customerEntity);

        bool TryAddCustomer(CustomerDataTransferObject customerDataTransferObject, out CustomerEntity customerEntity);

        bool TryUpdateCustomer(Guid id,
                               CustomerDataTransferObject customerDataTransferObject,
                               out CustomerEntity customerEntity);
    }
}
