using Microsoft.AspNetCore.Authorization;

namespace SOAP.Web.Authorization
{
    public class ParentDataAccessRequirement : IAuthorizationRequirement
    {
        public ParentDataAccessRequirement()
        {
        }
    }
}