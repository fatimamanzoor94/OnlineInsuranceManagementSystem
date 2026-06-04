// Models/Payment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public string Status { get; set; }

        public User User { get; set; }

        public Policy Policy { get; set; }

        // ✅ ADD THIS PROPERTY
        [NotMapped]
        public string PaymentIdDisplay => $"#PAY-{PaymentId:D4}";
    }
}
