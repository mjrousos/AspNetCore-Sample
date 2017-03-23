using CustomersShared.Data.DataEntities;

namespace CustomerAPI.Data
{
    public class CustomerDataActionResult
    {
        public bool IsSuccess { get; }
        public CustomerEntity CustomerEntity { get; private set; }

        public CustomerDataActionResult(bool isSuccess, CustomerEntity customerEntity)
        {
            IsSuccess = isSuccess;
            CustomerEntity = customerEntity;
        }
    }
}
