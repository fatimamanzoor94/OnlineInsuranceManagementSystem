using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class MotorInsurancePolicyViewModel
    {
        public int PolicyId { get; set; }

        [Required, StringLength(100)]
        public string PolicyName { get; set; } = string.Empty;

        public string InsuranceTypeName { get; set; } = "Motor Insurance";

        [DataType(DataType.Currency)]
        public decimal PremiumAmount { get; set; }

        public int Duration { get; set; }

        [DataType(DataType.Currency)]
        public decimal CoverageAmount { get; set; }

        public string? Terms { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // === Display Helpers (View mein direct use honge) ===
        public string PolicyIdDisplay => $"#POL-{PolicyId:D4}";

        public string PremiumDisplay => $"${PremiumAmount:N2}/month";

        public string CoverageDisplay => $"${CoverageAmount:N0}";

        public string DurationDisplay => Duration >= 12
            ? $"{Duration / 12} Year{(Duration / 12 > 1 ? "s" : "")}"
            : $"{Duration} Months";

        public string StatusBadgeClass => IsActive
            ? "bg-success-subtle text-success"
            : "bg-secondary-subtle text-secondary";

        public string StatusText => IsActive ? "Active" : "Inactive";

        public string PremiumPlanBadge => PremiumAmount >= 500
            ? "bg-primary-subtle text-primary"
            : "bg-light text-muted";

        public string PremiumPlanText => PremiumAmount >= 500 ? "Premium Plan" : "Standard";
    }
}