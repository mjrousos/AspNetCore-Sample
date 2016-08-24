using System.ComponentModel.DataAnnotations;

namespace CustomersShared.Data.DataEntities
{
    public class UpdateableCustomerInfo
    {
        [Required]
        public string FirstName {get; set;}

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public int ZipCode { get; set; }
    }
}


