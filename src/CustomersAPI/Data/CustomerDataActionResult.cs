// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersShared.Data.DataEntities;

namespace CustomersAPI.Data
{
    public class CustomerDataActionResult
    {
        public CustomerDataActionResult(bool isSuccess, CustomerEntity customerEntity)
        {
            IsSuccess = isSuccess;
            CustomerEntity = customerEntity;
        }

        public bool IsSuccess { get; }

        public CustomerEntity CustomerEntity { get; private set; }
    }
}
