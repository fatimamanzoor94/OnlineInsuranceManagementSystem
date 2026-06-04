// ViewModels/AgentApplicationViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class AgentApplicationViewModel
    {
        public int ApplicationId { get; set; }

        // Display format: APP-1234
        public string ApplicationIdDisplay => $"APP-{ApplicationId}";

        [Required]
        public int UserId { get; set; }
        public string? UserName { get; set; }

        [Required]
        public int PolicyId { get; set; }
        public string? PolicyName { get; set; }

        public int? AgentId { get; set; }
        public string? AgentName { get; set; }

        // ✅ NEW: Assignment tracking
        public DateTime? AssignedDate { get; set; }
        public string AssignedDateDisplay => AssignedDate?.ToString("dd MMM yyyy, hh:mm tt") ?? "-";

        public DateTime ApplyDate { get; set; }
        public string ApplyDateDisplay => ApplyDate.ToString("dd MMM yyyy");

        [StringLength(20)]
        public string? Status { get; set; } = "Pending";

        public string StatusText => Status?.ToLower() switch
        {
            "approved" => "Approved",
            "rejected" => "Rejected",
            _ => "Pending"
        };

        public string StatusBadgeClass => Status?.ToLower() switch
        {
            "approved" => "bg-success-subtle text-success rounded-pill",
            "rejected" => "bg-danger-subtle text-danger rounded-pill",
            _ => "bg-warning-subtle text-warning rounded-pill"
        };

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}