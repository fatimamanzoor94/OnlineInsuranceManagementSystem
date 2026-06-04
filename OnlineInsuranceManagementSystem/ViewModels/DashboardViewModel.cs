using OnlineInsuranceManagementSystem.Models;

namespace OnlineInsuranceManagementSystem.ViewModels
{

    public class DashboardViewModel
    {

        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }

        // ==================== SUMMARY CARDS ====================
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAgents { get; set; }
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public decimal TotalPayments { get; set; }
        public int TotalMessages { get; set; }
        public int UnreadMessages { get; set; }

        // ==================== CHARTS DATA ====================
        public List<ChartSeriesData> MonthlyClaimsData { get; set; } = new();
        public List<ChartSeriesData> MonthlyPaymentsData { get; set; } = new();

        // ==================== RECENT ACTIVITY ====================
        public List<RecentClaimItem> RecentClaims { get; set; } = new();
        public List<RecentMessageItem> RecentMessages { get; set; } = new();
        public List<RecentPaymentItem> RecentPayments { get; set; } = new();
    }

    public class ChartSeriesData
    {
        public string Month { get; set; } = string.Empty;
        public int ClaimsCount { get; set; }
        public decimal PaymentsAmount { get; set; }

        public int ApplicationsCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = "#0d6efd";
    }

    public class RecentClaimItem
    {
        public int ClaimId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
    }

    public class RecentMessageItem
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class RecentPaymentItem
    {
        public int PaymentId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    public class RecentApplicationItem
    {
        public int ApplicationId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PolicyName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime ApplyDate { get; set; }
    }
}