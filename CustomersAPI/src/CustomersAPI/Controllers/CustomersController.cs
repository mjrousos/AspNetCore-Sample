using CustomerAPI.Data;
using CustomersShared.Data.DataEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace CustomerAPI.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly CustomersDbContext _customersDbContext;
        private readonly ResourceManager _resourceManager; 

        public CustomersController(CustomersDbContext customersDbContext, ResourceManager resourceManager)
        {
            _customersDbContext = customersDbContext;
            _resourceManager = resourceManager;
        }

        // GET: api/Customers
        [HttpGet]
        public IEnumerable<CustomerEntity> Get()
        {
            return _customersDbContext.Customers;
        }

        // GET api/Customers/5
        [HttpGet("{id}")]
        public ObjectResult Get(Guid id)
        {
            var customer = _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            return new OkObjectResult(customer);
        }

        // POST api/Customers
        [HttpPost]
        public ObjectResult Post([FromBody]UpdateableCustomerInfo customerInfo)
        {
            if (!_customersDbContext.ValidateUpdateableCustomerInfo(customerInfo))
            {
                return BadRequest(_resourceManager.GetString("CustomerInfoInvalid"));
            }

            try
            {
                var customer = _customersDbContext.AddCustomer(customerInfo);

                if (customer == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                                      _resourceManager.GetString("UnexpectedServerError"));
                }

                return new OkObjectResult(customer);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }
        }

        // PUT api/Customers/5
        [HttpPut("{id}")]
        public ObjectResult Put(Guid id, [FromBody]UpdateableCustomerInfo customerUpdateInfo)
        {
            if (!_customersDbContext.ValidateUpdateableCustomerInfo(customerUpdateInfo))
            {
                return BadRequest(_resourceManager.GetString("CustomerInfoInvalid"));
            }

            var customer = _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            try
            {
                _customersDbContext.UpdateCustomerInfo(customer, customerUpdateInfo);
                _customersDbContext.SaveChanges();
                return new OkObjectResult(customer);
            }
            catch
            {
               return StatusCode(StatusCodes.Status500InternalServerError, null);
            }
        }

        // DELETE api/Customers/5
        [HttpDelete("{id}")]
        public ObjectResult Delete(Guid id)
        {
            var customer = _customersDbContext.Customers.FirstOrDefault(c => c.Id == id);

            if (customer == null)
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            try
            {
                _customersDbContext.Customers.Remove(customer);
                _customersDbContext.SaveChanges();
                return new OkObjectResult(customer);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }
        }
    }
}
