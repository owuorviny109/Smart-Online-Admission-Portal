using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Areas.Admin.ViewModels;

namespace SOAP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ApplicationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Review(int id)
        {
            var viewModel = new ApplicationReviewViewModel
            {
                ApplicationId = id
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            // TODO: Implement approval logic
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Reject(int id, string reason)
        {
            // TODO: Implement rejection logic
            return RedirectToAction(nameof(Index));
        }
    }
}