// Controllers/BaseCustomerController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace OnlineInsuranceManagementSystem.Controllers
{
    [Authorize(Roles = "Customer,User")]
    public abstract class BaseCustomerController : Controller
    {
        protected int GetLoggedInUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("UserId");
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        protected bool IsCustomerAuthenticated()
        {
            return User.Identity?.IsAuthenticated == true &&
                   (User.IsInRole("Customer") || User.IsInRole("User"));
        }
    }
}