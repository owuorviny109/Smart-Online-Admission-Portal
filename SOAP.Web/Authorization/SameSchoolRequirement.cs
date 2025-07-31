using Microsoft.AspNetCore.Authorization;

namespace SOAP.Web.Authorization
{
    public class SameSchoolRequirement : IAuthorizationRequirement
    {
        public SameSchoolRequirement()
        {
        }
    }
}