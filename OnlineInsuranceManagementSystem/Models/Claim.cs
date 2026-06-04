using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        public decimal ClaimAmount { get; set; }

        public string Reason { get; set; }

        public DateTime ClaimDate { get; set; }

        public string Status { get; set; }

        public User User { get; set; }

        public Policy Policy { get; set; }
    }
}