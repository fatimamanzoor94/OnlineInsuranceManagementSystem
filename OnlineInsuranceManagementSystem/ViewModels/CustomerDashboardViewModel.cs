using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class CustomerDashboardViewModel
    {
        // ==================== SUMMARY CARD METRICS ====================
        public int ActivePoliciesCount { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int PendingApplications { get; set; }
        public int RejectedApplications { get; set; }

        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }

        public int TotalPayments { get; set; }
        public decimal TotalPaidAmount { get; set; }

        public int LoanRequests { get; set; }
        public int PendingLoans { get; set; }
        public int ApprovedLoans { get; set; }

        public int NotificationsCount { get; set; }
        public int UnreadNotificationsCount { get; set; }

        public int MessagesCount { get; set; }
        public int UnreadMessagesCount { get; set; }

        // ==================== CHART DATA ====================
        public List<CustomerChartSeriesData> ApplicationsByStatusData { get; set; } = new();
        public List<CustomerChartSeriesData> ClaimsByStatusData { get; set; } = new();
        public List<CustomerChartSeriesData> MonthlyPaymentsData { get; set; } = new();
        public List<CustomerChartSeriesData> PolicyDistributionData { get; set; } = new();
        public List<CustomerChartSeriesData> LoanStatusData { get; set; } = new();

        // ==================== RECENT ACTIVITY ITEMS ====================
        public List<CustomerRecentApplicationItem> RecentApplications { get; set; } = new();
        public List<CustomerRecentClaimItem> RecentClaims { get; set; } = new();
        public List<CustomerRecentPaymentItem> RecentPayments { get; set; } = new();
        public List<CustomerRecentNotificationItem> RecentNotifications { get; set; } = new();
        public List<CustomerRecentMessageItem> RecentMessages { get; set; } = new();
        public List<CustomerRecentPolicyItem> RecentPolicies { get; set; } = new();
    }

    // ==================== SUPPORTING MODELS (UNIQUE NAMES) ====================

    public class CustomerChartSeriesData
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public string Color { get; set; } = "#0d6efd";
    }

    public class CustomerRecentApplicationItem
    {
        public int ApplicationId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceType { get; set; } = string.Empty;
        public DateTime ApplyDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PremiumAmount { get; set; }
    }

    public class CustomerRecentClaimItem
    {
        public int ClaimId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CustomerRecentPaymentItem
    {
        public int PaymentId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }

    public class CustomerRecentNotificationItem
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class CustomerRecentMessageItem
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public string Preview { get; set; } = string.Empty;
    }

    public class CustomerRecentPolicyItem
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceType { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}