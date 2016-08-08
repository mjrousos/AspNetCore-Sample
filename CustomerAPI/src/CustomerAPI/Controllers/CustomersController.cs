using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CustomerAPI.Data.DataEntities;
using CustomerAPI.Data;


namespace CustomerAPI.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private CustomersDbContext _CustomersDbContext;

        public CustomersController(CustomersDbContext customersDbContext)
        {
            _CustomersDbContext = customersDbContext;
            _CustomersDbContext.Add(new CustomerEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Jon",
                LastName = "Jung",
                PhoneNumber = "425-213-2728"
            });
            _CustomersDbContext.SaveChanges();
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<CustomerEntity> Get()
        {
            return _CustomersDbContext.Customers.ToList();
        }

        // GET api/values/5
        [HttpGet("{Guid}")]
        public CustomerEntity Get(Guid id)
        {
            return _CustomersDbContext.Customers.FirstOrDefault(c => c.Id == id);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]CustomerEntity customerInfo)
        {
            if (!_CustomersDbContext.ValidateCustomerEntityInfo(customerInfo))
            {
                return BadRequest("CustomerInfo is not valid!");
            }

            _CustomersDbContext.Customers.Add(customerInfo);
            _CustomersDbContext.SaveChanges();
            return new OkResult();
        }

        // PUT api/values/5
        [HttpPut("{Guid}")]
        public void Put(Guid id, [FromBody]CustomerUpdateInfo customerUpdateInfo)
        {
            var customer = _CustomersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer != null)
            {
                _CustomersDbContext.UpdateCustomer(customer, customerUpdateInfo);
                _CustomersDbContext.SaveChanges();
            }
        }

        // DELETE api/values/5
        [HttpDelete("{Guid}")]
        public void Delete(Guid id)
        {
            var customer = _CustomersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer != null)
            {
                _CustomersDbContext.Customers.Remove(customer);
                _CustomersDbContext.SaveChanges();
            }
        }
    }
}
