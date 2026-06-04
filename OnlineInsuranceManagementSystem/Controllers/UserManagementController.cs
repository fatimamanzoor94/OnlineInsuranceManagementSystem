using BCrypt.Net;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OnlineInsuranceManagementSystem.Data;
using OnlineInsuranceManagementSystem.Models;
using OnlineInsuranceManagementSystem.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineInsuranceManagementSystem.Controllers
{
    public class UserManagementController : BaseAdminController
    {
        private readonly IEmailService _emailService;

        public UserManagementController(ApplicationDbContext context, IEmailService emailService)
            : base(context)
        {
            _emailService = emailService;
        }

        // READ (LIST)
        public IActionResult User()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // CREATE
        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("User");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("User");
        }

        // EDIT (GET)
        [HttpPost]
        public IActionResult Edit(User updatedUser)
        {
            var user = _context.Users.Find(updatedUser.UserId);

            if (user == null)
                return NotFound();

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.Phone = updatedUser.Phone;
            user.Role = updatedUser.Role;
            user.Status = updatedUser.Status;
            user.Address = updatedUser.Address;

            _context.SaveChanges();

            return RedirectToAction("User");
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("User");
        }

        // Customers
        public IActionResult Customers()
        {
            var customers = _context.Users
                                    .Where(x => x.Role == "Customer")
                                    .ToList();

            return View(customers);
        }

        //Customer Create (Add) Action :
        [HttpPost]
        public IActionResult CreateCustomer(User customer)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Customers");

            customer.Role = "Customer";

            customer.Password =
                BCrypt.Net.BCrypt.HashPassword(customer.Password);

            customer.CreatedAt = DateTime.Now;

            _context.Users.Add(customer);

            _context.SaveChanges();

            return RedirectToAction("Customers");
        }

        // Customer Update (Edit) Action :
        [HttpPost]
        public IActionResult EditCustomer(User updatedCustomer)
        {
            var customer =
                _context.Users.Find(updatedCustomer.UserId);

            if (customer == null)
                return NotFound();

            customer.FullName = updatedCustomer.FullName;

            customer.Email = updatedCustomer.Email;

            customer.Phone = updatedCustomer.Phone;

            customer.Address = updatedCustomer.Address;

            customer.Status = updatedCustomer.Status;

            _context.SaveChanges();

            return RedirectToAction("Customers");
        }

        // Customer Delete Action :
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _context.Users.Find(id);

            if (customer != null)
            {
                _context.Users.Remove(customer);

                _context.SaveChanges();
            }

            return RedirectToAction("Customers");
        }

        // READ: List Agents
        public async Task<IActionResult> Agents()
        {
            var agents = await _context.Users
                .Where(x => x.Role == "Agent")
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return View(agents);
        }

        // CREATE: Add New Agent
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateAgent(User agent)
{
    try
    {
        // Check email exists
        if (await _context.Users.AnyAsync(x => x.Email == agent.Email))
        {
            TempData["Error"] = "Email already registered.";
            return RedirectToAction("Agents");
        }

        // Set required values
        agent.Role = "Agent";

        // Save original password for email
        string plainPassword = agent.Password;

        // Hash password
        agent.Password = BCrypt.Net.BCrypt.HashPassword(agent.Password);

        agent.CreatedAt = DateTime.Now;

        agent.AccountStatus = "Pending";

        // Use form selected status
        // agent.Status already coming from form

        await _context.Users.AddAsync(agent);

        await _context.SaveChangesAsync();

        // Send email
        _ = Task.Run(async () =>
            await _emailService.SendAgentAccountCreatedEmailAsync(
                agent.Email,
                agent.FullName,
                plainPassword
            )
        );

        TempData["Success"] = "Agent created successfully!";
    }
    catch (Exception ex)
    {
        TempData["Error"] = $"Error creating agent: {ex.Message}";
    }

    return RedirectToAction("Agents");
}

        // UPDATE: Edit Agent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAgent(User updatedAgent)
        {
            var agent = await _context.Users.FindAsync(updatedAgent.UserId);

            if (agent == null || agent.Role != "Agent")
                return NotFound();

            try
            {
                // Update only editable fields
                agent.FullName = updatedAgent.FullName;
                agent.Email = updatedAgent.Email;
                agent.Phone = updatedAgent.Phone;
                agent.Address = updatedAgent.Address;
                agent.Status = updatedAgent.Status;

                _context.Users.Update(agent);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Agent updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating agent: {ex.Message}";
            }

            return RedirectToAction("Agents");
        }

        // DELETE: Remove Agent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var agent = await _context.Users.FindAsync(id);
            if (agent == null || agent.Role != "Agent")
                return NotFound();

            try
            {
                _context.Users.Remove(agent);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Agent deleted successfully.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error deleting agent: {ex.Message}";
            }

            return RedirectToAction("Agents");
        }

        // APPROVE: Change AccountStatus from Pending to Approved
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAgent(int id)
        {
            var agent = await _context.Users.FindAsync(id);
            if (agent == null || agent.Role != "Agent")
                return NotFound();

            if (agent.AccountStatus == "Approved")
            {
                TempData["Info"] = "Agent is already approved.";
                return RedirectToAction("Agents");
            }

            try
            {
                agent.AccountStatus = "Approved";
                _context.Users.Update(agent);
                await _context.SaveChangesAsync();

                // 📧 Send approval email async
                _ = Task.Run(async () =>
                    await _emailService.SendAgentApprovalEmailAsync(agent.Email, agent.FullName)
                );

                TempData["Success"] = "Agent approved successfully! Approval email sent.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error approving agent: {ex.Message}";
            }

            return RedirectToAction("Agents");
        }

        // TOGGLE STATUS: Active/Inactive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAgentStatus(int id)
        {
            var agent = await _context.Users.FindAsync(id);
            if (agent == null || agent.Role != "Agent")
                return NotFound();

            try
            {
                agent.Status = !agent.Status;
                _context.Users.Update(agent);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Agent status changed to {(agent.Status ? "Active" : "Inactive")}.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Error toggling status: {ex.Message}";
            }

            return RedirectToAction("Agents");
        }

        // SEARCH API: For AJAX live search (optional enhancement)
        [HttpGet]
        public async Task<IActionResult> SearchAgents(string searchTerm, string statusFilter, string approvalFilter)
        {
            var query = _context.Users.Where(x => x.Role == "Agent").AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    x.FullName.Contains(searchTerm) ||
                    x.Email.Contains(searchTerm) ||
                    x.Phone.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                bool isActive = statusFilter.ToLower() == "active";
                query = query.Where(x => x.Status == isActive);
            }

            if (!string.IsNullOrWhiteSpace(approvalFilter))
            {
                query = query.Where(x => x.AccountStatus == approvalFilter);
            }

            var agents = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return PartialView("_AgentsTablePartial", agents); // Create partial view if needed
        }
    }
}