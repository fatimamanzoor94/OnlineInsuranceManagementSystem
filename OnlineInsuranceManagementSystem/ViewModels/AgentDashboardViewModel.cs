// ViewModels/AgentDashboardViewModel.cs
using System;
using System.Collections.Generic;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class AgentDashboardViewModel
    {
        // ✅ Application Statistics
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }

        // ✅ Claim Statistics
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int RecommendedClaims { get; set; }

        // ✅ Chart Data - Monthly Applications
        public List<ChartSeriesData> MonthlyApplicationsData { get; set; } = new();

        // ✅ Chart Data - Applications by Status (Pie)
        public List<ChartSeriesData> ApplicationsByStatus { get; set; } = new();

        // ✅ Recent Activity Lists
        public List<RecentApplicationItem> RecentApplications { get; set; } = new();
        public List<RecentClaimItem> RecentClaims { get; set; } = new();
    }
}