using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using OnlineInsuranceManagementSystem.Services;
using OnlineInsuranceManagementSystem.ViewModels;
using System.Collections.Generic;
using System.Globalization;  // ✅ ADD THIS
using System.Linq;
using System.Threading.Tasks;

namespace OnlineInsuranceManagementSystem.Controllers
{
    public class AdminController : BaseAdminController
    {
        // ✅ Sirf yeh rakhein
        private readonly IConfiguration _config;

        // ✅ Constructor
        public AdminController(ApplicationDbContext context, IConfiguration configuration) : base(context)
        {
            _config = configuration;
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel();
            var today = DateTime.Now;
            var sixMonthsAgo = today.AddMonths(-6);

            // ==================== SUMMARY CARDS - LINQ QUERIES ====================

            viewModel.TotalUsers = await _context.Users.CountAsync(u => u.Role == "User");
            viewModel.TotalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
            viewModel.TotalAgents = await _context.Users.CountAsync(u => u.Role == "Agent");

            viewModel.TotalClaims = await _context.Claims.CountAsync();
            viewModel.PendingClaims = await _context.Claims.CountAsync(c => c.Status == "Pending");

            viewModel.TotalPayments = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => p.Amount);

            viewModel.TotalMessages = await _context.Messages.CountAsync();
            viewModel.UnreadMessages = await _context.Messages.CountAsync(m => m.IsRead == false);

            // ==================== 🔄 FIXED: MONTHLY CLAIMS CHART DATA ====================
            // Step 1: Fetch RAW data from DB (without string formatting)
            var rawClaimsData = await _context.Claims
                .Where(c => c.ClaimDate >= sixMonthsAgo)
                .GroupBy(c => new { c.ClaimDate.Year, c.ClaimDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            // Step 2: Format in MEMORY (C# side)
            viewModel.MonthlyClaimsData = rawClaimsData
                .Select(d => new ChartSeriesData
                {
                    Month = $"{GetMonthName(d.Month)} {d.Year}",  // ✅ Now this works!
                    ClaimsCount = d.Count,
                    PaymentsAmount = 0
                })
                .OrderBy(x => ParseMonthYear(x.Month))
                .ToList();

            // Fill missing months
            viewModel.MonthlyClaimsData = FillMissingMonths(viewModel.MonthlyClaimsData, sixMonthsAgo, today);

            // ==================== 🔄 FIXED: MONTHLY PAYMENTS CHART DATA ====================
            // Step 1: Fetch RAW data from DB
            var rawPaymentsData = await _context.Payments
                .Where(p => p.PaymentDate >= sixMonthsAgo && p.Status == "Paid")
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            // Step 2: Format in MEMORY
            viewModel.MonthlyPaymentsData = rawPaymentsData
                .Select(d => new ChartSeriesData
                {
                    Month = $"{GetMonthName(d.Month)} {d.Year}",  // ✅ Now this works!
                    ClaimsCount = 0,
                    PaymentsAmount = d.Amount
                })
                .OrderBy(x => ParseMonthYear(x.Month))
                .ToList();

            // Fill missing months
            viewModel.MonthlyPaymentsData = FillMissingMonths(viewModel.MonthlyPaymentsData, sixMonthsAgo, today);

            // ==================== RECENT CLAIMS (Latest 5) ====================
            viewModel.RecentClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Policy)
                .OrderByDescending(c => c.ClaimDate)
                .Take(5)
                .Select(c => new RecentClaimItem
                {
                    ClaimId = c.ClaimId,
                    CustomerName = c.User.FullName,
                    PolicyName = c.Policy.PolicyName,
                    ClaimAmount = c.ClaimAmount,
                    Status = c.Status,
                    ClaimDate = c.ClaimDate
                })
                .ToListAsync();

            // ==================== RECENT MESSAGES (Latest 5) ====================
            viewModel.RecentMessages = await _context.Messages
                .OrderByDescending(m => m.SentDate)
                .Take(5)
                .Select(m => new RecentMessageItem
                {
                    MessageId = m.MessageId,
                    SenderName = m.FullName ?? m.Sender.FullName,
                    Subject = m.Subject,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            // ==================== RECENT PAYMENTS (Latest 5) ====================
            viewModel.RecentPayments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Policy)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new RecentPaymentItem
                {
                    PaymentId = p.PaymentId,
                    CustomerName = p.User.FullName,
                    PolicyName = p.Policy.PolicyName,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            ViewBag.UnreadMessageCount = viewModel.UnreadMessages;

            return View(viewModel);
        }

        // Helper: Get month name from number
        private string GetMonthName(int monthNumber)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthNumber);
        }

