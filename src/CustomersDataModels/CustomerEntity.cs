// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomersShared.Data.DataEntities
{
    [Table("Customers")]
    public class CustomerEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CustomerId")]
        public virtual Guid Id { get; set; }

        // Localization: Here we are using data annotations for the error messages. The localized resources are automatically
        //               looked up in the resources folder. The filename should be the namespace along with class.
        [Required(ErrorMessage = "FirstNameRequiredError")]
        public virtual string FirstName { get; set; }

        [Required(ErrorMessage = "LastNameRequiredError")]
        public virtual string LastName { get; set; }

        [Phone(ErrorMessage = "PhoneNumberInvalidError")]
        public virtual string PhoneNumber { get; set; }

        public virtual string Address { get; set; }

        public virtual string City { get; set; }

        public virtual string State { get; set; }

        public virtual int ZipCode { get; set; }

        /// <summary>
        /// Validates CustomerEntity to ensure that it meets the requirements
        /// </summary>
        public bool ValidateCustomerEntity()
        {
            return !(Id == Guid.Empty)
                && !string.IsNullOrEmpty(FirstName)
                && !string.IsNullOrEmpty(LastName);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var customerEntityPassedIn = obj as CustomerEntity;
            if (customerEntityPassedIn == null)
            {
                return false;
            }

            if (Id != customerEntityPassedIn.Id
                || string.CompareOrdinal(FirstName, customerEntityPassedIn.FirstName) != 0
                || string.CompareOrdinal(LastName, customerEntityPassedIn.LastName) != 0
                || string.CompareOrdinal(PhoneNumber, customerEntityPassedIn.PhoneNumber) != 0
                || string.CompareOrdinal(Address, customerEntityPassedIn.Address) != 0
                || string.CompareOrdinal(City, customerEntityPassedIn.City) != 0
                || string.CompareOrdinal(State, customerEntityPassedIn.State) != 0
                || ZipCode != customerEntityPassedIn.ZipCode)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = 0x2D2816FE;

            result = result * 31 + Id.GetHashCode();
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
