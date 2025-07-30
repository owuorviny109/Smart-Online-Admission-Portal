using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Areas.Parent.ViewModels;

namespace SOAP.Web.Areas.Parent.Controllers
{
    [Area("Parent")]
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult VerifyPhone()
        {
            return View();
        }
    }
}