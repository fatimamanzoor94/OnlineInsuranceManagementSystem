// ViewModels/CustomerClaimsViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    // Main Index ViewModel for Customer Claims Page
    public class CustomerClaimsIndexViewModel
    {
        public List<CustomerClaimItem> Claims { get; set; } = new();
        public List<CustomerPolicyItem> ApprovedPolicies { get; set; } = new();

        // Summary Stats for Top Cards
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
    }

    // Individual Claim Item for Table/Modal Display
    public class CustomerClaimItem
    {
        public int ClaimId { get; set; }
        public int PolicyId { get; set; }

        // Policy Details
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;

        // Claim Details
        public decimal ClaimAmount { get; set; }
        public string? Reason { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Status { get; set; } = "Pending";

        // UI Helper Properties (Formatted for Display)
        public string ClaimIdDisplay => $"#CLM-{ClaimId:D4}";
        public string ClaimAmountDisplay => ClaimAmount.ToString("N2");
        public string ClaimDateDisplay => ClaimDate.ToString("dd-MMM-yyyy");
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

    // Policy Item for Dropdown (Only Approved Policies)
    public class CustomerPolicyItem
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }

        // ✅ ADD THESE MISSING PROPERTIES
        public decimal PremiumAmount { get; set; }
        public string PremiumFormatted => PremiumAmount.ToString("N2");
        public string CoverageFormatted => CoverageAmount.ToString("N2");

        // Keep these for Claims module
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Create Claim ViewModel
    public class CreateClaimViewModel
    {
        [Required(ErrorMessage = "Please select a policy")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Claim amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Claim amount must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal ClaimAmount { get; set; }

        [Required(ErrorMessage = "Please provide a reason")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Reason { get; set; } = string.Empty;

        public DateTime ClaimDate { get; set; } = DateTime.Now;
    }

    // Claim Details ViewModel
    public class ClaimDetailsViewModel
    {
        public int ClaimId { get; set; }
        public string ClaimIdDisplay => $"#CLM-{ClaimId:D4}";
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string ClaimAmountFormatted => ClaimAmount.ToString("N2");
        public string Reason { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public string ClaimDateFormatted => ClaimDate.ToString("dd-MMM-yyyy");
        public string Status { get; set; } = "Pending";
        public string StatusBadgeClass => Status.ToLower() switch
        {
            "approved" => "bg-success-subtle text-success",
            "rejected" => "bg-danger-subtle text-danger",
            _ => "bg-warning-subtle text-warning"
        };
    }
}