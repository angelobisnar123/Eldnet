using Microsoft.AspNetCore.Mvc;

namespace WebApplication1_Test1.Controllers
{
    public class Controller1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
