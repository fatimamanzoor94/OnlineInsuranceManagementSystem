using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class InsuranceType
    {
        [Key]
        public int InsuranceTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }  

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

