// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersMVC.Customers;
using CustomersMVC.CustomersAPI;
using Microsoft.AspNetCore.Mvc;
using RequestCorrelation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CustomersMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly CustomersAPIService _customersService;

        public HomeController(CustomersAPIService customersService)
        {
            _customersService = customersService;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> CustomersList()
        {
            SetCorrelationId();

            var customersList = await _customersService.GetCustomersListAsync();

            return View(customersList);
        }

        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> AddCustomer()
        {
            SetCorrelationId();

            var customer = new CustomerEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Jon",
                LastName = "Smith",
                PhoneNumber = "555-555-5555"
            };

            await _customersService.AddCustomerAsync(customer);

            return RedirectToAction("CustomersList");
        }

        [Route("[Controller]/[Action]/{customerId}")]
        public async Task<IActionResult> DeleteCustomer(Guid customerId)
        {
            SetCorrelationId();

            await _customersService.DeleteCustomerAsync(customerId);

            return RedirectToAction("CustomersList");
        }

        // If the request has a correlation ID, register it with the HTTP client (to be included in outgoing requests)
        private void SetCorrelationId() =>
            _customersService.CorrelationId = HttpContext?.Request.Headers
                                              .First(h => h.Key.Equals(RequestCorrelationMiddleware.CorrelationHeaderName, StringComparison.OrdinalIgnoreCase))
                                              .Value.FirstOrDefault();
    }
}
