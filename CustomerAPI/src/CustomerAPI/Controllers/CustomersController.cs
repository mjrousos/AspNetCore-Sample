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
        private readonly CustomersDbContext _customersDbContext;

        public CustomersController(CustomersDbContext customersDbContext)
        {
            _customersDbContext = customersDbContext;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<CustomerEntity> Get()
        {
            return _customersDbContext.Customers;
        }

        // GET api/values/5
        [HttpGet("{Guid}")]
        public CustomerEntity Get(Guid id)
        {
            return _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]CustomerEntity customerInfo)
        {
            if (!_customersDbContext.ValidateCustomerEntityInfo(customerInfo))
            {
                return BadRequest("CustomerInfo is not valid!");
            }

            _customersDbContext.Customers.Add(customerInfo);
            _customersDbContext.SaveChanges();
            return new OkResult();
        }

        // PUT api/values/5
        [HttpPut("{Guid}")]
        public void Put(Guid id, [FromBody]CustomerUpdateInfo customerUpdateInfo)
        {
            var customer = _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer != null)
            {
                _customersDbContext.UpdateCustomer(customer, customerUpdateInfo);
                _customersDbContext.SaveChanges();
            }
        }

        // DELETE api/values/5
        [HttpDelete("{Guid}")]
        public void Delete(Guid id)
        {
            var customer = _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer != null)
            {
                _customersDbContext.Customers.Remove(customer);
                _customersDbContext.SaveChanges();
            }
        }
    }
}
