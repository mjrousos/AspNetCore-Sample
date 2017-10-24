// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using CustomersMVC.CustomersAPI;
using CustomersShared.Data.DataEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<HomeController> _localizer;

        // Configuration: Here the HomeControllerOptions are dependency injected into the home controller. These options
        //                are then passed into the Index view to be used there as well.
        //
        // Localization: Here we are dependency injecting the IStringLocalizer for the HomeController. This will be used for
        //               localizing resources in the HomeController.
        public HomeController(CustomersAPIService customersService,
                              IOptions<HomeControllerOptions> homeControllerOptions,
                              IStringLocalizer<HomeController> localizer)
        {
            _customersService = customersService;
            _homeControllerOptions = homeControllerOptions;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Configuration: Here we are passing the IOptions<HomeControllerOptions> into the view to be used for the model in the view.
            //                This allows us to use whatever options and setting we want in a strongly typed way in the view as well
            //                as in the controller.
            return View("Index", _homeControllerOptions);
        }

        // Localization: This method adds the localized ping message from the localizer into the ViewData. In
        //               this case we are demonstrating that there is no .resx file for the resources, but the
        //               localizer still returns a message. Once a .resx file exists it will replace the
        //               text with the text from the "PingMessage" inside the .resx for the request culture.
        [HttpGet]
        public IActionResult Ping()
        {
            // Localization: The localized string will have a value of "PingMessage" since
            //               the resource name is used as a default value if no resource with
            //               an appropriate name is found.
            ViewData["PingMessage"] = _localizer["PingMessage"].Value;
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CustomersList()
        {
            SetCorrelationId();

            var customersList = await _customersService.GetCustomersListAsync();

            return View(customersList);
        }

        [HttpGet]
        public IActionResult AddCustomer()
        {
            SetCorrelationId();

            return View(new CustomerDataTransferObject());
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerDataTransferObject customer)
        {
            SetCorrelationId();

            if (ModelState.IsValid)
            {
                await _customersService.AddCustomerAsync(customer);
                return RedirectToAction(nameof(CustomersList));
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Delete(Guid? id)
        {
            SetCorrelationId();

            if (id == null)
            {
                return NotFound();
            }

            return View(new CustomerEntity { Id = id.Value });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            SetCorrelationId();

            await _customersService.DeleteCustomerAsync(id);

            return RedirectToAction(nameof(CustomersList));
        }

        // If the request has a correlation ID, register it with the HTTP client (to be included in outgoing requests)
        private void SetCorrelationId() =>
            _customersService.CorrelationId = HttpContext?.Request.Headers
                                              .First(h => h.Key.Equals(RequestCorrelationMiddleware.CorrelationHeaderName, StringComparison.OrdinalIgnoreCase))
                                              .Value.FirstOrDefault();
    }
}
