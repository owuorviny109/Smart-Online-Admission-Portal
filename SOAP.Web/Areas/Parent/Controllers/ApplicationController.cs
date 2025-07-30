using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Areas.Parent.ViewModels;

namespace SOAP.Web.Areas.Parent.Controllers
{
    [Area("Parent")]
    public class ApplicationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Status()
        {
            return View();
        }

        public IActionResult VerifyKcpe()
        {
            return View();
        }
    }
}