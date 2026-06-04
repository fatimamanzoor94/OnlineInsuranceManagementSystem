using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class ReportViewModel
    {
        // ==================== SUMMARY METRICS ====================
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalClaimAmount { get; set; }
        public decimal ApprovedClaimsAmount { get; set; }
        public decimal PendingClaimsAmount { get; set; }

        // ==================== MONTHLY REVENUE CHART ====================
        public List<MonthlyRevenueItem> MonthlyRevenueData { get; set; } = new();

        public class MonthlyRevenueItem
        {
            public string Month { get; set; }      // "Jan 2024"
            public decimal Revenue { get; set; }    // Total payment amount
            public int TransactionCount { get; set; }
        }

        // ==================== CLAIMS ANALYSIS CHART ====================
        public List<ClaimsAnalysisItem> ClaimsAnalysisData { get; set; } = new();

        public class ClaimsAnalysisItem
        {
            public string Category { get; set; }    // "Life", "Medical", "Motor", "Home"
            public int TotalClaims { get; set; }
            public int ApprovedClaims { get; set; }
            public int PendingClaims { get; set; }
            public int RejectedClaims { get; set; }
            public decimal TotalAmount { get; set; }
        }

        // ==================== POLICY DISTRIBUTION CHART ====================
        public List<PolicyDistributionItem> PolicyDistributionData { get; set; } = new();

        public class PolicyDistributionItem
        {
            public string PolicyType { get; set; }  // Insurance type name
            public int PolicyCount { get; set; }
            public decimal TotalPremium { get; set; }
            public decimal TotalCoverage { get; set; }
            public string Color { get; set; }        // Assigned from palette
        }

        // ==================== RECENT REPORTS TABLE ====================
        public List<RecentReportItem> RecentReports { get; set; } = new();

        public class RecentReportItem
        {
            public int ReportId { get; set; }
            public string ReportType { get; set; }      // "Revenue", "Claims", "Policy"
            public string Period { get; set; }          // "Jan 2024", "Q1 2024"
            public DateTime GeneratedDate { get; set; }
            public string GeneratedBy { get; set; }
            public string Status { get; set; }          // "Completed", "Processing"
            public string FileUrl { get; set; }
        }

        // ==================== FILTER OPTIONS ====================
        public List<string> AvailableYears { get; set; } = new();
        public List<string> AvailablePolicyTypes { get; set; } = new();
        public string SelectedYear { get; set; }
        public string SelectedPolicyType { get; set; }
    }
}