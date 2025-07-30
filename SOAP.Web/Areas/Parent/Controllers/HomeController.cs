using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Areas.Parent.ViewModels;

namespace SOAP.Web.Areas.Parent.Controllers
{
    [Area("Parent")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}