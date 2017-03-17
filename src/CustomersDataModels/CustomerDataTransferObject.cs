using System.ComponentModel.DataAnnotations;

namespace CustomersShared.Data.DataEntities
{
    public class CustomerDataTransferObject
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

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
            int result = 0x2D2816FE;

            result = result * 31 + (FirstName == null ? 0 : FirstName.GetHashCode());
            result = result * 31 + (LastName == null ? 0 : LastName.GetHashCode());
            result = result * 31 + (PhoneNumber == null ? 0 : PhoneNumber.GetHashCode());
            result = result * 31 + (Address == null ? 0 : Address.GetHashCode());
            result = result * 31 + (City == null ? 0 : City.GetHashCode());
            result = result * 31 + (State == null ? 0 : State.GetHashCode());
            result = result * 31 + (ZipCode == 0 ? 0 : ZipCode.GetHashCode());

            return result;
        }
    }
}


