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

        // GET: api/Customers
        [HttpGet]
        public IEnumerable<CustomerEntity> Get()
        {
            return _customersDbContext.Customers;
        }

        // GET api/Customers/5
        [HttpGet("{id}")]
        public CustomerEntity Get(Guid id)
        {
            return _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);
        }

        // POST api/Customers
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

        // PUT api/Customers/5
        [HttpPut("{id}")]
        public void Put(Guid id, [FromBody]CustomerUpdateInfo customerUpdateInfo)
        {
            var customer = _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer != null)
            {
                _customersDbContext.UpdateCustomer(customer, customerUpdateInfo);
                _customersDbContext.SaveChanges();
            }
        }

        // DELETE api/Customers/5
        [HttpDelete("{id}")]
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
