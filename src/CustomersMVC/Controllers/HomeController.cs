// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersMVC.Customers;
using CustomersMVC.CustomersAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RequestCorrelation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CustomersMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly CustomersAPIService _customersService;
        private readonly IOptions<HomeControllerOptions> _homeControllerOptions;

        // Configuration: Here the HomeControllerOptions are dependency injected into the home controller. These options
        //                are then passed into the Index view to be used there as well.
        public HomeController(CustomersAPIService customersService, IOptions<HomeControllerOptions> homeControllerOptions)
        {
            _customersService = customersService;
            _homeControllerOptions = homeControllerOptions;
        }

        [Route("")]
        public IActionResult Index()
        {
            // Configuration: Here we are passing the IOptions<HomeControllerOptions> into the view to be used for the model in the view.
            //                This allows us to use whatever options and setting we want in a strongly typed way in the view as well
            //                as in the controller.
            return View("Index", _homeControllerOptions);
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
