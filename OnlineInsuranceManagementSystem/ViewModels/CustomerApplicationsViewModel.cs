// ViewModels/CustomerApplicationsViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    // Main View Model for Customer Applications Index Page
    public class CustomerApplicationsIndexViewModel
    {
        public List<CustomerApplicationItem> Applications { get; set; } = new();
        public List<InsuranceTypeSummary> InsuranceTypes { get; set; } = new();

        // Summary Stats for Top Cards
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
    }

    // Individual Application Item for Table/Modal Display
    public class CustomerApplicationItem
    {
        public int ApplicationId { get; set; }
        public int PolicyId { get; set; }

        // Policy Details
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public decimal PremiumAmount { get; set; }
        public decimal CoverageAmount { get; set; }
        public int Duration { get; set; }

        // Application Details
        public DateTime ApplyDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Remarks { get; set; }
        public string? AgentName { get; set; }

        // Documents (if any)
        public List<ApplicationDocument> Documents { get; set; } = new();

        // UI Helper Properties (Formatted for Display)
        public string ApplicationIdDisplay => $"#APP-{ApplicationId:D4}";
        public string ApplyDateDisplay => ApplyDate.ToString("dd-MMM-yyyy");
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
        public string PremiumFormatted => PremiumAmount.ToString("N2");
        public string CoverageFormatted => CoverageAmount.ToString("N2");
        public string DurationFormatted => $"{Duration} Months";
    }

    // Insurance Type Summary for Filter Dropdown
    public class InsuranceTypeSummary
    {
        public int InsuranceTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Document Item for Policy Documents Modal
    public class ApplicationDocument
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public string FileSizeDisplay => GetFileSizeDisplay();

        private string GetFileSizeDisplay()
        {
            // Placeholder - implement actual file size logic if storing in DB
            return "N/A";
        }
    }

    // Policy Details Modal ViewModel
    public class PolicyDetailsViewModel
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public decimal PremiumAmount { get; set; }
        public decimal CoverageAmount { get; set; }
        public int Duration { get; set; }
        public string? Terms { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string PremiumFormatted => PremiumAmount.ToString("N2");
        public string CoverageFormatted => CoverageAmount.ToString("N2");
        public string DurationFormatted => $"{Duration} Months";
        public string CreatedAtFormatted => CreatedAt.ToString("dd-MMM-yyyy");
    }

    // ✅ ADD THIS CLASS - For Application Timeline
    public class TimelineItem
    {
        public string Date { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // ✅ Response wrapper for timeline API
    public class ApplicationTimelineResponse
    {
        public int ApplicationId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public List<TimelineItem> Timeline { get; set; } = new();
    }
}