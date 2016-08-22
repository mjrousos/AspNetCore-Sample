using System;

namespace CustomersShared.Resources
{
    public static class ResourceStrings
    {
        public static readonly string CustomerInvalidInfoText = "CustomerInfo is not valid!";
        public static readonly string InternalServerErrorText = "Something unexpected went wrong during the request!";

        public static string GetCustomerNotFoundText(Guid id)
        {
            return $"Could find customer with Id of {id}";
        }
    }
}


