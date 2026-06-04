using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using OnlineInsuranceManagementSystem.ViewModels;
using OnlineInsuranceManagementSystem.Services;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OnlineInsuranceManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ===== REGISTER CHOICE =====
        public IActionResult RegisterChoice()
        {
            return View();
        }

        // =====   REGISTER   =====
        public IActionResult Register(string role = "Customer")
        {
            ViewBag.Role = role;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Email = model.Email.Trim();
                model.FullName = model.FullName.Trim();

                var existingUser = _context.Users
                    .FirstOrDefault(x => x.Email == model.Email);

                if (existingUser != null)
                {
                    ViewBag.Message = "Email already exists";
                    return View(model);
                }

                if (!model.AcceptTerms)
                {
                    ModelState.AddModelError("AcceptTerms",
                        "You must accept the Terms & Conditions");

                    return View(model);
                }

                // ===== GET ROLE FROM FORM =====
                string role = Request.Form["role"];

                // ===== HASH PASSWORD =====
                string hashedPassword =
                    BCrypt.Net.BCrypt.HashPassword(model.Password);

                // ===== CREATE USER =====
                User user = new User()
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = hashedPassword,
                    Phone = model.Phone,

                    // CUSTOMER OR AGENT
                    Role = string.IsNullOrEmpty(role)
                                ? "Customer"
                                : role,

                    // AGENT = PENDING
                    // CUSTOMER = ACTIVE
                    Status = (role == "Agent") ? false : true,

                    AcceptTerms = model.AcceptTerms,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // ===== SUCCESS MESSAGE =====
                if (role == "Agent")
                {
                    TempData["Success"] =
                        "Agent registration submitted. Wait for admin approval.";
                }
                else
                {
                    TempData["Success"] =
                        "Registration Successful";
                }

                return RedirectToAction("Login");
            }

            return View(model);
        }

        // =====  LOGIN  =====
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            //  Show validation errors even if ModelState is invalid
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Please correct the errors below.";
                return View(model);
            }

            model.Email = model.Email.Trim();

            // Case-insensitive email lookup (matches ForgotPassword logic)
            var user = _context.Users.FirstOrDefault(x =>
                x.Email.ToLower() == model.Email.ToLower());

            if (user != null)
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
                if (isPasswordValid)
                {
                    if (user.Status == false)
                    {
                        ViewBag.Message = "Your account is disabled";
                        return View(model);
                    }

                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetInt32("UserId", user.UserId);

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (user.Role == "Agent")
                    {
                        return RedirectToAction("Dashboard", "Agent");
                    }
                    else if (user.Role == "Customer")
                    {
                        return RedirectToAction("Dashboard", "Customer");
                    }

                    return RedirectToAction("Login");
                }
                else
                {
                    // Explicit invalid password message
                    ViewBag.Message = "Invalid email or password";
                }
            }
            else
            {
                // Explicit user not found message (same as invalid password for security)
                ViewBag.Message = "Invalid email or password";
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(ContactMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ContactError"] = "true";
                TempData["Error"] = "Please fill all required fields correctly.";

                return Redirect("/#contact");
            }

            var message = new Message
            {
                SenderId = 1,
                ReceiverId = 1,

                FullName = model.FullName!.Trim(),
                Email = model.Email!.Trim(),
                Subject = model.Subject!.Trim(),
                MessageText = model.Message!.Trim(),

                SentDate = DateTime.Now
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            TempData["Success"] = "Message sent successfully!";

            return Redirect("/#contact");
        }

        public IActionResult ServiceRedirect(string service)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            else if (role == "Agent")
                return RedirectToAction("AgentDashboard", "Agent");

            else
                return RedirectToAction("UserDashboard", "User");
        }

        public IActionResult GetStarted()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Register", "Account");

            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Admin")
                return RedirectToAction("Dashboard", "Admin");
            else if (role == "Agent")
                return RedirectToAction("AgentDashboard", "Agent");
            else
                return RedirectToAction("UserDashboard", "User");
        }

        // ===== FORGOT PASSWORD METHODS =====

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = model.Email.Trim().ToLower();
                var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email);

                // Security: Always show same message to prevent email enumeration
                if (user != null && user.Status)
                {
                    // Generate cryptographically secure token (64-char hex string)
                    var token = GenerateSecureToken();

                    // Store hashed token + expiry (1 hour) - FIXED: Use BCrypt.Net.BCrypt
                    user.ResetToken = BCrypt.Net.BCrypt.HashPassword(token);
                    user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    // Generate reset link (URL-safe token)
                    var resetLink = Url.Action("ResetPassword", "Account",
                        new { token, email = user.Email },
                        Request.Scheme);

                    // Send professional email
                    await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);
                }

                TempData["Success"] = "If an account with that email exists, we've sent a password reset link.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ===== RESET PASSWORD METHODS =====

        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u =>
                    u.Email.ToLower() == model.Email.Trim().ToLower());

                // Validate user exists + token is valid + not expired
                if (user == null ||
                    string.IsNullOrEmpty(user.ResetToken) ||
                    user.ResetTokenExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError(string.Empty, "Invalid or expired reset token. Please request a new one.");
                    return View(model);
                }

                // Verify token (BCrypt compare) - FIXED: Use BCrypt.Net.BCrypt
                if (!BCrypt.Net.BCrypt.Verify(model.Token, user.ResetToken))
                {
                    ModelState.AddModelError(string.Empty, "Invalid or expired reset token. Please request a new one.");
                    return View(model);
                }

                // Token valid 
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.ResetToken = null;
                user.ResetTokenExpiry = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Your password has been reset successfully. Please login with your new password.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // Helper: Generate 32-byte cryptographically secure token (hex encoded = 64 chars)
        private string GenerateSecureToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}

