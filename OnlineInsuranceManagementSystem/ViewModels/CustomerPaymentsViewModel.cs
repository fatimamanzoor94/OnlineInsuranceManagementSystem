// ViewModels/CustomerPaymentsViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    // Main Index ViewModel for Customer Payments Page
    public class CustomerPaymentsIndexViewModel
    {
        public List<CustomerPaymentItem> Payments { get; set; } = new();
        public List<CustomerPolicyItem> ActivePolicies { get; set; } = new();

        // Summary Stats for Top Cards
        public int TotalPayments { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public int PaidPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
    }

    // Individual Payment Item for Table/Modal Display
    public class CustomerPaymentItem
    {
        public int PaymentId { get; set; }
        public int PolicyId { get; set; }

        // Policy Details
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;

        // Payment Details
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = "Paid";

        // UI Helper Properties (Formatted for Display)
        public string PaymentIdDisplay => $"#PAY-{PaymentId:D4}";  // ✅ ADD THIS
        public string AmountDisplay => Amount.ToString("N2");
        public string PaymentDateDisplay => PaymentDate.ToString("dd-MMM-yyyy");
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

    // Create Payment ViewModel
    public class CreatePaymentViewModel
    {
        [Required(ErrorMessage = "Please select a policy")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select a payment method")]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Paid"; // Auto-set to Paid for customer payments
    }

    // Payment Details ViewModel
    public class PaymentDetailsViewModel
    {
        public int PaymentId { get; set; }
        public string PaymentIdDisplay => $"#PAY-{PaymentId:D4}";
        public string PolicyName { get; set; } = string.Empty;
        public string InsuranceTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AmountFormatted => Amount.ToString("N2");
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentDateFormatted => PaymentDate.ToString("dd-MMM-yyyy");
        public string Status { get; set; } = "Paid";
        public string StatusBadgeClass => Status.ToLower() switch
        {
            "paid" => "bg-success-subtle text-success",
            "failed" => "bg-danger-subtle text-danger",
            _ => "bg-warning-subtle text-warning"
        };
    }
}