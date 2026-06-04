using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using OnlineInsuranceManagementSystem.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;  
using System.Threading.Tasks;
using ClaimModel = OnlineInsuranceManagementSystem.Models.Claim;

namespace OnlineInsuranceManagementSystem.Controllers
{
    public class CustomerController : BaseAdminController
    {
        public CustomerController(ApplicationDbContext context) : base(context)
        {
            // _context is already initialized by base class
        }

        public async Task<IActionResult> Dashboard()
        {
            // ✅ Session se user check karein
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // ✅ Agar session invalid hai to login par bhej dein
            if (!userIdSession.HasValue || userRole != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = userIdSession.Value;

            var viewModel = new CustomerDashboardViewModel();
            var today = DateTime.Now;
            var sixMonthsAgo = today.AddMonths(-6);

            try
            {
                // ==================== SUMMARY CARDS ====================

                viewModel.ActivePoliciesCount = await _context.Applications
                    .Where(a => a.UserId == userId && a.Status == "Approved")
                    .Join(_context.Policies.Where(p => p.Status),
                        a => a.PolicyId,
                        p => p.PolicyId,
                        (a, p) => a)
                    .CountAsync();

                var appQuery = _context.Applications.Where(a => a.UserId == userId);
                viewModel.TotalApplications = await appQuery.CountAsync();
                viewModel.ApprovedApplications = await appQuery.CountAsync(a => a.Status == "Approved");
                viewModel.PendingApplications = await appQuery.CountAsync(a => a.Status == "Pending");
                viewModel.RejectedApplications = await appQuery.CountAsync(a => a.Status == "Rejected");

                var claimQuery = _context.Claims.Where(c => c.UserId == userId);
                viewModel.TotalClaims = await claimQuery.CountAsync();
                viewModel.PendingClaims = await claimQuery.CountAsync(c => c.Status == "Pending");
                viewModel.ApprovedClaims = await claimQuery.CountAsync(c => c.Status == "Approved");
                viewModel.RejectedClaims = await claimQuery.CountAsync(c => c.Status == "Rejected");

                var paymentQuery = _context.Payments.Where(p => p.UserId == userId);
                viewModel.TotalPayments = await paymentQuery.CountAsync();
                viewModel.TotalPaidAmount = await paymentQuery
                    .Where(p => p.Status == "Paid")
                    .SumAsync(p => p.Amount);

                var loanQuery = _context.Loans.Where(l => l.UserId == userId);
                viewModel.LoanRequests = await loanQuery.CountAsync();
                viewModel.PendingLoans = await loanQuery.CountAsync(l => l.Status == "Pending");
                viewModel.ApprovedLoans = await loanQuery.CountAsync(l => l.Status == "Approved");

                var msgQuery = _context.Messages.Where(m => m.ReceiverId == userId);
                viewModel.MessagesCount = await msgQuery.CountAsync();
                viewModel.UnreadMessagesCount = await msgQuery.CountAsync(m => !m.IsRead);

                // ==================== CHART DATA ====================

                viewModel.ApplicationsByStatusData = await appQuery
                    .GroupBy(a => a.Status)
                    .Select(g => new CustomerChartSeriesData
                    {
                        Label = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Color = GetStatusColor(g.Key)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                viewModel.ClaimsByStatusData = await claimQuery
                    .GroupBy(c => c.Status)
                    .Select(g => new CustomerChartSeriesData
                    {
                        Label = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Amount = g.Sum(x => x.ClaimAmount),
                        Color = GetStatusColor(g.Key)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var rawPaymentsData = await _context.Payments
                    .Where(p => p.UserId == userId && p.PaymentDate >= sixMonthsAgo && p.Status == "Paid")
                    .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Amount = g.Sum(x => x.Amount)
                    })
                    .ToListAsync();

                viewModel.MonthlyPaymentsData = rawPaymentsData
                    .Select(d => new CustomerChartSeriesData
                    {
                        Label = $"{GetMonthName(d.Month)} {d.Year}",
                        Amount = d.Amount
                    })
                    .OrderBy(x => ParseMonthYear(x.Label))
                    .ToList();

                viewModel.MonthlyPaymentsData = FillMissingMonths(viewModel.MonthlyPaymentsData, sixMonthsAgo, today);

                viewModel.PolicyDistributionData = await _context.Applications
                    .Where(a => a.UserId == userId && a.Status == "Approved")
                    .Join(_context.Policies.Include(p => p.InsuranceType),
                        a => a.PolicyId,
                        p => p.PolicyId,
                        (a, p) => new { a, p })
                    .GroupBy(x => x.p.InsuranceType.Name)
                    .Select(g => new CustomerChartSeriesData
                    {
                        Label = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Color = GetRandomColor(g.Key)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                viewModel.LoanStatusData = await loanQuery
                    .GroupBy(l => l.Status)
                    .Select(g => new CustomerChartSeriesData
                    {
                        Label = g.Key ?? "Unknown",
                        Count = g.Count(),
                        Amount = g.Sum(x => x.Amount),
                        Color = GetStatusColor(g.Key)
                    })
                    .ToListAsync();

                // ==================== RECENT ACTIVITY ====================

                viewModel.RecentApplications = await _context.Applications
                    .Where(a => a.UserId == userId)
                    .Include(a => a.Policy).ThenInclude(p => p.InsuranceType)
                    .OrderByDescending(a => a.ApplyDate)
                    .Take(5)
                    .Select(a => new CustomerRecentApplicationItem
                    {
                        ApplicationId = a.ApplicationId,
                        PolicyName = a.Policy.PolicyName,
                        InsuranceType = a.Policy.InsuranceType.Name,
                        ApplyDate = a.ApplyDate,
                        Status = a.Status ?? "Pending",
                        PremiumAmount = a.Policy.PremiumAmount
                    })
                    .ToListAsync();

                viewModel.RecentClaims = await _context.Claims
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Policy)
                    .OrderByDescending(c => c.ClaimDate)
                    .Take(5)
                    .Select(c => new CustomerRecentClaimItem
                    {
                        ClaimId = c.ClaimId,
                        PolicyName = c.Policy.PolicyName,
                        ClaimAmount = c.ClaimAmount,
                        Status = c.Status ?? "Pending",
                        ClaimDate = c.ClaimDate,
                        Reason = c.Reason != null && c.Reason.Length > 50 ? c.Reason.Substring(0, 50) + "..." : (c.Reason ?? "")
                    })
                    .ToListAsync();

                viewModel.RecentPayments = await _context.Payments
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Policy)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .Select(p => new CustomerRecentPaymentItem
                    {
                        PaymentId = p.PaymentId,
                        PolicyName = p.Policy.PolicyName,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod ?? "Unknown",
                        PaymentDate = p.PaymentDate,
                        TransactionId = p.TransactionId ?? ""
                    })
                    .ToListAsync();

                viewModel.RecentMessages = await _context.Messages
                    .Where(m => m.ReceiverId == userId)
                    .OrderByDescending(m => m.SentDate)
                    .Take(5)
                    .Select(m => new CustomerRecentMessageItem
                    {
                        MessageId = m.MessageId,
                        SenderName = m.FullName ?? "System",
                        Subject = m.Subject ?? "(No Subject)",
                        SentDate = m.SentDate,
                        IsRead = m.IsRead,
                        Preview = m.MessageText != null && m.MessageText.Length > 40 ? m.MessageText.Substring(0, 40) + "..." : (m.MessageText ?? "")
                    })
                    .ToListAsync();

                viewModel.RecentPolicies = await _context.Applications
                    .Where(a => a.UserId == userId && a.Status == "Approved")
                    .Include(a => a.Policy).ThenInclude(p => p.InsuranceType)
                    .OrderByDescending(a => a.ApplyDate)
                    .Take(5)
                    .Select(a => new CustomerRecentPolicyItem
                    {
                        PolicyId = a.PolicyId,
                        PolicyName = a.Policy.PolicyName,
                        InsuranceType = a.Policy.InsuranceType.Name,
                        CoverageAmount = a.Policy.CoverageAmount,
                        StartDate = a.ApplyDate,
                        EndDate = a.ApplyDate.AddMonths(a.Policy.Duration),
                        Status = a.Policy.Status ? "Active" : "Inactive"
                    })
                    .ToListAsync();

                ViewBag.UnreadMessageCount = viewModel.UnreadMessagesCount;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load dashboard. Please try again.";
                return View(viewModel);
            }
        }

        // ==================== HELPER METHODS ====================

        private string GetMonthName(int monthNumber)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthNumber);
        }

        private DateTime ParseMonthYear(string monthYear)
        {
            return DateTime.ParseExact(monthYear, "MMM yyyy", CultureInfo.InvariantCulture);
        }

        private string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "approved" or "paid" or "active" => "#198754",
                "pending" or "processing" => "#ffc107",
                "rejected" or "failed" => "#dc3545",
                "cancelled" => "#6c757d",
                _ => "#0d6efd"
            };
        }

