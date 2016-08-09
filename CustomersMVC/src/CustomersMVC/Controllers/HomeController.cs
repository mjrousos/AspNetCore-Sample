using CustomersMVC.CustomersAPI;
using Microsoft.AspNetCore.Mvc;
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
    }
}
