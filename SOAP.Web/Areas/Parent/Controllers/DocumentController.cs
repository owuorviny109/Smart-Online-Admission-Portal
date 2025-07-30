using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Areas.Parent.ViewModels;

namespace SOAP.Web.Areas.Parent.Controllers
{
    [Area("Parent")]
    public class DocumentController : Controller
    {
        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult View(int id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(DocumentUploadViewModel model)
        {
            return View(model);
        }
    }
}