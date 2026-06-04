using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using OnlineInsuranceManagementSystem.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OnlineInsuranceManagementSystem.Controllers
{
    public class AgentController : BaseAdminController
    {
        public AgentController(ApplicationDbContext context) : base(context)
        {
            //_context = context;
        }

        // AgentController.cs - Replace/Update the Dashboard() method

        // GET: Agent/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return View(new AgentDashboardViewModel());

            var today = DateTime.Now;
            var sixMonthsAgo = today.AddMonths(-6);

            var viewModel = new AgentDashboardViewModel();

            // ==================== APPLICATIONS STATS ====================
            var agentApplications = await _context.Applications
                .Where(a => a.AgentId == agentId.Value)
                .ToListAsync();

            viewModel.TotalApplications = agentApplications.Count;
            viewModel.PendingApplications = agentApplications.Count(a => a.Status == "Pending");
            viewModel.ApprovedApplications = agentApplications.Count(a => a.Status == "Approved");
            viewModel.RejectedApplications = agentApplications.Count(a => a.Status == "Rejected");

            // ==================== CLAIMS STATS ====================
            var agentClaims = await _context.Claims
                .Join(_context.Applications,
                    claim => new { claim.UserId, claim.PolicyId },
                    app => new { app.UserId, app.PolicyId },
                    (claim, app) => new { Claim = claim, App = app })
                .Where(x => x.App.AgentId == agentId.Value)
                .Select(x => x.Claim)
                .ToListAsync();

            viewModel.TotalClaims = agentClaims.Count;
            viewModel.PendingClaims = agentClaims.Count(c => c.Status == "Pending");
            viewModel.RecommendedClaims = agentClaims.Count(c =>
                c.Status != null && c.Status.Contains("Recommended"));

            // ==================== MONTHLY APPLICATIONS CHART DATA ====================
            var monthlyAppsRaw = await _context.Applications
                .Where(a => a.AgentId == agentId.Value && a.ApplyDate >= sixMonthsAgo)
                .GroupBy(a => new { a.ApplyDate.Year, a.ApplyDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Pending = g.Count(x => x.Status == "Pending"),
                    Approved = g.Count(x => x.Status == "Approved"),
                    Rejected = g.Count(x => x.Status == "Rejected")
                })
                .ToListAsync();

            viewModel.MonthlyApplicationsData = FillMissingMonths(
                monthlyAppsRaw.Select(d => new ChartSeriesData
                {
                    Month = $"{GetMonthName(d.Month)} {d.Year}",
                    ApplicationsCount = d.Count,
                    PendingCount = d.Pending,
                    ApprovedCount = d.Approved,
                    RejectedCount = d.Rejected
                }).ToList(), sixMonthsAgo, today);

            // ==================== APPLICATIONS STATUS PIE CHART ====================
            viewModel.ApplicationsByStatus = new List<ChartSeriesData>
    {
        new ChartSeriesData { Label = "Pending", Value = viewModel.PendingApplications, Color = "#ffc107" },
        new ChartSeriesData { Label = "Approved", Value = viewModel.ApprovedApplications, Color = "#198754" },
        new ChartSeriesData { Label = "Rejected", Value = viewModel.RejectedApplications, Color = "#dc3545" }
    }.Where(x => x.Value > 0).ToList();

            // ==================== RECENT APPLICATIONS ====================
            viewModel.RecentApplications = await _context.Applications
                .Where(a => a.AgentId == agentId.Value)
                .Include(a => a.User)
                .Include(a => a.Policy)
                .OrderByDescending(a => a.ApplyDate)
                .Take(5)
                .Select(a => new RecentApplicationItem
                {
                    ApplicationId = a.ApplicationId,
                    CustomerName = a.User != null ? a.User.FullName : "Unknown",
                    PolicyName = a.Policy != null ? a.Policy.PolicyName : "Unknown",
                    Status = a.Status ?? "Pending",
                    ApplyDate = a.ApplyDate
                })
                .ToListAsync();

            // ==================== RECENT CLAIMS ====================
            viewModel.RecentClaims = await _context.Claims
                .Join(_context.Applications,
                    claim => new { claim.UserId, claim.PolicyId },
                    app => new { app.UserId, app.PolicyId },
                    (claim, app) => new { Claim = claim, App = app })
                .Where(x => x.App.AgentId == agentId.Value)
                .Select(x => new RecentClaimItem
                {
                    ClaimId = x.Claim.ClaimId,
                    CustomerName = x.Claim.User != null ? x.Claim.User.FullName : "Unknown",
                    PolicyName = x.Claim.Policy != null ? x.Claim.Policy.PolicyName : "Unknown",
                    ClaimAmount = x.Claim.ClaimAmount,
                    Status = x.Claim.Status ?? "Pending",
                    ClaimDate = x.Claim.ClaimDate
                })
                .OrderByDescending(c => c.ClaimDate)
                .Take(5)
                .ToListAsync();

            var agent = await _context.Users.FindAsync(agentId.Value);
            ViewBag.AgentName = agent?.FullName ?? "Agent";
            ViewBag.AgentEmail = agent?.Email ?? "";

            return View(viewModel);
        }

        // Helper: Get month name
        private string GetMonthName(int monthNumber)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthNumber);
        }

        // Helper: Fill missing months for chart continuity
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
                    ApplicationsCount = 0,
                    PendingCount = 0,
                    ApprovedCount = 0,
                    RejectedCount = 0
                });

                current = current.AddMonths(1);
            }

            return result.OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy",
                System.Globalization.CultureInfo.InvariantCulture)).ToList();
        }

        // GET: Agent/Applications
        public IActionResult Applications()
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return View(new List<AgentApplicationViewModel>());

            var applications = _context.Applications
                .Include(a => a.User)
                .Include(a => a.Policy)
                .Where(a => a.AgentId == agentId.Value)
                .OrderByDescending(a => a.ApplyDate)
                .ToList();

            var model = applications.Select(app => new AgentApplicationViewModel
            {
                ApplicationId = app.ApplicationId,
                UserId = app.UserId,
                UserName = app.User?.FullName ?? "Unknown",
                PolicyId = app.PolicyId,
                PolicyName = app.Policy?.PolicyName ?? "Unknown",
                AgentId = app.AgentId,
                AgentName = app.Agent?.FullName ?? "-",
                AssignedDate = app.AssignedDate,
                ApplyDate = app.ApplyDate,
                Status = app.Status,
                Remarks = app.Remarks ?? "-"
            }).ToList();

            ViewBag.TotalApplications = model.Count;
            ViewBag.PendingApplications = model.Count(a => a.Status == "Pending");
            ViewBag.ApprovedApplications = model.Count(a => a.Status == "Approved");
            ViewBag.RejectedApplications = model.Count(a => a.Status == "Rejected");

            return View(model);
        }

        // POST: Agent/ApproveApplication/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveApplication(int id)
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return RedirectToAction("Applications");

            var application = _context.Applications.FirstOrDefault(a => a.ApplicationId == id && a.AgentId == agentId.Value);
            if (application == null)
            {
                TempData["Error"] = "Application not found or not assigned to you.";
                return RedirectToAction("Applications");
            }

            if (application.Status != "Approved")
            {
                application.Status = "Approved";
                _context.SaveChanges();
                TempData["Success"] = "Application approved successfully.";
            }
            else
            {
                TempData["Info"] = "Application is already approved.";
            }

            return RedirectToAction("Applications");
        }

        // POST: Agent/RejectApplication/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectApplication(int id)
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return RedirectToAction("Applications");

            var application = _context.Applications.FirstOrDefault(a => a.ApplicationId == id && a.AgentId == agentId.Value);
            if (application == null)
            {
                TempData["Error"] = "Application not found or not assigned to you.";
                return RedirectToAction("Applications");
            }

            if (application.Status != "Rejected")
            {
                application.Status = "Rejected";
                _context.SaveChanges();
                TempData["Success"] = "Application rejected successfully.";
            }
            else
            {
                TempData["Info"] = "Application is already rejected.";
            }

            return RedirectToAction("Applications");
        }

        // POST: Agent/UpdateRemarks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateRemarks(int id, string remarks)
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return RedirectToAction("Applications");

            var application = _context.Applications.FirstOrDefault(a => a.ApplicationId == id && a.AgentId == agentId.Value);
            if (application == null)
            {
                TempData["Error"] = "Application not found or not assigned to you.";
                return RedirectToAction("Applications");
            }

            application.Remarks = remarks ?? string.Empty;
            _context.SaveChanges();
            TempData["Success"] = "Remarks updated successfully.";

            return RedirectToAction("Applications");
        }

        // GET: Agent/ViewApplication/{id}
        public IActionResult ViewApplication(int id)
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return NotFound();

            var app = _context.Applications
                .Include(a => a.User)
                .Include(a => a.Policy)
                .FirstOrDefault(a => a.ApplicationId == id && a.AgentId == agentId.Value);

            if (app == null)
                return NotFound();

            var vm = new AgentApplicationViewModel
            {
                ApplicationId = app.ApplicationId,
                UserName = app.User?.FullName ?? "Unknown",
                PolicyName = app.Policy?.PolicyName ?? "Unknown",
                AgentName = app.AgentId.HasValue ? (_context.Users.Find(app.AgentId)?.FullName ?? "-") : "-",
                ApplyDate = app.ApplyDate,
                Status = app.Status,
                Remarks = app.Remarks ?? "-"
            };

            return PartialView("_ViewApplicationPartial", vm);
        }

        // Helper methods
        private string GetStatusText(string status)
        {
            return status?.ToLower() switch
            {
                "approved" => "Approved",
                "rejected" => "Rejected",
                _ => "Pending"
            };
        }

        private string GetStatusBadgeClass(string status)
        {
            return status?.ToLower() switch
            {
                "approved" => "bg-success-subtle text-success rounded-pill",
                "rejected" => "bg-danger-subtle text-danger rounded-pill",
                _ => "bg-warning-subtle text-warning rounded-pill"
            };
        }

        // GET: Agent/Customers
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return View(new List<AgentCustomerViewModel>());

            var customers = await _context.Applications
                .Where(a => a.AgentId == agentId.Value)
                .Include(a => a.User)
                .Include(a => a.Policy)
                .Select(a => new {
                    Customer = a.User,
                    Policy = a.Policy,
                    ApplicationStatus = a.Status,
                    ApplicationId = a.ApplicationId,
                    ApplyDate = a.ApplyDate
                })
                .ToListAsync();

            var grouped = customers
                .GroupBy(c => c.Customer.UserId)
                .Select(g => new AgentCustomerViewModel
                {
                    UserId = g.Key,
                    FullName = g.First().Customer?.FullName ?? "Unknown",
                    Email = g.First().Customer?.Email ?? "-",
                    Phone = g.First().Customer?.Phone ?? "-",
                    Address = g.First().Customer?.Address ?? "Not provided",
                    Status = g.First().Customer?.Status ?? false,
                    CreatedAt = g.First().Customer?.CreatedAt ?? DateTime.Now,
                    PolicyName = g.First().Policy?.PolicyName ?? "Unknown",
                    ApplicationStatus = g.First().ApplicationStatus ?? "Pending",
                    ApplicationId = g.First().ApplicationId,
                    ApplyDate = g.First().ApplyDate
                })
                .OrderByDescending(c => c.ApplyDate)
                .ToList();

            ViewBag.TotalCustomers = grouped.Count;
            ViewBag.ActiveCustomers = grouped.Count(c => c.Status);
            ViewBag.PendingApps = grouped.Count(c => c.ApplicationStatus == "Pending");
            ViewBag.ApprovedApps = grouped.Count(c => c.ApplicationStatus == "Approved");

            return View(grouped);
        }

        // GET: Agent/Policies
        [HttpGet]
        public async Task<IActionResult> Policies()
        {
            var policies = await _context.Policies
                .Where(p => p.Status)
                .Include(p => p.InsuranceType)
                .OrderBy(p => p.PolicyName)
                .ToListAsync();

            ViewBag.InsuranceTypes = await _context.InsuranceTypes
                .Where(t => t.Name != null)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(policies);
        }

        // GET: Agent/Claims - ✅ FIXED VERSION
        [HttpGet]
        public async Task<IActionResult> Claims()
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return View(new List<AgentClaimViewModel>());

            // ✅ FIXED: Fetch data first
            var query = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Policy)
                .Join(
                    _context.Applications,
                    claim => new { claim.UserId, claim.PolicyId },
                    app => new { app.UserId, app.PolicyId },
                    (claim, app) => new { Claim = claim, Application = app }
                )
                .Where(x => x.Application.AgentId == agentId.Value)
                .ToListAsync();

            // ✅ FIXED: Project to ViewModel AFTER fetching
            var claims = query.Select(x => new AgentClaimViewModel
            {
                ClaimId = x.Claim.ClaimId,
                UserId = x.Claim.UserId,
                UserName = x.Claim.User != null ? x.Claim.User.FullName : "Unknown",
                PolicyId = x.Claim.PolicyId,
                PolicyName = x.Claim.Policy != null ? x.Claim.Policy.PolicyName : "Unknown",
                ClaimAmount = x.Claim.ClaimAmount,
                Reason = x.Claim.Reason,
                ClaimDate = x.Claim.ClaimDate,
                Status = x.Claim.Status ?? "Pending",
                ApplicationId = x.Application.ApplicationId,
                AgentRemarks = x.Application.Remarks
            })
            .OrderByDescending(c => c.ClaimDate)
            .ToList();

            ViewBag.TotalClaims = claims.Count;
            ViewBag.PendingClaims = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            ViewBag.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            ViewBag.RecommendedClaims = claims.Count(c => c.Status != null && c.Status.Contains("Recommended"));

            return View(claims);
        }

        // POST: Agent/AddRemarks - ✅ FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRemarks(int claimId, string remarks)
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return Json(new { success = false, message = "Unauthorized" });

            // ✅ FIXED: Fetch claim first
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
                return Json(new { success = false, message = "Claim not found" });

            // ✅ FIXED: No await inside lambda
            var application = await _context.Applications
                .FirstOrDefaultAsync(a =>
                    a.AgentId == agentId.Value &&
                    a.UserId == claim.UserId &&
                    a.PolicyId == claim.PolicyId);

            if (application == null)
                return Json(new { success = false, message = "Claim not assigned to you" });

            application.Remarks = remarks ?? string.Empty;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Remarks saved successfully" });
        }

        // POST: Agent/RecommendClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecommendClaim(int claimId, string recommendation)
        {
            var agentId = HttpContext.Session.GetInt32("UserId");
            if (!agentId.HasValue)
                return Json(new { success = false, message = "Unauthorized" });

            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
                return Json(new { success = false, message = "Claim not found" });

            var application = await _context.Applications
                .FirstOrDefaultAsync(a =>
                    a.AgentId == agentId.Value &&
                    a.UserId == claim.UserId &&
                    a.PolicyId == claim.PolicyId);

            if (application == null)
                return Json(new { success = false, message = "Claim not assigned to you" });

            claim.Status = recommendation == "Approve" ? "Recommended-Approve" : "Recommended-Reject";

            var recommendationNote = $"[Agent {agentId} recommends: {recommendation} - {DateTime.Now:dd-MMM}]";
            application.Remarks = string.IsNullOrEmpty(application.Remarks)
                ? recommendationNote
                : $"{application.Remarks} | {recommendationNote}";

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Recommendation submitted: {recommendation}",
                newStatus = claim.Status
            });
        }
    }
}