        private string GetRandomColor(string seed)
        {
            var colors = new[] { "#0A3D62", "#1B4F72", "#21618C", "#2874A6", "#2E86C1", "#3498DB" };
            return colors[Math.Abs(seed.GetHashCode()) % colors.Length];
        }

        private List<CustomerChartSeriesData> FillMissingMonths(
            List<CustomerChartSeriesData> data,
            DateTime startDate,
            DateTime endDate)
        {
            var result = new List<CustomerChartSeriesData>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                var monthLabel = $"{GetMonthName(current.Month)} {current.Year}";
                var existing = data.FirstOrDefault(d => d.Label == monthLabel);
                result.Add(existing ?? new CustomerChartSeriesData { Label = monthLabel, Amount = 0 });
                current = current.AddMonths(1);
            }

            return result.OrderBy(x => ParseMonthYear(x.Label)).ToList();
        }

        // ✅ FIXED: Use Session instead of Claims (consistent with Dashboard)
        private int GetLoggedInCustomerId()
        {
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userIdSession.HasValue && userRole == "Customer")
                return userIdSession.Value;

            return 0;
        }

        // GET: Customer/Policies
        [HttpGet]
        public async Task<IActionResult> Policies()
        {
            try
            {
                var customerId = GetLoggedInCustomerId();
                if (customerId == 0)
                    return RedirectToAction("Login", "Account");

                // Fetch all ACTIVE policies with insurance type
                var policies = await _context.Policies
                    .Where(p => p.Status)
                    .Include(p => p.InsuranceType)
                    .OrderBy(p => p.PolicyName)
                    .ToListAsync();

                // Fetch customer's existing applications
                var customerApplications = await _context.Applications
                    .Where(a => a.UserId == customerId)
                    .Select(a => new { a.PolicyId, a.ApplicationId, a.Status, a.ApplyDate })
                    .ToListAsync();

                // Project to ViewModel
                var viewModelList = policies.Select(p => new CustomerPoliciesViewModel
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    InsuranceTypeId = p.InsuranceTypeId,
                    InsuranceTypeName = p.InsuranceType?.Name ?? "Unknown",
                    PremiumAmount = p.PremiumAmount,
                    Duration = p.Duration,
                    CoverageAmount = p.CoverageAmount,
                    Terms = p.Terms,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    HasApplied = customerApplications.Any(a => a.PolicyId == p.PolicyId),
                    ApplicationStatus = customerApplications.FirstOrDefault(a => a.PolicyId == p.PolicyId)?.Status,
                    ApplicationId = customerApplications.FirstOrDefault(a => a.PolicyId == p.PolicyId)?.ApplicationId,
                    AppliedDate = customerApplications.FirstOrDefault(a => a.PolicyId == p.PolicyId)?.ApplyDate
                }).ToList();

                var insuranceTypes = await _context.InsuranceTypes
                    .Select(t => new InsuranceTypeViewModel
                    {
                        InsuranceTypeId = t.InsuranceTypeId,
                        Name = t.Name
                    })
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                var viewModel = new CustomerPoliciesIndexViewModel
                {
                    Policies = viewModelList,
                    InsuranceTypes = insuranceTypes,
                    TotalAvailablePolicies = viewModelList.Count,
                    TotalAppliedPolicies = viewModelList.Count(p => p.HasApplied),
                    PendingApplications = viewModelList.Count(p => p.ApplicationStatus == "Pending"),
                    ApprovedApplications = viewModelList.Count(p => p.ApplicationStatus == "Approved")
                };

                return View(viewModel);
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Database error while loading policies. Please try again later.";
                return View(new CustomerPoliciesIndexViewModel());
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred. Please contact support.";
                return View(new CustomerPoliciesIndexViewModel());
            }
        }

        // POST: Customer/ApplyPolicy
        // POST: Customer/ApplyPolicy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPolicy(ApplyPolicyViewModel model)
        {
            try
            {
                // 🔐 Get logged-in customer from Session (consistent with Dashboard)
                var customerId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (!customerId.HasValue || userRole != "Customer")
                {
                    return Json(new { success = false, message = "Please login to apply." });
                }

                // 🎯 DEBUG: Log received PolicyId
                System.Diagnostics.Debug.WriteLine($"[ApplyPolicy] Received PolicyId: {model.PolicyId}, UserId: {customerId}");

                // ✅ Validation: Policy must exist AND be Active (Status = true)
                var policy = await _context.Policies
                    .AsNoTracking()  // 🚀 Performance: No tracking needed for read
                    .FirstOrDefaultAsync(p => p.PolicyId == model.PolicyId && p.Status);

                if (policy == null)
                {
                    // 🎯 DEBUG: Log why policy wasn't found
                    var policyCheck = await _context.Policies
                        .FirstOrDefaultAsync(p => p.PolicyId == model.PolicyId);

                    if (policyCheck == null)
                        System.Diagnostics.Debug.WriteLine($"[ApplyPolicy] Policy {model.PolicyId} does not exist");
                    else if (!policyCheck.Status)
                        System.Diagnostics.Debug.WriteLine($"[ApplyPolicy] Policy {model.PolicyId} exists but is Inactive");

                    return Json(new { success = false, message = "Policy not found or inactive." });
                }

                // 🚫 Prevent duplicate applications
                var existingApplication = await _context.Applications
                    .FirstOrDefaultAsync(a => a.PolicyId == model.PolicyId && a.UserId == customerId);

                if (existingApplication != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"You have already applied for this policy. Status: {existingApplication.Status}",
                        applicationId = existingApplication.ApplicationId
                    });
                }

                // 💾 Create and save new application
                var application = new Application
                {
                    UserId = customerId.Value,      // ✅ From Session (secure)
                    PolicyId = model.PolicyId,      // ✅ From validated form
                    ApplyDate = DateTime.Now,
                    Status = "Pending",             // ✅ Default status
                    Remarks = string.Empty
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"[ApplyPolicy] Application {application.ApplicationId} created successfully");

                return Json(new
                {
                    success = true,
                    message = "Application submitted successfully! Status: Pending",
                    applicationId = application.ApplicationId,
                    redirectUrl = Url.Action("Policies", "Customer") // Optional: redirect after success
                });
            }
            catch (Exception ex)
            {
                // 🎯 DEBUG: Log full exception
                System.Diagnostics.Debug.WriteLine($"[ApplyPolicy] ERROR: {ex.Message}\n{ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Failed to submit application. Please try again."
                });
            }
        }

        // GET: Customer/GetApplicationStatus (AJAX)
        [HttpGet]
        [Route("Customer/GetApplicationStatus")]
        public async Task<IActionResult> GetApplicationStatus(int policyId)
        {
            try
            {
                var customerId = GetLoggedInCustomerId();
                if (customerId == 0)
                    return Json(new { error = "Unauthorized" });

                var application = await _context.Applications
                    .FirstOrDefaultAsync(a => a.PolicyId == policyId && a.UserId == customerId);

                if (application == null)
                    return Json(new { applied = false });

                return Json(new
                {
                    applied = true,
                    status = application.Status,
                    applicationId = application.ApplicationId,
                    appliedDate = application.ApplyDate.ToString("dd-MMM-yyyy")
                });
            }
            catch
            {
                return Json(new { error = "Failed to fetch status" });
            }
        }

        // GET: Customer/Applications - FIXED VERSION
        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (!customerId.HasValue || userRole != "Customer")
                    return RedirectToAction("Login", "Account");

                // ✅ REMOVED: .Include(a => a.PolicyDocuments)
                var applications = await _context.Applications
                    .Where(a => a.UserId == customerId.Value)
                    .Include(a => a.Policy)
                        .ThenInclude(p => p.InsuranceType)
                    // ✅ REMOVED: .Include(a => a.PolicyDocuments)
                    .OrderByDescending(a => a.ApplyDate)
                    .Select(a => new CustomerApplicationItem
                    {
                        ApplicationId = a.ApplicationId,
                        PolicyId = a.PolicyId,
                        PolicyName = a.Policy.PolicyName,
                        InsuranceTypeName = a.Policy.InsuranceType.Name,
                        PremiumAmount = a.Policy.PremiumAmount,
                        CoverageAmount = a.Policy.CoverageAmount,
                        Duration = a.Policy.Duration,
                        ApplyDate = a.ApplyDate,
                        Status = a.Status ?? "Pending",
                        Remarks = a.Remarks,
                        AgentName = a.AgentId.HasValue
                            ? _context.Users.Where(u => u.UserId == a.AgentId).Select(u => u.FullName).FirstOrDefault()
                            : null,
                        // ✅ SET EMPTY LIST INSTEAD
                        Documents = new List<ApplicationDocument>()
                    })
                    .ToListAsync();

                var insuranceTypes = await _context.InsuranceTypes
                    .Select(t => new InsuranceTypeSummary
                    {
                        InsuranceTypeId = t.InsuranceTypeId,
                        Name = t.Name
                    })
                    .OrderBy(t => t.Name)
                    .ToListAsync();

                var viewModel = new CustomerApplicationsIndexViewModel
                {
                    Applications = applications,
                    InsuranceTypes = insuranceTypes,
                    TotalApplications = applications.Count,
                    PendingApplications = applications.Count(a => a.Status == "Pending"),
                    ApprovedApplications = applications.Count(a => a.Status == "Approved"),
                    RejectedApplications = applications.Count(a => a.Status == "Rejected")
                };

                ViewBag.Policies = await _context.Policies
            .Where(p => p.Status) // Sirf Active policies dropdown mein aayengi
            .OrderBy(p => p.PolicyName)
            .ToListAsync();

                ViewBag.CurrentUserId = customerId.Value;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Customer/Applications] Error: {ex.Message}");
                TempData["Error"] = "Unable to load applications. Please try again.";
                return View(new CustomerApplicationsIndexViewModel());
            }
        }

        // POST: Customer/CreateApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateApplication(int PolicyId, string Remarks)
        {
            // 1. Session se Customer ki ID check karein
            var customerId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (!customerId.HasValue || userRole != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Basic Validation
            if (PolicyId <= 0)
            {
                TempData["Error"] = "Please select a valid policy.";
                return RedirectToAction(nameof(Applications));
            }

            // 3. Check karein ke Policy exist karti hai aur Active hai
            var policy = await _context.Policies.FirstOrDefaultAsync(p => p.PolicyId == PolicyId && p.Status);
            if (policy == null)
            {
                TempData["Error"] = "Selected policy is not available or inactive.";
                return RedirectToAction(nameof(Applications));
            }

            // 4. Duplicate Application Prevent karein (Ek hi policy ke liye dobara apply na kar sake)
            var existingApp = await _context.Applications
                .FirstOrDefaultAsync(a => a.UserId == customerId.Value && a.PolicyId == PolicyId);

            if (existingApp != null)
            {
                TempData["Error"] = "You have already applied for this policy.";
                return RedirectToAction(nameof(Applications));
            }

            // 5. Database mein Nayi Application Save karein
            var newApplication = new Application
            {
                UserId = customerId.Value,      // ✅ Session se secure ID
                PolicyId = PolicyId,            // ✅ Form se aayi Policy ID
                ApplyDate = DateTime.Now,       // ✅ Current date/time
                Status = "Pending",             // ✅ Customer hamesha 'Pending' status mein apply karega
                Remarks = Remarks ?? string.Empty,
                AgentId = null                  // ✅ Agent baad mein Admin panel se assign hoga
            };

            _context.Applications.Add(newApplication);
            await _context.SaveChangesAsync();

            // Success message set karein aur wapis Applications page par redirect karein
            TempData["Success"] = "Your application has been submitted successfully! Status: Pending.";
            return RedirectToAction(nameof(Applications));
        }

        // GET: Customer/GetPolicyDetails (AJAX - for Policy Details Modal)
        [HttpGet]
        [Route("Customer/GetPolicyDetails")]
        public async Task<IActionResult> GetPolicyDetails(int policyId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (!customerId.HasValue)
                    return Json(new { error = "Unauthorized" });

                var policy = await _context.Policies
                    .Include(p => p.InsuranceType)
                    .FirstOrDefaultAsync(p => p.PolicyId == policyId);

                if (policy == null)
                    return Json(new { error = "Policy not found" });

                var viewModel = new PolicyDetailsViewModel
                {
                    PolicyId = policy.PolicyId,
                    PolicyName = policy.PolicyName,
                    InsuranceTypeName = policy.InsuranceType?.Name ?? "Unknown",
                    PremiumAmount = policy.PremiumAmount,
                    CoverageAmount = policy.CoverageAmount,
                    Duration = policy.Duration,
                    Terms = policy.Terms,
                    Status = policy.Status,
                    CreatedAt = policy.CreatedAt
                };

                return Json(viewModel);
            }
            catch
            {
                return Json(new { error = "Failed to fetch policy details" });
            }
        }

        // GET: Customer/GetApplicationTimeline (AJAX - for Tracking Modal) - ✅ FIXED
        [HttpGet]
        [Route("Customer/GetApplicationTimeline")]
        public async Task<IActionResult> GetApplicationTimeline(int applicationId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (!customerId.HasValue)
                    return Json(new { error = "Unauthorized" });

                var application = await _context.Applications
                    .Include(a => a.Policy)
                    .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.UserId == customerId.Value);

                if (application == null)
                    return Json(new { error = "Application not found" });

                // ✅ USE List<TimelineItem> INSTEAD OF List<object>
                var timeline = new List<TimelineItem>
        {
            new TimelineItem
            {
                Date = application.ApplyDate.ToString("dd-MMM-yyyy HH:mm"),
                Event = "Application Submitted",
                Status = "Pending",
                Description = $"Applied for policy: {application.Policy.PolicyName}"
            }
        };

                // If approved/rejected, add that event
                if (application.Status == "Approved")
                {
                    timeline.Add(new TimelineItem
                    {
                        Date = application.ApplyDate.AddDays(1).ToString("dd-MMM-yyyy HH:mm"),
                        Event = "Application Approved",
                        Status = "Approved",
                        Description = "Your application has been approved by our team."
                    });
                }
                else if (application.Status == "Rejected")
                {
                    timeline.Add(new TimelineItem
                    {
                        Date = application.ApplyDate.AddDays(1).ToString("dd-MMM-yyyy HH:mm"),
                        Event = "Application Rejected",
                        Status = "Rejected",
                        Description = !string.IsNullOrEmpty(application.Remarks)
                            ? $"Reason: {application.Remarks}"
                            : "Application did not meet our criteria."
                    });
                }

                // ✅ NOW THIS WORKS: TimelineItem has 'Date' property
                return Json(new
                {
                    applicationId = application.ApplicationId,
                    policyName = application.Policy.PolicyName,
                    currentStatus = application.Status,
                    timeline = timeline.OrderBy(t => t.Date).ToList()
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetApplicationTimeline] Error: {ex.Message}");
                return Json(new { error = "Failed to fetch timeline" });
            }
        }

        // GET: Customer/Claims - ENHANCED VERSION
        [HttpGet]
        public async Task<IActionResult> Claims()
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                // 🔐 Debug: Log session values
                System.Diagnostics.Debug.WriteLine($"[Claims] Session UserId: {customerId}, Role: {userRole}");

                if (!customerId.HasValue || userRole != "Customer")
                    return RedirectToAction("Login", "Account");

                // 🎯 Fetch customer's claims
                var claims = await _context.Claims
                    .Where(c => c.UserId == customerId.Value)
                    .Include(c => c.Policy)
                        .ThenInclude(p => p.InsuranceType)
                    .OrderByDescending(c => c.ClaimDate)
                    .Select(c => new CustomerClaimItem
                    {
                        ClaimId = c.ClaimId,
                        PolicyId = c.PolicyId,
                        PolicyName = c.Policy.PolicyName,
                        InsuranceTypeName = c.Policy.InsuranceType.Name,
                        ClaimAmount = c.ClaimAmount,
                        Reason = c.Reason,
                        ClaimDate = c.ClaimDate,
                        Status = c.Status ?? "Pending"
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[Claims] Found {claims.Count} claims for customer {customerId}");

                // 📋 Get customer's APPROVED policies (for claim submission)
                // ✅ CRITICAL: Only policies where Application.Status == "Approved"
                var approvedPolicies = await _context.Applications
                    .Where(a => a.UserId == customerId.Value && a.Status == "Approved")
                    .Include(a => a.Policy)
                        .ThenInclude(p => p.InsuranceType)
                    .Select(a => new CustomerPolicyItem
                    {
                        PolicyId = a.PolicyId,
                        PolicyName = a.Policy.PolicyName,
                        InsuranceTypeName = a.Policy.InsuranceType.Name,
                        CoverageAmount = a.Policy.CoverageAmount,
                        StartDate = a.ApplyDate,
                        EndDate = a.ApplyDate.AddMonths(a.Policy.Duration)
                    })
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[Claims] Found {approvedPolicies.Count} approved policies for dropdown");

                // 🔍 If no approved policies, show helpful message via ViewBag
                if (!approvedPolicies.Any())
                {
                    ViewBag.NoApprovedPolicies = true;
                    ViewBag.Message = "You don't have any approved policies yet. Apply for a policy first to submit claims.";
                }

                var viewModel = new CustomerClaimsIndexViewModel
                {
                    Claims = claims,
                    ApprovedPolicies = approvedPolicies,
                    TotalClaims = claims.Count,
                    PendingClaims = claims.Count(c => c.Status == "Pending"),
                    ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                    RejectedClaims = claims.Count(c => c.Status == "Rejected")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Customer/Claims] ERROR: {ex.Message}\n{ex.StackTrace}");
                TempData["Error"] = "Unable to load claims. Please try again.";
                return View(new CustomerClaimsIndexViewModel());
            }
        }

        // POST: Customer/CreateClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClaim(CreateClaimViewModel model)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (!customerId.HasValue || userRole != "Customer")
                    return Json(new { success = false, message = "Authentication required." });

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }

                var application = await _context.Applications
                    .FirstOrDefaultAsync(a => a.PolicyId == model.PolicyId &&
                                             a.UserId == customerId.Value &&
                                             a.Status == "Approved");

                if (application == null)
                {
                    return Json(new { success = false, message = "Invalid policy or policy not approved." });
                }

                // ✅ USE FULLY QUALIFIED NAME OR ALIAS
                var claim = new ClaimModel  // Or use: new OnlineInsuranceManagementSystem.Models.Claim
                {
                    UserId = customerId.Value,
                    PolicyId = model.PolicyId,
                    ClaimAmount = model.ClaimAmount,
                    Reason = model.Reason,
                    ClaimDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"[CreateClaim] Claim {claim.ClaimId} created successfully");

                return Json(new
                {
                    success = true,
                    message = "Claim submitted successfully! Status: Pending",
                    claimId = claim.ClaimId,
                    redirectUrl = Url.Action("Claims", "Customer")
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CreateClaim] ERROR: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = "Failed to submit claim. Please try again." });
            }
        }

        // GET: Customer/GetClaimDetails (AJAX - for Claim Details Modal)
        [HttpGet]
        [Route("Customer/GetClaimDetails")]
        public async Task<IActionResult> GetClaimDetails(int claimId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (!customerId.HasValue)
                    return Json(new { error = "Unauthorized" });

                var claim = await _context.Claims
                    .Include(c => c.Policy)
                        .ThenInclude(p => p.InsuranceType)
                    .FirstOrDefaultAsync(c => c.ClaimId == claimId && c.UserId == customerId.Value);

                if (claim == null)
                    return Json(new { error = "Claim not found" });

                var viewModel = new ClaimDetailsViewModel
                {
                    ClaimId = claim.ClaimId,
                    PolicyName = claim.Policy.PolicyName,
                    InsuranceTypeName = claim.Policy.InsuranceType.Name,
                    ClaimAmount = claim.ClaimAmount,
                    Reason = claim.Reason ?? "No reason provided",
                    ClaimDate = claim.ClaimDate,
                    Status = claim.Status ?? "Pending"
                };

                return Json(viewModel);
            }
            catch
            {
                return Json(new { error = "Failed to fetch claim details" });
            }
        }

        // GET: Customer/Payments
        [HttpGet]
        public async Task<IActionResult> Payments()
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (!customerId.HasValue || userRole != "Customer")
                    return RedirectToAction("Login", "Account");

                // 🎯 Fetch customer's payments with related data
                var payments = await _context.Payments
                    .Where(p => p.UserId == customerId.Value)
                    .Include(p => p.Policy)
                        .ThenInclude(pol => pol.InsuranceType)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new CustomerPaymentItem
                    {
                        PaymentId = p.PaymentId,
                        PolicyId = p.PolicyId,
                        PolicyName = p.Policy.PolicyName,
                        InsuranceTypeName = p.Policy.InsuranceType.Name,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod,
                        TransactionId = p.TransactionId,
                        PaymentDate = p.PaymentDate,
                        Status = p.Status ?? "Paid"
                    })
                    .ToListAsync();

                // 📋 Get customer's active policies (for new payment submission)
                var activePolicies = await _context.Applications
                    .Where(a => a.UserId == customerId.Value && a.Status == "Approved")
                    .Include(a => a.Policy)
                        .ThenInclude(p => p.InsuranceType)
                    .Select(a => new CustomerPolicyItem
                    {
                        PolicyId = a.PolicyId,
                        PolicyName = a.Policy.PolicyName,
                        InsuranceTypeName = a.Policy.InsuranceType.Name,
                        PremiumAmount = a.Policy.PremiumAmount,  // ✅ Now works!
                        CoverageAmount = a.Policy.CoverageAmount,
                        StartDate = a.ApplyDate,
                        EndDate = a.ApplyDate.AddMonths(a.Policy.Duration)
                    })
                    .ToListAsync();

                // 📈 Calculate summary stats for top cards
                var viewModel = new CustomerPaymentsIndexViewModel
                {
                    Payments = payments,
                    ActivePolicies = activePolicies,
                    TotalPayments = payments.Count,
                    TotalAmountPaid = payments.Where(p => p.Status == "Paid").Sum(p => p.Amount),
                    PaidPayments = payments.Count(p => p.Status == "Paid"),
                    PendingPayments = payments.Count(p => p.Status == "Pending"),
                    FailedPayments = payments.Count(p => p.Status == "Failed")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Customer/Payments] Error: {ex.Message}");
                TempData["Error"] = "Unable to load payments. Please try again.";
                return View(new CustomerPaymentsIndexViewModel());
            }
        }

        // POST: Customer/CreatePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(CreatePaymentViewModel model)
        {
            try
            {
                // 🔐 Get logged-in customer from Session
                var customerId = HttpContext.Session.GetInt32("UserId");
                var userRole = HttpContext.Session.GetString("UserRole");

                if (!customerId.HasValue || userRole != "Customer")
                    return Json(new { success = false, message = "Authentication required." });

                // ✅ Validate model
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = errors });
                }

                // 🔍 Verify policy belongs to customer and is approved
                var application = await _context.Applications
                    .FirstOrDefaultAsync(a => a.PolicyId == model.PolicyId &&
                                             a.UserId == customerId.Value &&
                                             a.Status == "Approved");

                if (application == null)
                {
                    return Json(new { success = false, message = "Invalid policy or policy not approved." });
                }

                // 💾 Generate unique transaction ID if not provided
                var transactionId = string.IsNullOrEmpty(model.TransactionId)
                    ? $"TXN-{DateTime.Now:yyyyMMddHHmmss}-{customerId.Value}"
                    : model.TransactionId;

                // 💾 Create new payment
                var payment = new Payment
                {
                    UserId = customerId.Value,
                    PolicyId = model.PolicyId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    TransactionId = transactionId,
                    PaymentDate = DateTime.Now,
                    Status = "Paid" // Customer payments are auto-marked as Paid
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"[CreatePayment] Payment {payment.PaymentId} created successfully");

                return Json(new
                {
                    success = true,
                    message = "Payment submitted successfully!",
                    paymentId = payment.PaymentId,
                    transactionId = payment.TransactionId,
                    redirectUrl = Url.Action("Payments", "Customer")
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CreatePayment] ERROR: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = "Failed to process payment. Please try again." });
            }
        }

        // GET: Customer/GetPaymentDetails (AJAX - for Payment Details Modal)
        [HttpGet]
        [Route("Customer/GetPaymentDetails")]
        public async Task<IActionResult> GetPaymentDetails(int paymentId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (!customerId.HasValue)
                    return Json(new { error = "Unauthorized" });

                var payment = await _context.Payments
                    .Include(p => p.Policy)
                        .ThenInclude(pol => pol.InsuranceType)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.UserId == customerId.Value);

                if (payment == null)
                    return Json(new { error = "Payment not found" });

                var viewModel = new PaymentDetailsViewModel
                {
                    PaymentId = payment.PaymentId,
                    PolicyName = payment.Policy.PolicyName,
                    InsuranceTypeName = payment.Policy.InsuranceType.Name,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod ?? "Unknown",
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.PaymentDate,
                    Status = payment.Status ?? "Paid"
                };

                return Json(viewModel);
            }
            catch
            {
                return Json(new { error = "Failed to fetch payment details" });
            }
        }

        // GET: Customer/DownloadReceipt (AJAX - generates receipt data)
        [HttpGet]
        [Route("Customer/DownloadReceipt")]
        public async Task<IActionResult> DownloadReceipt(int paymentId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (!customerId.HasValue)
                    return Json(new { error = "Unauthorized" });

                var payment = await _context.Payments
                    .Include(p => p.Policy)
                        .ThenInclude(pol => pol.InsuranceType)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.UserId == customerId.Value);

                if (payment == null)
                    return Json(new { error = "Payment not found" });

                // Return receipt data for client-side PDF generation or display
                var receipt = new
                {
                    receiptId = $"RCP-{payment.PaymentId:D4}-{DateTime.Now:yyyyMMdd}",
                    paymentId = payment.PaymentIdDisplay,
                    customerName = payment.User?.FullName ?? "Customer",
                    policyName = payment.Policy.PolicyName,
                    insuranceType = payment.Policy.InsuranceType.Name,
                    amount = payment.Amount.ToString("N2"),
                    paymentMethod = payment.PaymentMethod ?? "Unknown",
                    transactionId = payment.TransactionId ?? "N/A",
                    paymentDate = payment.PaymentDate.ToString("dd-MMM-yyyy HH:mm"),
                    status = payment.Status ?? "Paid",
                    generatedAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss")
                };

                return Json(receipt);
            }
            catch
            {
                return Json(new { error = "Failed to generate receipt" });
            }
        }

        public async Task<IActionResult> MyLoans()
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var loans = await _context.Loans
                .Where(l => l.UserId == customerId.Value)
                .Include(l => l.Policy)
                .Select(l => new LoanViewModel
                {
                    LoanId = l.LoanId,
                    PolicyName = l.Policy.PolicyName,
                    Amount = l.Amount,
                    Status = l.Status,
                    ApplyDate = l.ApplyDate
                })
                .ToListAsync();

            ViewBag.ActivePolicies = await _context.Applications
                .Where(a => a.UserId == customerId.Value &&
                            a.Status == "Approved")
                .Include(a => a.Policy)
                .Select(a => a.Policy)
                .ToListAsync();

            ViewBag.TotalLoans = loans.Count;
            ViewBag.PendingLoans = loans.Count(x => x.Status == "Pending");
            ViewBag.ApprovedLoans = loans.Count(x => x.Status == "Approved");
            ViewBag.RejectedLoans = loans.Count(x => x.Status == "Rejected");

            return View(loans);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLoan(int PolicyId, decimal Amount, string? Reason)
        {
            var customerId = HttpContext.Session.GetInt32("UserId");

            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var loan = new Loan
            {
                UserId = customerId.Value,
                PolicyId = PolicyId,
                Amount = Amount,
                //Reason = Reason,
                ApplyDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Loan request submitted successfully.";

            return RedirectToAction(nameof(MyLoans));
        }
    }
}
