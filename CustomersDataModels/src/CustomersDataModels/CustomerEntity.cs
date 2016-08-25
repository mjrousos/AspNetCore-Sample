using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace CustomersShared.Data.DataEntities
{
    [Table("Customers")]
    public class CustomerEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CustomerId")]
        public virtual Guid Id { get; set; }

        [Required]
        public virtual string FirstName { get; set; }

        [Required]
        public virtual string LastName { get; set; }

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

            var newObject = obj as CustomerEntity;
            if (obj == null)
            {
                return false;
            }

            var thisFields = this.GetType().GetTypeInfo().GetFields();
            var objFields = obj.GetType().GetTypeInfo().GetFields();

            if (thisFields.Length != objFields.Length)
            {
                return false;
            }

            for (int i = 0; i < thisFields.Length; i++)
            {
                if (thisFields[i] != objFields[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = 0x2D2816FE;

            foreach (var item in this.GetType().GetTypeInfo().GetFields())
            {
                result = result * 31 + (item == null ? 0 : item.GetHashCode());
            }

            return result;
        }
    }
}


