using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        public decimal Amount { get; set; }

        public DateTime ApplyDate { get; set; }

        public string Status { get; set; }

        public User User { get; set; }

        public Policy Policy { get; set; }
    }
}