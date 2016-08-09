using CustomersMVC.Customers;
using CustomersMVC.CustomersAPI;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var customersList = await _customersService.GetCustomersListAsync();
            return View(customersList);
        }

        [Route("[Controller]/[Action]")]
        public async Task<IActionResult> AddCustomer()
        {
            var customer = new CustomerEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Jon",
                LastName = "Smith",
                PhoneNumber = "555-555-5555"
            };

            await _customersService.AddCustomerAsync(customer);

            var customersList = await _customersService.GetCustomersListAsync();
            return View("CustomersList", customersList);
        }

        [Route("[Controller]/[Action]/{customerId}")]
        public async Task<IActionResult> DeleteCustomer(Guid customerId)
        {
            await _customersService.DeleteCustomerAsync(customerId);

            var customersList = await _customersService.GetCustomersListAsync();
            return View("CustomersList", customersList);
        }
    }
}
