// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomersMVC.Customers
{
    [Table("Customers")]
    public class CustomerEntity
    {
        [Key]
        [Column("CustomerId")]
        public virtual Guid Id { get; set; }

        [Required]
        public virtual string FirstName { get; set; }

        [Required]
        public virtual string LastName { get; set; }

        [Required]
        public virtual string PhoneNumber { get; set; }

        public virtual string Address { get; set; }

        public virtual string City { get; set; }

        public virtual string State { get; set; }

        public virtual int ZipCode { get; set; }
    }
}