        // ✅ NEW HELPER: Parse "MMM yyyy" string to DateTime for sorting
        private DateTime ParseMonthYear(string monthYear)
        {
            return DateTime.ParseExact(monthYear, "MMM yyyy", CultureInfo.InvariantCulture);
        }

        // Helper: Fill missing months with zero values for continuous chart
        private List<ChartSeriesData> FillMissingMonths(
            List<ChartSeriesData> data,
            DateTime startDate,
            DateTime endDate)
        {
            var result = new List<ChartSeriesData>();
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                var monthLabel = $"{GetMonthName(current.Month)} {current.Year}";
                var existing = data.FirstOrDefault(d => d.Month == monthLabel);

                result.Add(existing ?? new ChartSeriesData
                {
                    Month = monthLabel,
                    ClaimsCount = 0,
                    PaymentsAmount = 0
                });

                current = current.AddMonths(1);
            }

            return result.OrderBy(x => ParseMonthYear(x.Month)).ToList();
        }

        // ==================== 📋 APPLICATIONS MANAGEMENT ====================

        // GET: Applications - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            // Fetch all applications with related entities
            var applications = await _context.Applications
                .Include(a => a.User)           // Customer
                .Include(a => a.Policy)
                .Include(a => a.Agent)          // Agent (optional)
                .OrderByDescending(a => a.ApplyDate)
                .ToListAsync();

            // Map to ViewModel
            var viewModelList = applications.Select(a => new AgentApplicationViewModel
            {
                ApplicationId = a.ApplicationId,
                UserId = a.UserId,
                UserName = a.User?.FullName ?? "Unknown",
                PolicyId = a.PolicyId,
                PolicyName = a.Policy?.PolicyName ?? "Unknown",
                AgentId = a.AgentId,
                AgentName = a.Agent?.FullName ?? "-",
                ApplyDate = a.ApplyDate,
                Status = a.Status ?? "Pending",
                Remarks = a.Remarks
            }).ToList();

            // Calculate summary stats for cards
            ViewBag.TotalApplications = viewModelList.Count;
            ViewBag.PendingApplications = viewModelList.Count(a => a.Status == "Pending");
            ViewBag.ApprovedApplications = viewModelList.Count(a => a.Status == "Approved");
            ViewBag.RejectedApplications = viewModelList.Count(a => a.Status == "Rejected");

            // Populate dropdowns for modals
            ViewBag.Customers = await _context.Users
                .Where(u => u.Role == "Customer" && u.Status)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Policies = await _context.Policies
                .Where(p => p.Status)
                .OrderBy(p => p.PolicyName)
                .ToListAsync();

