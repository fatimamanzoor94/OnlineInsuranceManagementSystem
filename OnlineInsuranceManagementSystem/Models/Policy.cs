using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class Policy
    {
        [Key]
        public int PolicyId { get; set; }

        [Required, StringLength(100)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        public int InsuranceTypeId { get; set; }

        [ForeignKey("InsuranceTypeId")]
        public virtual InsuranceType? InsuranceType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PremiumAmount { get; set; }

        [Required]
        public int Duration { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CoverageAmount { get; set; }

        [StringLength(2000)]
        public string? Terms { get; set; }

        public bool Status { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}