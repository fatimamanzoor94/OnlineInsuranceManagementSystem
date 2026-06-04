// ViewModels/AgentCustomerViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class AgentCustomerViewModel
    {
        public int UserId { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // ✅ Application & Policy Info (Agent-specific)
        public string? PolicyName { get; set; }
        public string? ApplicationStatus { get; set; }
        public int ApplicationId { get; set; }
        public DateTime ApplyDate { get; set; }

        // ✅ Display helpers
        public string CustomerIdDisplay => $"CUS-{UserId}";
        public string ApplyDateDisplay => ApplyDate.ToString("dd MMM yyyy");
        public string StatusBadgeClass => Status
            ? "bg-success-subtle text-success rounded-pill"
            : "bg-danger-subtle text-danger rounded-pill";

        public string AppStatusBadgeClass => ApplicationStatus?.ToLower() switch
        {
            "approved" => "bg-success-subtle text-success rounded-pill",
            "rejected" => "bg-danger-subtle text-danger rounded-pill",
            _ => "bg-warning-subtle text-warning rounded-pill"
        };
    }
}