            ViewBag.Agents = await _context.Users
                .Where(u => u.Role == "Agent" && u.Status)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(viewModelList);
        }

        // POST: Create Application
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateApplication(AgentApplicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var application = new Application
                {
                    UserId = model.UserId,
                    PolicyId = model.PolicyId,

                    // ✅ FIXED LINE - Yeh replace karein
                    AgentId = (model.AgentId.HasValue && model.AgentId > 0) ? model.AgentId : (int?)null,

                    ApplyDate = model.ApplyDate,
                    Status = model.Status ?? "Pending",
                    Remarks = model.Remarks
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Application submitted successfully!";
                return RedirectToAction(nameof(Applications));
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to create application. Please check your input.";
            return RedirectToAction(nameof(Applications));
        }

        // POST: Update Application
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplication(AgentApplicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Applications.FindAsync(model.ApplicationId);
                if (existing != null)
                {
                    existing.UserId = model.UserId;
                    existing.PolicyId = model.PolicyId;
                    existing.AgentId = (model.AgentId.HasValue && model.AgentId > 0) ? model.AgentId : (int?)null;

                    existing.ApplyDate = model.ApplyDate;
                    existing.Status = model.Status ?? "Pending";
                    existing.Remarks = model.Remarks;

                    _context.Applications.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Application updated successfully!";
                    return RedirectToAction(nameof(Applications));
                }
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to update application. Please check your input.";
            return RedirectToAction(nameof(Applications));
        }

        // POST: Delete Application
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application != null)
            {
                // Check for related records before deletion
                var hasPayments = await _context.Payments.AnyAsync(p => p.PolicyId == application.PolicyId && p.UserId == application.UserId);
                var hasClaims = await _context.Claims.AnyAsync(c => c.PolicyId == application.PolicyId && c.UserId == application.UserId);

                if (hasPayments || hasClaims)
                {
                    TempData["Error"] = "Cannot delete application with associated payments or claims.";
                }
                else
                {
                    _context.Applications.Remove(application);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Application deleted successfully!";
                }
            }
            else
            {
                TempData["Error"] = "Application not found.";
            }

            return RedirectToAction(nameof(Applications));
        }

        // GET: AJAX Load Application Details
        [HttpGet]
        [Route("InsuranceManagement/GetApplicationDetails")]
        public async Task<IActionResult> GetApplicationDetails(int id)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.Policy)
                .Include(a => a.Agent)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var viewModel = new AgentApplicationViewModel
            {
                ApplicationId = application.ApplicationId,
                UserId = application.UserId,
                UserName = application.User?.FullName ?? "Unknown",
                PolicyId = application.PolicyId,
                PolicyName = application.Policy?.PolicyName ?? "Unknown",
                AgentId = application.AgentId,
                AgentName = application.Agent?.FullName ?? "-",
                ApplyDate = application.ApplyDate,
                Status = application.Status ?? "Pending",
                Remarks = application.Remarks
            };

            return Json(viewModel);
        }

        // Helper: Populate dropdowns for modals
        private async Task PopulateDropdowns()
        {
            ViewBag.Customers = await _context.Users
                .Where(u => u.Role == "Customer" && u.Status)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Policies = await _context.Policies
                .Where(p => p.Status)
                .OrderBy(p => p.PolicyName)
                .ToListAsync();

            ViewBag.Agents = await _context.Users
                .Where(u => u.Role == "Agent" && u.Status)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        // ==================== 📋 CLAIMS MANAGEMENT ====================

        // GET: Claims - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> Claims()
        {
            // Fetch all claims with related entities
            var claims = await _context.Claims
                .Include(c => c.User)           // Customer
                .Include(c => c.Policy)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            // Map to ViewModel
            var viewModelList = claims.Select(c => new ClaimViewModel
            {
                ClaimId = c.ClaimId,
                UserId = c.UserId,
                UserName = c.User?.FullName ?? "Unknown",
                PolicyId = c.PolicyId,
                PolicyName = c.Policy?.PolicyName ?? "Unknown",
                ClaimAmount = c.ClaimAmount,
                Reason = c.Reason,
                ClaimDate = c.ClaimDate,
                Status = c.Status ?? "Pending"
            }).ToList();

            // Calculate summary stats for cards
            ViewBag.TotalClaims = viewModelList.Count;
            ViewBag.PendingClaims = viewModelList.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = viewModelList.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = viewModelList.Count(c => c.Status == "Rejected");

            // Populate dropdowns for modals
            ViewBag.Customers = await _context.Users
                .Where(u => u.Role == "Customer" && u.Status)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Policies = await _context.Policies
                .Where(p => p.Status)
                .OrderBy(p => p.PolicyName)
                .ToListAsync();

            return View(viewModelList);
        }

        // POST: Create Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClaim(ClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                var claim = new Claim
                {
                    UserId = model.UserId,
                    PolicyId = model.PolicyId,
                    ClaimAmount = model.ClaimAmount,
                    Reason = model.Reason,
                    ClaimDate = model.ClaimDate,
                    Status = model.Status ?? "Pending"
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(Claims));
            }

            // Repopulate dropdowns on validation error
            await PopulateDropdowns();
            TempData["Error"] = "Failed to create claim. Please check your input.";
            return RedirectToAction(nameof(Claims));
        }

        // POST: Update Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaim(ClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Claims.FindAsync(model.ClaimId);
                if (existing != null)
                {
                    existing.UserId = model.UserId;
                    existing.PolicyId = model.PolicyId;
                    existing.ClaimAmount = model.ClaimAmount;
                    existing.Reason = model.Reason;
                    existing.ClaimDate = model.ClaimDate;
                    existing.Status = model.Status ?? "Pending";

                    _context.Claims.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Claim updated successfully!";
                    return RedirectToAction(nameof(Claims));
                }
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to update claim. Please check your input.";
            return RedirectToAction(nameof(Claims));
        }

        // POST: Delete Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                // Check for related payments before deletion
                var hasPayments = await _context.Payments.AnyAsync(p => p.PolicyId == claim.PolicyId && p.UserId == claim.UserId);

                if (hasPayments)
                {
                    TempData["Error"] = "Cannot delete claim with associated payments.";
                }
                else
                {
                    _context.Claims.Remove(claim);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Claim deleted successfully!";
                }
            }
            else
            {
                TempData["Error"] = "Claim not found.";
            }

            return RedirectToAction(nameof(Claims));
        }

        // GET: AJAX Load Claim Details
        [HttpGet]
        [Route("InsuranceManagement/GetClaimDetails")]
        public async Task<IActionResult> GetClaimDetails(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Policy)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
                return NotFound();

            var viewModel = new ClaimViewModel
            {
                ClaimId = claim.ClaimId,
                UserId = claim.UserId,
                UserName = claim.User?.FullName ?? "Unknown",
                PolicyId = claim.PolicyId,
                PolicyName = claim.Policy?.PolicyName ?? "Unknown",
                ClaimAmount = claim.ClaimAmount,
                Reason = claim.Reason,
                ClaimDate = claim.ClaimDate,
                Status = claim.Status ?? "Pending"
            };

            return Json(viewModel);
        }

        // ==================== 💰 LOANS MANAGEMENT ====================

        // GET: Loans - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> Loans()
        {
            // Fetch all loans with related entities
            var loans = await _context.Loans
                .Include(l => l.User)           // Customer
                .Include(l => l.Policy)
                .OrderByDescending(l => l.ApplyDate)
                .ToListAsync();

            // Map to ViewModel
            var viewModelList = loans.Select(l => new LoanViewModel
            {
                LoanId = l.LoanId,
                UserId = l.UserId,
                UserName = l.User?.FullName ?? "Unknown",
                PolicyId = l.PolicyId,
                PolicyName = l.Policy?.PolicyName ?? "Unknown",
                Amount = l.Amount,
                ApplyDate = l.ApplyDate,
                Status = l.Status ?? "Pending"
            }).ToList();

            // Calculate summary stats for cards
            ViewBag.TotalLoans = viewModelList.Count;
            ViewBag.PendingLoans = viewModelList.Count(l => l.Status == "Pending");
            ViewBag.ApprovedLoans = viewModelList.Count(l => l.Status == "Approved");
            ViewBag.RejectedLoans = viewModelList.Count(l => l.Status == "Rejected");

            // Populate dropdowns for modals (reuse existing helper)
            await PopulateDropdowns();

            return View(viewModelList);
        }

        // POST: Create Loan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLoan(LoanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loan = new Loan
                {
                    UserId = model.UserId,
                    PolicyId = model.PolicyId,
                    Amount = model.Amount,
                    ApplyDate = model.ApplyDate,
                    Status = model.Status ?? "Pending"
                };

                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Loan submitted successfully!";
                return RedirectToAction(nameof(Loans));
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to create loan. Please check your input.";
            return RedirectToAction(nameof(Loans));
        }

        // POST: Update Loan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLoan(LoanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Loans.FindAsync(model.LoanId);
                if (existing != null)
                {
                    existing.UserId = model.UserId;
                    existing.PolicyId = model.PolicyId;
                    existing.Amount = model.Amount;
                    existing.ApplyDate = model.ApplyDate;
                    existing.Status = model.Status ?? "Pending";

                    _context.Loans.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Loan updated successfully!";
                    return RedirectToAction(nameof(Loans));
                }
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to update loan. Please check your input.";
            return RedirectToAction(nameof(Loans));
        }

        // POST: Delete Loan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan != null)
            {
                // Check for related payments before deletion
                var hasPayments = await _context.Payments.AnyAsync(p => p.PolicyId == loan.PolicyId && p.UserId == loan.UserId);

                if (hasPayments)
                {
                    TempData["Error"] = "Cannot delete loan with associated payments.";
                }
                else
                {
                    _context.Loans.Remove(loan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Loan deleted successfully!";
                }
            }
            else
            {
                TempData["Error"] = "Loan not found.";
            }

            return RedirectToAction(nameof(Loans));
        }

        // GET: AJAX Load Loan Details
        [HttpGet]
        [Route("Admin/GetLoanDetails")]
        public async Task<IActionResult> GetLoanDetails(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .Include(l => l.Policy)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                return NotFound();

            var viewModel = new LoanViewModel
            {
                LoanId = loan.LoanId,
                UserId = loan.UserId,
                UserName = loan.User?.FullName ?? "Unknown",
                PolicyId = loan.PolicyId,
                PolicyName = loan.Policy?.PolicyName ?? "Unknown",
                Amount = loan.Amount,
                ApplyDate = loan.ApplyDate,
                Status = loan.Status ?? "Pending"
            };

            return Json(viewModel);
        }

        // ==================== 💳 PAYMENTS MANAGEMENT ====================

        // GET: Payments - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> Payments()
        {
            // Fetch all payments with related entities
            var payments = await _context.Payments
                .Include(p => p.User)           // Customer
                .Include(p => p.Policy)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Map to ViewModel
            var viewModelList = payments.Select(p => new PaymentViewModel
            {
                PaymentId = p.PaymentId,
                UserId = p.UserId,
                UserName = p.User?.FullName ?? "Unknown",
                PolicyId = p.PolicyId,
                PolicyName = p.Policy?.PolicyName ?? "Unknown",
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                TransactionId = p.TransactionId,
                Status = p.Status ?? "Paid"
            }).ToList();

            // Calculate summary stats for cards
            ViewBag.TotalPayments = viewModelList.Count;
            ViewBag.PaidPayments = viewModelList.Count(p => p.Status == "Paid");
            ViewBag.PendingPayments = viewModelList.Count(p => p.Status == "Pending");
            ViewBag.FailedPayments = viewModelList.Count(p => p.Status == "Failed");

            // Populate dropdowns for modals (reuse existing helper)
            await PopulateDropdowns();

            return View(viewModelList);
        }

        // POST: Create Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var payment = new Payment
                {
                    UserId = model.UserId,
                    PolicyId = model.PolicyId,
                    Amount = model.Amount,
                    PaymentDate = model.PaymentDate,
                    PaymentMethod = model.PaymentMethod,
                    TransactionId = model.TransactionId,
                    Status = model.Status ?? "Paid"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Payment recorded successfully!";
                return RedirectToAction(nameof(Payments));
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to create payment. Please check your input.";
            return RedirectToAction(nameof(Payments));
        }

        // POST: Update Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayment(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Payments.FindAsync(model.PaymentId);
                if (existing != null)
                {
                    existing.UserId = model.UserId;
                    existing.PolicyId = model.PolicyId;
                    existing.Amount = model.Amount;
                    existing.PaymentDate = model.PaymentDate;
                    existing.PaymentMethod = model.PaymentMethod;
                    existing.TransactionId = model.TransactionId;
                    existing.Status = model.Status ?? "Paid";

                    _context.Payments.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Payment updated successfully!";
                    return RedirectToAction(nameof(Payments));
                }
            }

            await PopulateDropdowns();
            TempData["Error"] = "Failed to update payment. Please check your input.";
            return RedirectToAction(nameof(Payments));
        }

        // POST: Delete Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                // Check for related claims before deletion
                var hasClaims = await _context.Claims.AnyAsync(c => c.PolicyId == payment.PolicyId && c.UserId == payment.UserId);

                if (hasClaims)
                {
                    TempData["Error"] = "Cannot delete payment with associated claims.";
                }
                else
                {
                    _context.Payments.Remove(payment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Payment deleted successfully!";
                }
            }
            else
            {
                TempData["Error"] = "Payment not found.";
            }

            return RedirectToAction(nameof(Payments));
        }

        // GET: AJAX Load Payment Details
        [HttpGet]
        [Route("Admin/GetPaymentDetails")]
        public async Task<IActionResult> GetPaymentDetails(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Policy)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound();

            var viewModel = new PaymentViewModel
            {
                PaymentId = payment.PaymentId,
                UserId = payment.UserId,
                UserName = payment.User?.FullName ?? "Unknown",
                PolicyId = payment.PolicyId,
                PolicyName = payment.Policy?.PolicyName ?? "Unknown",
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                Status = payment.Status ?? "Paid"
            };

            return Json(viewModel);
        }

        // ==================== 💬 MESSAGES MANAGEMENT ====================

        // GET: Messages - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            var messages = await _context.Messages
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            var viewModelList = messages.Select(m => new MessageViewModel
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                FullName = m.FullName ?? "Anonymous",
                Email = m.Email ?? "",
                Subject = m.Subject ?? "(No Subject)",
                MessageText = m.MessageText ?? "",
                SentDate = m.SentDate,
                IsRead = m.IsRead  // ✅ Database se actual value lo
            }).ToList();

            ViewBag.TotalMessages = viewModelList.Count;
            ViewBag.TodayMessages = viewModelList.Count(m => m.SentDate.Date == DateTime.Today);
            ViewBag.ReadMessages = viewModelList.Count(m => m.IsRead);  // ✅ IsRead use karo
            ViewBag.UnreadMessages = viewModelList.Count(m => !m.IsRead);  // ✅ !IsRead use karo
            ViewBag.UnreadMessageCount = ViewBag.UnreadMessages;

            // ✅ Sidebar badge ke liye
            ViewBag.UnreadMessageCount = ViewBag.UnreadMessages;

            return View(viewModelList);
        }

        // POST: Save Contact Form Message (Called from Landing Page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/SaveContactMessage")]
        public async Task<IActionResult> SaveContactMessage(ContactMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create new message record
                var message = new Message
                {
                    SenderId = 0, // 0 = anonymous/landing page user
                    ReceiverId = 1, // 1 = admin user (adjust based on your admin UserId)
                    FullName = model.FullName,
                    Email = model.Email,
                    Subject = model.Subject,
                    MessageText = model.Message,
                    SentDate = DateTime.Now
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Optional: Send admin notification email
                // await _emailService.SendAdminNotificationAsync(model);

                TempData["Success"] = "Thank you! Your message has been sent successfully.";
                return RedirectToAction("Index", "Home"); // Redirect back to landing page
            }

            // If validation fails, return to contact section with errors
            TempData["ContactError"] = "Please fill all required fields correctly.";
            return RedirectToAction("Index", "Home", new { section = "contact" });
        }

        // POST: Mark Message as Read (AJAX call from View modal)
        [HttpPost]
        [Route("Admin/MarkMessageAsRead")]
        public async Task<IActionResult> MarkMessageAsRead(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;  // ✅ Database mein update karo
                await _context.SaveChangesAsync();
                return Json(new { success = true, isRead = true });
            }
            return NotFound();
        }

        // POST: Delete Message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Message not found.";
            }

            return RedirectToAction(nameof(Messages));
        }

        // GET: AJAX Load Message Details (Optional - for future enhancements)
        [HttpGet]
        [Route("Admin/GetMessageDetails")]
        public async Task<IActionResult> GetMessageDetails(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            var viewModel = new MessageViewModel
            {
                MessageId = message.MessageId,
                FullName = message.FullName ?? "Anonymous",
                Email = message.Email ?? "",
                Subject = message.Subject ?? "(No Subject)",
                MessageText = message.MessageText ?? "",
                SentDate = message.SentDate,
                IsRead = true
            };

            return Json(viewModel);
        }


        // POST: Reply to Message via Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyToMessage(int messageId, string replyMessage)
        {
            try
            {
                var message = await _context.Messages.FindAsync(messageId);
                if (message == null || string.IsNullOrEmpty(message.Email))
                {
                    TempData["Error"] = "Message not found or email not available.";
                    return RedirectToAction(nameof(Messages));
                }

                // Send reply email
                var emailService = _context.GetService<IEmailService>();
                var result = await emailService.SendReplyToCustomerEmailAsync(
                    message.Email,
                    message.FullName,
                    message.Subject,
                    replyMessage,
                    $"#MSG-{message.MessageId:D4}"
                );

                if (result)
                {
                    // Mark message as read/replied
                    message.IsRead = true;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Reply sent successfully!";

                    // For AJAX requests, return JSON instead of redirect
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Reply sent successfully!" });
                    }

                    return RedirectToAction(nameof(Messages));
                }
                else
                {
                    TempData["Error"] = "Failed to send reply. Please try again.";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Failed to send email" });
                    }

                    return RedirectToAction(nameof(Messages));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }

                return RedirectToAction(nameof(Messages));
            }
        }

        public IActionResult Notifications()
        {
            return View();
        }

        // Add these methods to your existing InsuranceManagementController.cs

        // ==================== 📊 REPORTS MANAGEMENT ====================

        // GET: Admin/Reports
        public async Task<IActionResult> Reports(string year, string policyType)
        {
            var viewModel = new ReportViewModel
            {
                SelectedYear = year ?? DateTime.Now.Year.ToString(),
                SelectedPolicyType = policyType ?? "All"
            };

            var selectedYear = int.TryParse(viewModel.SelectedYear, out int y) ? y : DateTime.Now.Year;

            // ==================== SUMMARY METRICS ====================
            viewModel.TotalRevenue = await _context.Payments
                .Where(p => p.Status == "Paid")
                .SumAsync(p => p.Amount);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            viewModel.MonthlyRevenue = await _context.Payments
                .Where(p => p.Status == "Paid"
                    && p.PaymentDate.Year == currentYear
                    && p.PaymentDate.Month == currentMonth)
                .SumAsync(p => p.Amount);

            viewModel.TotalPolicies = await _context.Policies.CountAsync();
            viewModel.ActivePolicies = await _context.Policies.CountAsync(p => p.Status);

            viewModel.TotalClaims = await _context.Claims.CountAsync();
            viewModel.TotalClaimAmount = await _context.Claims.SumAsync(c => c.ClaimAmount);
            viewModel.ApprovedClaimsAmount = await _context.Claims
                .Where(c => c.Status == "Approved").SumAsync(c => c.ClaimAmount);
            viewModel.PendingClaimsAmount = await _context.Claims
                .Where(c => c.Status == "Pending").SumAsync(c => c.ClaimAmount);

            // ==================== MONTHLY REVENUE CHART DATA (FIXED) ====================
            var revenueRawData = await _context.Payments
                .Where(p => p.Status == "Paid" && p.PaymentDate.Year == selectedYear)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.Amount),
                    TransactionCount = g.Count()
                })
                .ToListAsync();

            viewModel.MonthlyRevenueData = revenueRawData
                .OrderBy(x => new DateTime(x.Year, x.Month, 1))
                .Select(x => new ReportViewModel.MonthlyRevenueItem
                {
                    Month = $"{new DateTime(x.Year, x.Month, 1):MMM yyyy}",
                    Revenue = x.Revenue,
                    TransactionCount = x.TransactionCount
                })
                .ToList();

            viewModel.MonthlyRevenueData = FillMissingMonthsRevenue(viewModel.MonthlyRevenueData, selectedYear);

            // ==================== CLAIMS ANALYSIS (FIXED) ====================
            var claimsRawData = await _context.Claims
                .Include(c => c.Policy)
                .ThenInclude(p => p.InsuranceType)
                .Where(c => c.ClaimDate.Year == selectedYear)
                .GroupBy(c => new { c.Policy.InsuranceType.Name, c.Policy.InsuranceTypeId })
                .Select(g => new
                {
                    Category = g.Key.Name,
                    TotalClaims = g.Count(),
                    ApprovedClaims = g.Count(x => x.Status == "Approved"),
                    PendingClaims = g.Count(x => x.Status == "Pending"),
                    RejectedClaims = g.Count(x => x.Status == "Rejected"),
                    TotalAmount = g.Sum(x => x.ClaimAmount)
                })
                .ToListAsync();

            viewModel.ClaimsAnalysisData = claimsRawData
                .OrderByDescending(x => x.TotalAmount)
                .Select(x => new ReportViewModel.ClaimsAnalysisItem
                {
                    Category = x.Category,
                    TotalClaims = x.TotalClaims,
                    ApprovedClaims = x.ApprovedClaims,
                    PendingClaims = x.PendingClaims,
                    RejectedClaims = x.RejectedClaims,
                    TotalAmount = x.TotalAmount
                })
                .ToList();

            // ==================== POLICY DISTRIBUTION (FIXED) ====================
            var colorPalette = new[] { "#0A3D62", "#1B4F72", "#21618C", "#2874A6", "#2E86C1" };

            var policyRawData = await _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.Status)
                .GroupBy(p => new { p.InsuranceType.Name, p.InsuranceTypeId })
                .Select(g => new
                {
                    PolicyType = g.Key.Name,
                    PolicyCount = g.Count(),
                    TotalPremium = g.Sum(x => x.PremiumAmount),
                    TotalCoverage = g.Sum(x => x.CoverageAmount)
                })
                .ToListAsync();

            viewModel.PolicyDistributionData = policyRawData
                .OrderByDescending(x => x.PolicyCount)
                .Select((x, index) => new ReportViewModel.PolicyDistributionItem
                {
                    PolicyType = x.PolicyType,
                    PolicyCount = x.PolicyCount,
                    TotalPremium = x.TotalPremium,
                    TotalCoverage = x.TotalCoverage,
                    Color = colorPalette[index % colorPalette.Length]
                })
                .ToList();

            // ==================== FILTER OPTIONS ====================
            viewModel.AvailableYears = await _context.Payments
                .Select(p => p.PaymentDate.Year.ToString())
                .Distinct()
                .OrderByDescending(y => y)
                .Take(5)
                .ToListAsync();

            viewModel.AvailablePolicyTypes = await _context.InsuranceTypes
                .Select(t => t.Name)
                .ToListAsync();
            viewModel.AvailablePolicyTypes.Insert(0, "All");

            // ==================== RECENT REPORTS ====================
            viewModel.RecentReports = new List<ReportViewModel.RecentReportItem>
    {
        new() { ReportId = 1, ReportType = "Revenue", Period = $"{DateTime.Now:MMM yyyy}",
                GeneratedDate = DateTime.Now.AddDays(-1), GeneratedBy = "Admin",
                Status = "Completed", FileUrl = "#" },
        new() { ReportId = 2, ReportType = "Claims", Period = "Q1 2024",
                GeneratedDate = DateTime.Now.AddDays(-3), GeneratedBy = "Admin",
                Status = "Completed", FileUrl = "#" },
        new() { ReportId = 3, ReportType = "Policy Analysis", Period = "2024 YTD",
                GeneratedDate = DateTime.Now.AddDays(-5), GeneratedBy = "System",
                Status = "Processing", FileUrl = "#" }
    };

            return View(viewModel);
        }


        // Helper: Fill missing months for revenue chart
        private List<ReportViewModel.MonthlyRevenueItem> FillMissingMonthsRevenue(
            List<ReportViewModel.MonthlyRevenueItem> data, int year)
        {
            var result = new List<ReportViewModel.MonthlyRevenueItem>();
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            for (int month = 1; month <= 12; month++)
            {
                var monthLabel = $"{monthNames[month - 1]} {year}";
                var existing = data.FirstOrDefault(d => d.Month == monthLabel);

                if (existing != null)
                {
                    result.Add(existing);
                }
                else
                {
                    if (year < DateTime.Now.Year || month <= DateTime.Now.Month)
                    {
                        result.Add(new ReportViewModel.MonthlyRevenueItem
                        {
                            Month = monthLabel,
                            Revenue = 0,
                            TransactionCount = 0
                        });
                    }
                }
            }

            return result;
        }  // ← Method close

        // Add these methods inside AdminController class, before the closing brace

        // ==================== 🔗 ASSIGN AGENT TO APPLICATION ====================

        // GET: AJAX - Get Approved Agents for Dropdown
        [HttpGet]
        [Route("Admin/GetApprovedAgents")]
        public async Task<IActionResult> GetApprovedAgents()
        {
            var agents = await _context.Users
                .Where(u => u.Role == "Agent" && u.Status && u.AccountStatus == "Approved")
                .OrderBy(u => u.FullName)
                .Select(u => new {
                    id = u.UserId,
                    name = u.FullName,
                    email = u.Email
                })
                .ToListAsync();

            return Json(agents);
        }

        // POST: Assign Agent to Application
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/AssignAgent")]
        public async Task<IActionResult> AssignAgent([FromBody] AssignAgentRequest model)
        {
            if (model.ApplicationId <= 0 || !model.AgentId.HasValue)
            {
                return Json(new { success = false, message = "Invalid application or agent selection." });
            }

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
            {
                return Json(new { success = false, message = "Application not found." });
            }

            // Verify agent exists and is approved
            var agent = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.AgentId && u.Role == "Agent" && u.Status);

            if (agent == null || agent.AccountStatus != "Approved")
            {
                return Json(new { success = false, message = "Selected agent is not available." });
            }

            // ✅ Update assignment
            application.AgentId = model.AgentId;
            application.AssignedDate = DateTime.Now;

            // Optional: Auto-update status if needed
            if (string.IsNullOrEmpty(application.Status) || application.Status == "New")
            {
                application.Status = "Pending";
            }

            await _context.SaveChangesAsync();

            // ✅ Optional: Send notification to agent (if EmailService is configured)
            //await NotifyAgentOfAssignment(agent.Email, application.ApplicationId);

            return Json(new
            {
                success = true,
                message = "Agent assigned successfully!",
                assignedDate = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt"),
                agentName = agent.FullName
            });
        }

        // Helper: Notify agent via email (optional enhancement)
        //private async Task NotifyAgentOfAssignment(string agentEmail, int applicationId)
        //{
        //    try
        //    {
        //        var emailService = HttpContext.RequestServices.GetService<IEmailService>();
        //        if (emailService != null)
        //        {
        //            await emailService.SendAgentAssignmentNotificationAsync(
        //                agentEmail,
        //                applicationId,
        //                $"New Application Assigned - #{applicationId}"
        //            );
        //        }
        //    }
        //    catch
        //    {
        //        // Log error but don't fail the assignment
        //    }
        //}

        // ==================== 📋 REQUEST MODEL FOR ASSIGNMENT ====================
        public class AssignAgentRequest
        {
            public int ApplicationId { get; set; }
            public int? AgentId { get; set; }
        }

    }  // ← ✅ ADD THIS: AdminController class close

}  // ← Namespace close

