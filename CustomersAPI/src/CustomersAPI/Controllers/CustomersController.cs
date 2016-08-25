using CustomerAPI.Data;
using CustomersShared.Data.DataEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Resources;

namespace CustomerAPI.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly ICustomersDataProvider _customersDataProvider;
        private readonly ResourceManager _resourceManager;

        public CustomersController(ICustomersDataProvider customersDataProvider, ResourceManager resourceManager)
        {
            _customersDataProvider = customersDataProvider;
            _resourceManager = resourceManager;
        }

        // GET: api/Customers
        [HttpGet]
        public IEnumerable<CustomerEntity> Get()
        {
            return _customersDataProvider.GetCustomers();
        }

        // GET api/Customers/5
        [HttpGet("{id}")]
        public ObjectResult Get(Guid id)
        {
            CustomerEntity customer;

            if (!_customersDataProvider.TryFindCustomer(id, out customer))
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            return Ok(customer);
        }

        // POST api/Customers
        [HttpPost]
        public ObjectResult Post([FromBody]CustomerDataTransferObject customerDataTransferObject)
        {
            if (customerDataTransferObject == null || !customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                return BadRequest(_resourceManager.GetString("CustomerInfoInvalid"));
            }

            CustomerEntity customerAdded;
            if (!_customersDataProvider.TryAddCustomer(customerDataTransferObject, out customerAdded))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }

            return Ok(customerAdded);
        }

        // PUT api/Customers/5
        [HttpPut("{id}")]
        public ObjectResult Put(Guid id, [FromBody]CustomerDataTransferObject customerDataTransferObject)
        {
            if (customerDataTransferObject == null || !customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                return BadRequest(_resourceManager.GetString("CustomerInfoInvalid"));
            }

            if (!_customersDataProvider.CustomerExists(id))
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            CustomerEntity customerUpdated;
            if (!_customersDataProvider.TryUpdateCustomer(id, customerDataTransferObject, out customerUpdated))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }

            return Ok(customerUpdated);
        }

        // DELETE api/Customers/5
        [HttpDelete("{id}")]
        public ObjectResult Delete(Guid id)
        {
            if (!_customersDataProvider.CustomerExists(id))
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            CustomerEntity customerDeleted;
            if (!_customersDataProvider.TryDeleteCustomer(id, out customerDeleted))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }

            return Ok(customerDeleted);
        }
    }
}
