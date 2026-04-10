using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MotorcycleRepairShop.Services;

namespace MotorcycleRepairShop.Filters
{
    public class RoleAuthorizeAttribute : TypeFilterAttribute
    {
        public RoleAuthorizeAttribute(params string[] roles) 
            : base(typeof(RoleAuthorizeFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class RoleAuthorizeFilter : IAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly IAuthService _authService;

        public RoleAuthorizeFilter(string[] roles, IAuthService authService)
        {
            _roles = roles;
            _authService = authService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = _authService.GetCurrentRole();
            if (string.IsNullOrEmpty(userRole))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var allowedRoles = _roles.ToList();
            if (!allowedRoles.Contains(userRole) && userRole != "Admin") // Admin bypasses all
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
