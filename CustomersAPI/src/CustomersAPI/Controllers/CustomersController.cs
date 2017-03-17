using CustomerAPI.Data;
using CustomersShared.Data.DataEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Threading.Tasks;

namespace CustomerAPI.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly ICustomersDataProvider _customersDataProvider;
        private readonly ResourceManager _resourceManager;

        // ASP.NET Core will automatically populate controller constructor arguments by resolving
        // services from the DI container. If needed, those objects will be created by calling constructors
        // whose own arguments will be provided by DI, and so on recursively until the whole object graph
        // needed has been constructed.
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
            CustomerDataActionResult customerDataActionResult = _customersDataProvider.TryFindCustomer(id);

            if (!customerDataActionResult.IsSuccess)
            {
                // HttpContext.RequestService can be used to resolve depdency-injected services
                // But receiving them via constructor injection is preferred.
                var resourceManager = HttpContext.RequestServices.GetService(typeof(ResourceManager)) as ResourceManager;

                return new NotFoundObjectResult(string.Format(resourceManager.GetString("CustomerNotFound"), id));
            }

            return Ok(customerDataActionResult.CustomerEntity);
        }

        // POST api/Customers
        [HttpPost]
        public async Task<ObjectResult> PostAsync([FromBody]CustomerDataTransferObject customerDataTransferObject, 
                                                  // Another way of requesting services from DI is [FromServices]
                                                  [FromServices] ResourceManager resManager)
        {
            if (customerDataTransferObject == null || !customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                return BadRequest(resManager.GetString("CustomerInfoInvalid"));
            }

            CustomerDataActionResult customerDataActionResult = 
                await _customersDataProvider.TryAddCustomerAsync(customerDataTransferObject);

            if (!customerDataActionResult.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  resManager.GetString("UnexpectedServerError"));
            }

            return Ok(customerDataActionResult.CustomerEntity);
        }

        // PUT api/Customers/5
        [HttpPut("{id}")]
        public async Task<ObjectResult> PutAsync(Guid id, [FromBody]CustomerDataTransferObject customerDataTransferObject)
        {
            if (customerDataTransferObject == null || !customerDataTransferObject.ValidateCustomerDataTransferObject())
            {
                return BadRequest(_resourceManager.GetString("CustomerInfoInvalid"));
            }

            if (!_customersDataProvider.CustomerExists(id))
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            CustomerDataActionResult customerDataActionResult = 
                    await _customersDataProvider.TryUpdateCustomerAsync(id, customerDataTransferObject);

            if (!customerDataActionResult.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }

            return Ok(customerDataActionResult.CustomerEntity);
        }

        // DELETE api/Customers/5
        [HttpDelete("{id}")]
        public async Task<ObjectResult> DeleteAsync(Guid id)
        {
            if (!_customersDataProvider.CustomerExists(id))
            {
                return new NotFoundObjectResult(string.Format(_resourceManager.GetString("CustomerNotFound"), id));
            }

            CustomerDataActionResult customerDataActionResult = await _customersDataProvider.TryDeleteCustomerAsync(id);
            if (!customerDataActionResult.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  _resourceManager.GetString("UnexpectedServerError"));
            }

            return Ok(customerDataActionResult.CustomerEntity);
        }
    }
}
