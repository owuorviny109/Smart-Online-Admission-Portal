using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Areas.Admin.ViewModels;

namespace SOAP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalApplications = 0,
                PendingApplications = 0,
                ApprovedApplications = 0,
                RejectedApplications = 0
            };
            return View(viewModel);
        }
    }
}