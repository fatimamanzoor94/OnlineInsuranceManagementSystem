using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using OnlineInsuranceManagementSystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnlineInsuranceManagementSystem.Controllers
{
    public class InsuranceManagementController : BaseAdminController
    {

        public InsuranceManagementController(ApplicationDbContext context) : base(context)
        {
            //_context = context;
        }

        // ==================== INSURANCE TYPES ====================
        public async Task<IActionResult> InsuranceTypes()
        {
            var insuranceTypes = await _context.InsuranceTypes
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return View(insuranceTypes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInsuranceType(InsuranceType model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _context.InsuranceTypes.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(InsuranceTypes));
            }
            return View("InsuranceTypes", await _context.InsuranceTypes.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInsuranceType(InsuranceType model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.InsuranceTypes.FindAsync(model.InsuranceTypeId);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Description = model.Description;
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(InsuranceTypes));
            }
            return View("InsuranceTypes", await _context.InsuranceTypes.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInsuranceType(int id)
        {
            var item = await _context.InsuranceTypes.FindAsync(id);
            if (item != null)
            {
                _context.InsuranceTypes.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(InsuranceTypes));
        }

        // ==================== POLICIES (General) ====================
        public IActionResult Policies()
        {
            var policies = _context.Policies
                .Include(p => p.InsuranceType)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.InsuranceTypes = _context.InsuranceTypes.ToList();
            return View(policies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePolicy(Policy model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.InsuranceTypes = _context.InsuranceTypes.ToList();
                    TempData["Error"] = "Validation failed. Check input.";
                    return View("Policies", _context.Policies.Include(p => p.InsuranceType).ToList());
                }

                model.CreatedAt = DateTime.Now;
                _context.Policies.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Policy created successfully!";
                return RedirectToAction("Policies");
            }
            catch (Exception ex)
            {
                ViewBag.InsuranceTypes = _context.InsuranceTypes.ToList();
                TempData["Error"] = $"Database Error: {ex.Message}";
                return View("Policies", _context.Policies.Include(p => p.InsuranceType).ToList());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPolicy(Policy model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InsuranceTypes = _context.InsuranceTypes.ToList();
                return View("Policies", _context.Policies.Include(p => p.InsuranceType).ToList());
            }

            var existing = _context.Policies.Find(model.PolicyId);
            if (existing == null) return NotFound();

            existing.PolicyName = model.PolicyName;
            existing.InsuranceTypeId = model.InsuranceTypeId;
            existing.PremiumAmount = model.PremiumAmount;
            existing.Duration = model.Duration;
            existing.CoverageAmount = model.CoverageAmount;
            existing.Terms = model.Terms;
            existing.Status = model.Status;

            _context.SaveChanges();
            TempData["Success"] = "Policy updated successfully!";
            return RedirectToAction("Policies");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePolicy(int id)
        {
            var policy = _context.Policies.Find(id);
            if (policy != null)
            {
                _context.Policies.Remove(policy);
                _context.SaveChanges();
                TempData["Success"] = "Policy deleted successfully!";
            }
            return RedirectToAction("Policies");
        }

        // ==================== 🎯 LIFE INSURANCE (FIXED) ====================

        // GET: LifeInsurance - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> LifeInsurance()
        {
            var lifeInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Life");

            if (lifeInsuranceType == null)
            {
                TempData["Error"] = "Life Insurance type not configured in database.";
                return View(new List<LifeInsurancePolicyViewModel>());
            }

            // Fetch policies + Map to ViewModel
            var lifePolicies = await _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == lifeInsuranceType.InsuranceTypeId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // === MAP Policy → LifeInsurancePolicyViewModel ===
            var viewModelList = lifePolicies.Select(p => new LifeInsurancePolicyViewModel
            {
                PolicyId = p.PolicyId,
                PolicyName = p.PolicyName,
                InsuranceTypeName = p.InsuranceType?.Name ?? "Life Insurance",
                PremiumAmount = p.PremiumAmount,
                Duration = p.Duration,
                CoverageAmount = p.CoverageAmount,
                Terms = p.Terms,
                IsActive = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList();

            // === CALCULATE SUMMARY STATS FOR CARDS ===
            ViewBag.TotalPolicies = viewModelList.Count;
            ViewBag.ActivePolicies = viewModelList.Count(p => p.IsActive);
            ViewBag.PremiumPlans = viewModelList.Count(p => p.PremiumAmount >= 500);
            ViewBag.TotalCoverage = viewModelList.Sum(p => p.CoverageAmount);

            ViewBag.InsuranceTypes = await _context.InsuranceTypes.ToListAsync();

            return View(viewModelList); // ✅ Correct type ab pass ho raha hai
        }

        // POST: AJAX Search/Filter for Life Insurance
        [HttpPost]
        [Route("InsuranceManagement/SearchLifeInsurance")]
        public async Task<IActionResult> SearchLifeInsurance(string searchTerm, string filter)
        {
            var lifeInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Life");

            if (lifeInsuranceType == null)
                return Json(new List<LifeInsurancePolicyViewModel>());

            var query = _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == lifeInsuranceType.InsuranceTypeId);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.PolicyId.ToString().Contains(searchTerm) ||
                    p.PolicyName.Contains(searchTerm) ||
                    p.CoverageAmount.ToString().Contains(searchTerm));
            }

            // Apply filter
            if (!string.IsNullOrEmpty(filter) && filter != "all")
            {
                filter = filter.ToLower();
                query = filter switch
                {
                    "active" => query.Where(p => p.Status == true),
                    "inactive" => query.Where(p => p.Status == false),
                    "premium" => query.Where(p => p.PremiumAmount >= 500),
                    _ => query
                };
            }

            var results = await query
                .Select(p => new LifeInsurancePolicyViewModel
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    InsuranceTypeName = p.InsuranceType != null ? p.InsuranceType.Name : "Life Insurance",
                    PremiumAmount = p.PremiumAmount,
                    Duration = p.Duration,
                    CoverageAmount = p.CoverageAmount,
                    Terms = p.Terms,
                    IsActive = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Json(results);
        }

        // GET: AJAX Load Policy Details
        [HttpGet]
        [Route("InsuranceManagement/GetPolicyDetails")]
        public async Task<IActionResult> GetPolicyDetails(int policyId)
        {
            var lifeInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Life");

            if (lifeInsuranceType == null)
                return NotFound();

            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == policyId &&
                                         p.InsuranceTypeId == lifeInsuranceType.InsuranceTypeId);

            if (policy == null)
                return NotFound();

            var viewModel = new LifeInsurancePolicyViewModel
            {
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName,
                InsuranceTypeName = policy.InsuranceType?.Name ?? "Life Insurance",
                PremiumAmount = policy.PremiumAmount,
                Duration = policy.Duration,
                CoverageAmount = policy.CoverageAmount,
                Terms = policy.Terms,
                IsActive = policy.Status,
                CreatedAt = policy.CreatedAt
            };

            return Json(viewModel);
        }

        // POST: Create Life Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLifeInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var lifeInsuranceType = await _context.InsuranceTypes
                    .FirstOrDefaultAsync(t => t.Name == "Life Insurance");

                if (lifeInsuranceType != null)
                {
                    model.InsuranceTypeId = lifeInsuranceType.InsuranceTypeId;
                    model.CreatedAt = DateTime.UtcNow;
                    model.Status = true; // Default active

                    _context.Policies.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Life Insurance policy created successfully.";
                    return RedirectToAction(nameof(LifeInsurance));
                }
            }

            TempData["Error"] = "Failed to create policy. Please check your input.";
            return RedirectToAction(nameof(LifeInsurance));
        }

        // POST: Edit Life Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLifeInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var existingPolicy = await _context.Policies
                    .FirstOrDefaultAsync(p => p.PolicyId == model.PolicyId);

                if (existingPolicy != null)
                {
                    var lifeInsuranceType = await _context.InsuranceTypes
                        .FirstOrDefaultAsync(t => t.Name == "Life Insurance");

                    if (lifeInsuranceType != null)
                    {
                        existingPolicy.PolicyName = model.PolicyName;
                        existingPolicy.InsuranceTypeId = lifeInsuranceType.InsuranceTypeId;
                        existingPolicy.PremiumAmount = model.PremiumAmount;
                        existingPolicy.Duration = model.Duration;
                        existingPolicy.CoverageAmount = model.CoverageAmount;
                        existingPolicy.Terms = model.Terms;
                        existingPolicy.Status = model.Status;

                        _context.Policies.Update(existingPolicy);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Life Insurance policy updated successfully.";
                        return RedirectToAction(nameof(LifeInsurance));
                    }
                }
            }

            TempData["Error"] = "Failed to update policy. Please check your input.";
            return RedirectToAction(nameof(LifeInsurance));
        }

        // POST: Delete Life Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLifeInsurance(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy != null && policy.InsuranceType?.Name == "Life Insurance")
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Life Insurance policy deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Policy not found or cannot be deleted.";
            }

            return RedirectToAction(nameof(LifeInsurance));
        }

        // ==================== 🎯 MEDICAL INSURANCE ====================

        // GET: MedicalInsurance - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> MedicalInsurance()
        {
            var medicalInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Health");

            if (medicalInsuranceType == null)
            {
                TempData["Error"] = "Medical Insurance type not configured in database.";
                return View(new List<MedicalInsurancePolicyViewModel>());
            }

            // Fetch policies + Map to ViewModel
            var medicalPolicies = await _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == medicalInsuranceType.InsuranceTypeId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // === MAP Policy → MedicalInsurancePolicyViewModel ===
            var viewModelList = medicalPolicies.Select(p => new MedicalInsurancePolicyViewModel
            {
                PolicyId = p.PolicyId,
                PolicyName = p.PolicyName,
                InsuranceTypeName = p.InsuranceType?.Name ?? "Medical Insurance",
                PremiumAmount = p.PremiumAmount,
                Duration = p.Duration,
                CoverageAmount = p.CoverageAmount,
                Terms = p.Terms,
                IsActive = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList();

            // === CALCULATE SUMMARY STATS FOR CARDS ===
            ViewBag.TotalPolicies = viewModelList.Count;
            ViewBag.ActivePolicies = viewModelList.Count(p => p.IsActive);
            ViewBag.PremiumPlans = viewModelList.Count(p => p.PremiumAmount >= 500);
            ViewBag.TotalCoverage = viewModelList.Sum(p => p.CoverageAmount);

            return View(viewModelList);
        }

        // POST: AJAX Search/Filter for Medical Insurance
        [HttpPost]
        [Route("InsuranceManagement/SearchMedicalInsurance")]
        public async Task<IActionResult> SearchMedicalInsurance(string searchTerm, string filter)
        {
            var medicalInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Health");

            if (medicalInsuranceType == null)
                return Json(new List<MedicalInsurancePolicyViewModel>());

            var query = _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == medicalInsuranceType.InsuranceTypeId);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.PolicyId.ToString().Contains(searchTerm) ||
                    p.PolicyName.Contains(searchTerm) ||
                    p.CoverageAmount.ToString().Contains(searchTerm));
            }

            // Apply filter
            if (!string.IsNullOrEmpty(filter) && filter != "all")
            {
                filter = filter.ToLower();
                query = filter switch
                {
                    "active" => query.Where(p => p.Status == true),
                    "inactive" => query.Where(p => p.Status == false),
                    "premium" => query.Where(p => p.PremiumAmount >= 500),
                    _ => query
                };
            }

            var results = await query
                .Select(p => new MedicalInsurancePolicyViewModel
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    InsuranceTypeName = p.InsuranceType != null ? p.InsuranceType.Name : "Medical Insurance",
                    PremiumAmount = p.PremiumAmount,
                    Duration = p.Duration,
                    CoverageAmount = p.CoverageAmount,
                    Terms = p.Terms,
                    IsActive = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Json(results);
        }

        // GET: AJAX Load Medical Policy Details
        [HttpGet]
        [Route("InsuranceManagement/GetMedicalPolicyDetails")]
        public async Task<IActionResult> GetMedicalPolicyDetails(int policyId)
        {
            var medicalInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Health");

            if (medicalInsuranceType == null)
                return NotFound();

            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == policyId &&
                                         p.InsuranceTypeId == medicalInsuranceType.InsuranceTypeId);

            if (policy == null)
                return NotFound();

            var viewModel = new MedicalInsurancePolicyViewModel
            {
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName,
                InsuranceTypeName = policy.InsuranceType?.Name ?? "Medical Insurance",
                PremiumAmount = policy.PremiumAmount,
                Duration = policy.Duration,
                CoverageAmount = policy.CoverageAmount,
                Terms = policy.Terms,
                IsActive = policy.Status,
                CreatedAt = policy.CreatedAt
            };

            return Json(viewModel);
        }

        // POST: Create Medical Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicalInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var medicalInsuranceType = await _context.InsuranceTypes
                    .FirstOrDefaultAsync(t => t.Name == "Health");

                if (medicalInsuranceType != null)
                {
                    model.InsuranceTypeId = medicalInsuranceType.InsuranceTypeId;
                    model.CreatedAt = DateTime.UtcNow;
                    model.Status = true; // Default active

                    _context.Policies.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Medical Insurance policy created successfully.";
                    return RedirectToAction(nameof(MedicalInsurance));
                }
            }

            TempData["Error"] = "Failed to create policy. Please check your input.";
            return RedirectToAction(nameof(MedicalInsurance));
        }

        // POST: Edit Medical Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var existingPolicy = await _context.Policies
                    .FirstOrDefaultAsync(p => p.PolicyId == model.PolicyId);

                if (existingPolicy != null)
                {
                    var medicalInsuranceType = await _context.InsuranceTypes
                        .FirstOrDefaultAsync(t => t.Name == "Health");

                    if (medicalInsuranceType != null)
                    {
                        existingPolicy.PolicyName = model.PolicyName;
                        existingPolicy.InsuranceTypeId = medicalInsuranceType.InsuranceTypeId;
                        existingPolicy.PremiumAmount = model.PremiumAmount;
                        existingPolicy.Duration = model.Duration;
                        existingPolicy.CoverageAmount = model.CoverageAmount;
                        existingPolicy.Terms = model.Terms;
                        existingPolicy.Status = model.Status;

                        _context.Policies.Update(existingPolicy);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Medical Insurance policy updated successfully.";
                        return RedirectToAction(nameof(MedicalInsurance));
                    }
                }
            }

            TempData["Error"] = "Failed to update policy. Please check your input.";
            return RedirectToAction(nameof(MedicalInsurance));
        }

        // POST: Delete Medical Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicalInsurance(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy != null && policy.InsuranceType?.Name == "Medical Insurance")
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Medical Insurance policy deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Policy not found or cannot be deleted.";
            }

            return RedirectToAction(nameof(MedicalInsurance));
        }

        // ==================== 🚗 MOTOR INSURANCE ====================

        // GET: MotorInsurance - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> MotorInsurance()
        {
            var motorInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Motor");

            if (motorInsuranceType == null)
            {
                TempData["Error"] = "Motor Insurance type not configured in database.";
                return View(new List<MotorInsurancePolicyViewModel>());
            }

            // Fetch policies + Map to ViewModel
            var motorPolicies = await _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == motorInsuranceType.InsuranceTypeId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // === MAP Policy → MotorInsurancePolicyViewModel ===
            var viewModelList = motorPolicies.Select(p => new MotorInsurancePolicyViewModel
            {
                PolicyId = p.PolicyId,
                PolicyName = p.PolicyName,
                InsuranceTypeName = p.InsuranceType?.Name ?? "Motor Insurance",
                PremiumAmount = p.PremiumAmount,
                Duration = p.Duration,
                CoverageAmount = p.CoverageAmount,
                Terms = p.Terms,
                IsActive = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList();

            // === CALCULATE SUMMARY STATS FOR CARDS ===
            ViewBag.TotalPolicies = viewModelList.Count;
            ViewBag.ActivePolicies = viewModelList.Count(p => p.IsActive);
            ViewBag.PremiumPlans = viewModelList.Count(p => p.PremiumAmount >= 500);
            ViewBag.TotalCoverage = viewModelList.Sum(p => p.CoverageAmount);

            return View(viewModelList);
        }

        // POST: AJAX Search/Filter for Motor Insurance
        [HttpPost]
        [Route("InsuranceManagement/SearchMotorInsurance")]
        public async Task<IActionResult> SearchMotorInsurance(string searchTerm, string filter)
        {
            var motorInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Motor");

            if (motorInsuranceType == null)
                return Json(new List<MotorInsurancePolicyViewModel>());

            var query = _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == motorInsuranceType.InsuranceTypeId);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.PolicyId.ToString().Contains(searchTerm) ||
                    p.PolicyName.Contains(searchTerm) ||
                    p.CoverageAmount.ToString().Contains(searchTerm));
            }

            // Apply filter
            if (!string.IsNullOrEmpty(filter) && filter != "all")
            {
                filter = filter.ToLower();
                query = filter switch
                {
                    "active" => query.Where(p => p.Status == true),
                    "inactive" => query.Where(p => p.Status == false),
                    "premium" => query.Where(p => p.PremiumAmount >= 500),
                    _ => query
                };
            }

            var results = await query
                .Select(p => new MotorInsurancePolicyViewModel
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    InsuranceTypeName = p.InsuranceType != null ? p.InsuranceType.Name : "Motor Insurance",
                    PremiumAmount = p.PremiumAmount,
                    Duration = p.Duration,
                    CoverageAmount = p.CoverageAmount,
                    Terms = p.Terms,
                    IsActive = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Json(results);
        }

        // GET: AJAX Load Motor Policy Details
        [HttpGet]
        [Route("InsuranceManagement/GetMotorPolicyDetails")]
        public async Task<IActionResult> GetMotorPolicyDetails(int policyId)
        {
            var motorInsuranceType = await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == "Motor");

            if (motorInsuranceType == null)
                return NotFound();

            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == policyId &&
                                         p.InsuranceTypeId == motorInsuranceType.InsuranceTypeId);

            if (policy == null)
                return NotFound();

            var viewModel = new MotorInsurancePolicyViewModel
            {
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName,
                InsuranceTypeName = policy.InsuranceType?.Name ?? "Motor Insurance",
                PremiumAmount = policy.PremiumAmount,
                Duration = policy.Duration,
                CoverageAmount = policy.CoverageAmount,
                Terms = policy.Terms,
                IsActive = policy.Status,
                CreatedAt = policy.CreatedAt
            };

            return Json(viewModel);
        }

        // POST: Create Motor Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMotorInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var motorInsuranceType = await _context.InsuranceTypes
                    .FirstOrDefaultAsync(t => t.Name == "Motor");

                if (motorInsuranceType != null)
                {
                    model.InsuranceTypeId = motorInsuranceType.InsuranceTypeId;
                    model.CreatedAt = DateTime.UtcNow;
                    model.Status = true; // Default active

                    _context.Policies.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Motor Insurance policy created successfully.";
                    return RedirectToAction(nameof(MotorInsurance));
                }
            }

            TempData["Error"] = "Failed to create policy. Please check your input.";
            return RedirectToAction(nameof(MotorInsurance));
        }

        // POST: Edit Motor Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMotorInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var existingPolicy = await _context.Policies
                    .FirstOrDefaultAsync(p => p.PolicyId == model.PolicyId);

                if (existingPolicy != null)
                {
                    var motorInsuranceType = await _context.InsuranceTypes
                        .FirstOrDefaultAsync(t => t.Name == "Motor");

                    if (motorInsuranceType != null)
                    {
                        existingPolicy.PolicyName = model.PolicyName;
                        existingPolicy.InsuranceTypeId = motorInsuranceType.InsuranceTypeId;
                        existingPolicy.PremiumAmount = model.PremiumAmount;
                        existingPolicy.Duration = model.Duration;
                        existingPolicy.CoverageAmount = model.CoverageAmount;
                        existingPolicy.Terms = model.Terms;
                        existingPolicy.Status = model.Status;

                        _context.Policies.Update(existingPolicy);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Motor Insurance policy updated successfully.";
                        return RedirectToAction(nameof(MotorInsurance));
                    }
                }
            }

            TempData["Error"] = "Failed to update policy. Please check your input.";
            return RedirectToAction(nameof(MotorInsurance));
        }

        // POST: Delete Motor Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMotorInsurance(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy != null && policy.InsuranceType?.Name == "Motor Insurance")
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Motor Insurance policy deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Policy not found or cannot be deleted.";
            }

            return RedirectToAction(nameof(MotorInsurance));
        }

        // ==================== 🏠 HOME INSURANCE ====================

        // GET: HomeInsurance - MAIN PAGE
        [HttpGet]
        public async Task<IActionResult> HomeInsurance()
        {
            var homeInsuranceType = await GetInsuranceType("Home");

            if (homeInsuranceType == null)
            {
                TempData["Error"] = "Home Insurance type not configured in database.";
                return View(new List<HomeInsurancePolicyViewModel>());
            }

            // Fetch policies + Map to ViewModel
            var homePolicies = await _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == homeInsuranceType.InsuranceTypeId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // === MAP Policy → HomeInsurancePolicyViewModel ===
            var viewModelList = homePolicies.Select(p => new HomeInsurancePolicyViewModel
            {
                PolicyId = p.PolicyId,
                PolicyName = p.PolicyName,
                InsuranceTypeName = p.InsuranceType?.Name ?? "Home Insurance",
                PremiumAmount = p.PremiumAmount,
                Duration = p.Duration,
                CoverageAmount = p.CoverageAmount,
                Terms = p.Terms,
                IsActive = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList();

            // === CALCULATE SUMMARY STATS FOR CARDS ===
            ViewBag.TotalPolicies = viewModelList.Count;
            ViewBag.ActivePolicies = viewModelList.Count(p => p.IsActive);
            ViewBag.PremiumPlans = viewModelList.Count(p => p.PremiumAmount >= 500);
            ViewBag.TotalCoverage = viewModelList.Sum(p => p.CoverageAmount);

            return View(viewModelList);
        }

        // POST: AJAX Search/Filter for Home Insurance
        [HttpPost]
        [Route("InsuranceManagement/SearchHomeInsurance")]
        public async Task<IActionResult> SearchHomeInsurance(string searchTerm, string filter)
        {
            var homeInsuranceType = await GetInsuranceType("Home");

            if (homeInsuranceType == null)
                return Json(new List<HomeInsurancePolicyViewModel>());

            var query = _context.Policies
                .Include(p => p.InsuranceType)
                .Where(p => p.InsuranceTypeId == homeInsuranceType.InsuranceTypeId);

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.PolicyId.ToString().Contains(searchTerm) ||
                    p.PolicyName.Contains(searchTerm) ||
                    p.CoverageAmount.ToString().Contains(searchTerm));
            }

            // Apply filter
            if (!string.IsNullOrEmpty(filter) && filter != "all")
            {
                filter = filter.ToLower();
                query = filter switch
                {
                    "active" => query.Where(p => p.Status == true),
                    "inactive" => query.Where(p => p.Status == false),
                    "premium" => query.Where(p => p.PremiumAmount >= 500),
                    _ => query
                };
            }

            var results = await query
                .Select(p => new HomeInsurancePolicyViewModel
                {
                    PolicyId = p.PolicyId,
                    PolicyName = p.PolicyName,
                    InsuranceTypeName = p.InsuranceType != null ? p.InsuranceType.Name : "Home Insurance",
                    PremiumAmount = p.PremiumAmount,
                    Duration = p.Duration,
                    CoverageAmount = p.CoverageAmount,
                    Terms = p.Terms,
                    IsActive = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Json(results);
        }

        // GET: AJAX Load Home Policy Details
        [HttpGet]
        [Route("InsuranceManagement/GetHomePolicyDetails")]
        public async Task<IActionResult> GetHomePolicyDetails(int policyId)
        {
            var homeInsuranceType = await GetInsuranceType("Home");

            if (homeInsuranceType == null)
                return NotFound();

            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == policyId &&
                                         p.InsuranceTypeId == homeInsuranceType.InsuranceTypeId);

            if (policy == null)
                return NotFound();

            var viewModel = new HomeInsurancePolicyViewModel
            {
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName,
                InsuranceTypeName = policy.InsuranceType?.Name ?? "Home Insurance",
                PremiumAmount = policy.PremiumAmount,
                Duration = policy.Duration,
                CoverageAmount = policy.CoverageAmount,
                Terms = policy.Terms,
                IsActive = policy.Status,
                CreatedAt = policy.CreatedAt
            };

            return Json(viewModel);
        }

        // POST: Create Home Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHomeInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var homeInsuranceType = await GetInsuranceType("Home");

                if (homeInsuranceType != null)
                {
                    model.InsuranceTypeId = homeInsuranceType.InsuranceTypeId;
                    model.CreatedAt = DateTime.UtcNow;
                    model.Status = true; // Default active

                    _context.Policies.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Home Insurance policy created successfully.";
                    return RedirectToAction(nameof(HomeInsurance));
                }
            }

            TempData["Error"] = "Failed to create policy. Please check your input.";
            return RedirectToAction(nameof(HomeInsurance));
        }

        // POST: Edit Home Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHomeInsurance(Policy model)
        {
            if (ModelState.IsValid)
            {
                var existingPolicy = await _context.Policies
                    .FirstOrDefaultAsync(p => p.PolicyId == model.PolicyId);

                if (existingPolicy != null)
                {
                    var homeInsuranceType = await _context.InsuranceTypes
                        .FirstOrDefaultAsync(t => t.Name == "Home Insurance");

                    if (homeInsuranceType != null)
                    {
                        existingPolicy.PolicyName = model.PolicyName;
                        existingPolicy.InsuranceTypeId = homeInsuranceType.InsuranceTypeId;
                        existingPolicy.PremiumAmount = model.PremiumAmount;
                        existingPolicy.Duration = model.Duration;
                        existingPolicy.CoverageAmount = model.CoverageAmount;
                        existingPolicy.Terms = model.Terms;
                        existingPolicy.Status = model.Status;

                        _context.Policies.Update(existingPolicy);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Home Insurance policy updated successfully.";
                        return RedirectToAction(nameof(HomeInsurance));
                    }
                }
            }

            TempData["Error"] = "Failed to update policy. Please check your input.";
            return RedirectToAction(nameof(HomeInsurance));
        }

        // POST: Delete Home Insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHomeInsurance(int id)
        {
            var policy = await _context.Policies
                .Include(p => p.InsuranceType)
                .FirstOrDefaultAsync(p => p.PolicyId == id);

            if (policy != null && policy.InsuranceType?.Name == "Home Insurance")
            {
                _context.Policies.Remove(policy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Home Insurance policy deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Policy not found or cannot be deleted.";
            }

            return RedirectToAction(nameof(HomeInsurance));
        }

        private async Task<InsuranceType> GetInsuranceType(string name)
        {
            return await _context.InsuranceTypes
                .FirstOrDefaultAsync(t => t.Name == name);
        }
    }
}
