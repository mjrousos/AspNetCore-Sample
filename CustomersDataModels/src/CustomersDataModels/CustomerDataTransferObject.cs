using System.ComponentModel.DataAnnotations;
using System.Reflection;

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

            var newObject = obj as CustomerDataTransferObject;
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


