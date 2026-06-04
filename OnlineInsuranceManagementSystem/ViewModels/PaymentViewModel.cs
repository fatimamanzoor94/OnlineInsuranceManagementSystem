using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class PaymentViewModel
    {
        public int PaymentId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Paid"; // Paid, Pending, Failed

        // === Display Helpers ===
        public string PaymentIdDisplay => $"#PAY-{PaymentId:D4}";

        public string PaymentDateDisplay => PaymentDate.ToString("dd-MMM-yyyy");

        public string AmountDisplay => $"${Amount:N2}";

        public string StatusBadgeClass => Status.ToLower() switch
        {
            "paid" => "bg-success-subtle text-success",
            "failed" => "bg-danger-subtle text-danger",
            _ => "bg-warning-subtle text-warning"
        };

        public string StatusText => Status switch
        {
            "Paid" => "Paid",
            "Failed" => "Failed",
            _ => "Pending"
        };
    }
}