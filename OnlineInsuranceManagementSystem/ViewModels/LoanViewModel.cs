using OnlineInsuranceManagementSystem.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class LoanViewModel
    {
        public int LoanId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public DateTime ApplyDate { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // === Display Helpers ===
        public string LoanIdDisplay => $"#LN-{LoanId:D4}";

        public string ApplyDateDisplay => ApplyDate.ToString("dd-MMM-yyyy");

        public string AmountDisplay => $"${Amount:N2}";

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


        public string? Reason { get; set; }
        public Policy Policy { get; set; }

        
    }
}