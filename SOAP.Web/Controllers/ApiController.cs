using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Data;

namespace SOAP.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("verify-kcpe")]
        public IActionResult VerifyKcpeNumber([FromBody] string kcpeNumber)
        {
            // TODO: Implement KCPE number verification
            return Ok(new { valid = true, studentName = "Sample Student" });
        }

        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] string phoneNumber)
        {
            // TODO: Implement OTP sending
            return Ok(new { sent = true });
        }
    }
}