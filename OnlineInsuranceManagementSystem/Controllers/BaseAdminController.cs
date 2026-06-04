using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.ViewModels;
namespace OnlineInsuranceManagementSystem.Controllers 
{ public class BaseAdminController : Controller 
    { 
        protected readonly ApplicationDbContext _context; 
        public BaseAdminController(ApplicationDbContext context) 
        { _context = context; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                // ✅ Fetch fresh user data from DB on every request
                var user = _context.Users.Find(userId.Value);

                if (user != null)
                {
                    // ✅ Update session with latest profile image
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("ProfileImageUrl",
                        string.IsNullOrEmpty(user.ProfileImageUrl)
                            ? "/aassets/img/avatars/default.png"
                            : user.ProfileImageUrl);
                }
            }

            var unreadCount = _context.Messages.Count(m => !m.IsRead);
            ViewBag.UnreadMessageCount = unreadCount;

            base.OnActionExecuting(context);
        }


        // Controllers/BaseAdminController.cs - Profile() method

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound();

            var viewModel = new UserProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone ?? string.Empty,
                Address = user.Address,
                CNIC = user.CNIC,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                City = user.City,
                Country = user.Country,
                Role = user.Role,
                ProfileImageUrl = user.ProfileImageUrl,
                LastLoginDate = user.LastLoginDate,
                CreatedAt = user.CreatedAt  // ✅ Added for "Member Since"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model, IFormFile? profileImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || model.UserId != userId.Value)
                return BadRequest("Unauthorized");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound();

            // Update text fields
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.CNIC = model.CNIC;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.City = model.City;
            user.Country = model.Country;
            user.LastLoginDate = DateTime.Now;

            // Handle profile image upload
            if (profileImage != null && profileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(profileImage.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "Only JPG, PNG, GIF, and WebP images are allowed.";
                    return RedirectToAction("Profile");
                }

                if (profileImage.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    TempData["Error"] = "Image size must be less than 5MB.";
                    return RedirectToAction("Profile");
                }

                // Create uploads folder if not exists
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "aassets", "img", "avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Save relative path to database
                user.ProfileImageUrl = $"/aassets/img/avatars/{uniqueFileName}";
            }

            await _context.SaveChangesAsync();

            // ✅ CRITICAL FIX: Update session with ALL user data
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("ProfileImageUrl",
                string.IsNullOrEmpty(user.ProfileImageUrl)
                    ? "/aassets/img/avatars/default.png"
                    : user.ProfileImageUrl);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
    
}