using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Data;
using SOAP.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SOAP.Web.Controllers
{
    public class AdmissionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdmissionController> _logger;

        public AdmissionController(ApplicationDbContext context, ILogger<AdmissionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Main landing page for SOAP - School Selection with Placement Data Status
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Get schools with their placement data status
            var schoolsWithStatus = await _context.Schools
                .Where(s => s.IsActive)
                .Select(s => new {
                    School = s,
                    PlacementCount = _context.SchoolStudents.Count(ss => ss.SchoolId == s.Id),
                    HasPlacementData = _context.SchoolStudents.Any(ss => ss.SchoolId == s.Id)
                })
                .OrderBy(s => s.School.Name)
                .ToListAsync();
            
            ViewBag.SchoolsWithStatus = schoolsWithStatus;
            ViewBag.SchoolCount = schoolsWithStatus.Count;
            ViewBag.SchoolsWithData = schoolsWithStatus.Count(s => s.HasPlacementData);
            
            return View();
        }

        /// <summary>
        /// School-specific admission portal
        /// </summary>
        public async Task<IActionResult> School(int id)
        {
            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (school == null)
            {
                TempData["Error"] = "School not found or not accepting applications.";
                return RedirectToAction("Index");
            }

            // Get count of students placed at this school
            var placedStudentsCount = await _context.SchoolStudents
                .CountAsync(s => s.SchoolId == id);

            // Check if school has uploaded placement data
            var hasPlacementData = placedStudentsCount > 0;

            ViewBag.School = school;
            ViewBag.PlacedStudentsCount = placedStudentsCount;
            ViewBag.HasPlacementData = hasPlacementData;

            return View();
        }

        /// <summary>
        /// Verify KCPE Index Number for specific school
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerifyKcpe(string kcpeIndex, int schoolId)
        {
            if (string.IsNullOrWhiteSpace(kcpeIndex))
            {
                TempData["Error"] = "Please enter a valid KCPE Index Number";
                return RedirectToAction("School", new { id = schoolId });
            }

            // Verify school exists and is active
            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.Id == schoolId && s.IsActive);

            if (school == null)
            {
                TempData["Error"] = "Invalid school selection.";
                return RedirectToAction("Index");
            }

            // Check if school has uploaded any placement data
            var hasPlacementData = await _context.SchoolStudents
                .AnyAsync(s => s.SchoolId == schoolId);

            if (!hasPlacementData)
            {
                TempData["Error"] = $"{school.Name} has not yet uploaded their placement list. Please contact the school administration or try again later.";
                return RedirectToAction("School", new { id = schoolId });
            }

            // Check if student exists in this school's placement list
            var student = await _context.SchoolStudents
                .Include(s => s.School)
                .FirstOrDefaultAsync(s => s.KcpeIndexNumber == kcpeIndex.Trim() && s.SchoolId == schoolId);

            if (student == null)
            {
                // Store the attempted KCPE index for potential manual override
                TempData["AttemptedKcpeIndex"] = kcpeIndex.Trim();
                TempData["SchoolId"] = schoolId;
                TempData["SchoolName"] = school.Name;
                
                TempData["Error"] = $"KCPE Index Number not found in {school.Name}'s placement list.";
                TempData["ShowFallbackOptions"] = true;
                
                return RedirectToAction("School", new { id = schoolId });
            }

            // Check if student has already applied
            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.KcpeIndexNumber == kcpeIndex.Trim());

            if (existingApplication != null)
            {
                TempData["Info"] = $"Application already exists for {student.StudentName}. Please login to continue.";
                return RedirectToAction("Login", "Account", new { kcpeIndex = kcpeIndex });
            }

            // Store student info in TempData for the application process
            TempData["StudentName"] = student.StudentName;
            TempData["KcpeIndex"] = student.KcpeIndexNumber;
            TempData["SchoolName"] = student.School?.Name;
            TempData["SchoolId"] = student.SchoolId;
            TempData["KcpeScore"] = student.KcpeScore;

            TempData["Success"] = $"Welcome {student.StudentName}! You have been placed at {student.School?.Name}. Please proceed with your application.";
            
            // Redirect to parent registration/login
            return RedirectToAction("Register", "Account");
        }

        /// <summary>
        /// Manual override - proceed without KCPE verification (admin will review)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProceedManually(string kcpeIndex, int schoolId, string studentName, string parentPhone)
        {
            if (string.IsNullOrWhiteSpace(kcpeIndex) || string.IsNullOrWhiteSpace(studentName) || string.IsNullOrWhiteSpace(parentPhone))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("School", new { id = schoolId });
            }

            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.Id == schoolId && s.IsActive);

            if (school == null)
            {
                TempData["Error"] = "Invalid school selection.";
                return RedirectToAction("Index");
            }

            // Check if application already exists
            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.KcpeIndexNumber == kcpeIndex.Trim());

            if (existingApplication != null)
            {
                TempData["Info"] = $"Application already exists for this KCPE Index. Please login to continue.";
                return RedirectToAction("Login", "Account", new { kcpeIndex = kcpeIndex });
            }

            // Store manual entry info for application process
            TempData["StudentName"] = studentName.Trim();
            TempData["KcpeIndex"] = kcpeIndex.Trim();
            TempData["SchoolName"] = school.Name;
            TempData["SchoolId"] = schoolId;
            TempData["KcpeScore"] = 0; // Unknown score
            TempData["ManualEntry"] = true; // Flag for admin review
            TempData["ParentPhone"] = parentPhone.Trim();

            TempData["Warning"] = $"Your application will be flagged for manual review by {school.Name} administration. Please ensure your KCPE Index Number is correct.";
            
            return RedirectToAction("Register", "Account");
        }

        /// <summary>
        /// About SOAP system
        /// </summary>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Help and FAQ
        /// </summary>
        public IActionResult Help()
        {
            return View();
        }
    }
}