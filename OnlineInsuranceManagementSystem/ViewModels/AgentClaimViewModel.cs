// ViewModels/AgentClaimViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class AgentClaimViewModel
    {
        public int ClaimId { get; set; }
        public string ClaimIdDisplay => $"CLM-{ClaimId}";

        public int UserId { get; set; }
        public string? UserName { get; set; }

        public int PolicyId { get; set; }
        public string? PolicyName { get; set; }

        public decimal ClaimAmount { get; set; }
        public string ClaimAmountDisplay => $"${ClaimAmount:N2}";

        public string? Reason { get; set; }
        public DateTime ClaimDate { get; set; }
        public string ClaimDateDisplay => ClaimDate.ToString("dd MMM yyyy");

        public string? Status { get; set; } = "Pending";

        public string StatusText => Status?.ToLower() switch
        {
            "approved" => "Approved",
            "rejected" => "Rejected",
            "recommended-approve" => "Rec: Approve",
            "recommended-reject" => "Rec: Reject",
            _ => "Pending"
        };

        public string StatusBadgeClass => Status?.ToLower() switch
        {
            "approved" => "bg-success-subtle text-success rounded-pill",
            "rejected" => "bg-danger-subtle text-danger rounded-pill",
            "recommended-approve" => "bg-info-subtle text-info rounded-pill",
            "recommended-reject" => "bg-warning-subtle text-warning rounded-pill",
            _ => "bg-secondary-subtle text-secondary rounded-pill"
        };

        // ✅ Agent-specific fields
        public int ApplicationId { get; set; }
        public string? AgentRemarks { get; set; }
    }
}