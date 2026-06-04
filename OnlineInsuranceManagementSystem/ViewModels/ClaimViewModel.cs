using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class ClaimViewModel
    {
        public int ClaimId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal ClaimAmount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public DateTime ClaimDate { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // === Display Helpers ===
        public string ClaimIdDisplay => $"#CLM-{ClaimId:D4}";

        public string ClaimDateDisplay => ClaimDate.ToString("dd-MMM-yyyy");

        public string ClaimAmountDisplay => $"${ClaimAmount:N2}";

        public string StatusBadgeClass => Status.ToLower() switch
        {
            "approved" => "bg-success-subtle text-success",
            "rejected" => "bg-danger-subtle text-danger",
            _ => "bg-warning-subtle text-warning"
        };

        public string StatusText => Status switch
        {
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            _ => "Pending"
        };
    }
}