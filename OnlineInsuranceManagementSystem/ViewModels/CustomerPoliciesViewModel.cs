// ViewModels/CustomerPoliciesViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class CustomerPoliciesViewModel
    {
        // Policy Display Properties
        public int PolicyId { get; set; }

        [Display(Name = "Policy Name")]
        public string PolicyName { get; set; } = string.Empty;

        public int InsuranceTypeId { get; set; }
        public string InsuranceTypeName { get; set; } = string.Empty;

        [Display(Name = "Premium Amount")]
        public decimal PremiumAmount { get; set; }

        public int Duration { get; set; }

        [Display(Name = "Coverage Amount")]
        public decimal CoverageAmount { get; set; }

        public string? Terms { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Application Tracking
        public bool HasApplied { get; set; }
        public string? ApplicationStatus { get; set; }
        public int? ApplicationId { get; set; }
        public DateTime? AppliedDate { get; set; }

        // UI Helper Properties
        public string PolicyIdFormatted => $"#POL-{PolicyId}";
        public string PremiumFormatted => PremiumAmount.ToString("N2");
        public string CoverageFormatted => CoverageAmount.ToString("N2");
        public string DurationFormatted => $"{Duration} Months";
        public string CreatedAtFormatted => CreatedAt.ToString("dd-MMM-yyyy");
    }

    public class CustomerPoliciesIndexViewModel
    {
        public System.Collections.Generic.List<CustomerPoliciesViewModel> Policies { get; set; } = new();
        public System.Collections.Generic.List<InsuranceTypeViewModel> InsuranceTypes { get; set; } = new();

        public int TotalAvailablePolicies { get; set; }
        public int TotalAppliedPolicies { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
    }

    public class InsuranceTypeViewModel
    {
        public int InsuranceTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

