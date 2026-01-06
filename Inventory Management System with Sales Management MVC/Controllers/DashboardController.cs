using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System_with_Sales_Management_MVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
