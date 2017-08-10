// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.ComponentModel.DataAnnotations;

namespace CustomersShared.Data.DataEntities
{
    public class CustomerDataTransferObject
    {
        // Localization: Here we are using data annotations for the error messages. The localized resources are automatically
        //               looked up in the resources folder. The filename should be the namespace along with class.
        [Required(ErrorMessage = "FirstNameRequiredError")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastNameRequiredError")]
        public string LastName { get; set; }

        [Phone(ErrorMessage = "PhoneNumberInvalidError")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public int ZipCode { get; set; }

        /// <summary>
        /// Validates customerDataTransferObject to ensure that it meets the requirements
        /// </summary>
        public bool ValidateCustomerDataTransferObject()
        {
            return !string.IsNullOrEmpty(FirstName)
                && !string.IsNullOrEmpty(LastName);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var customerDTOPassedIn = obj as CustomerDataTransferObject;
            if (customerDTOPassedIn == null)
            {
                return false;
            }

            if (string.CompareOrdinal(FirstName, customerDTOPassedIn.FirstName) != 0
                || string.CompareOrdinal(LastName, customerDTOPassedIn.LastName) != 0
                || string.CompareOrdinal(PhoneNumber, customerDTOPassedIn.PhoneNumber) != 0
                || string.CompareOrdinal(Address, customerDTOPassedIn.Address) != 0
                || string.CompareOrdinal(City, customerDTOPassedIn.City) != 0
                || string.CompareOrdinal(State, customerDTOPassedIn.State) != 0
                || ZipCode != customerDTOPassedIn.ZipCode)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = 0x2D2816FE;

            result = result * 31 + FirstName?.GetHashCode() ?? 0;
            result = result * 31 + LastName?.GetHashCode() ?? 0;
            result = result * 31 + PhoneNumber?.GetHashCode() ?? 0;
            result = result * 31 + Address?.GetHashCode() ?? 0;
            result = result * 31 + City?.GetHashCode() ?? 0;
            result = result * 31 + State?.GetHashCode() ?? 0;
            result = result * 31 + (ZipCode == 0 ? 0 : ZipCode.GetHashCode());

            return result;
        }
    }
}
