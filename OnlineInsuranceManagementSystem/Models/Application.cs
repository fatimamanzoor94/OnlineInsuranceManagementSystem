// Models/Application.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public int PolicyId { get; set; }

        [ForeignKey("PolicyId")]
        public virtual Policy? Policy { get; set; }

        public int? AgentId { get; set; }

        [ForeignKey("AgentId")]
        public virtual User? Agent { get; set; }

        // ✅ NEW: Track when agent was assigned by Admin
        public DateTime? AssignedDate { get; set; }

        public DateTime ApplyDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string? Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Remarks { get; set; }

        public virtual ICollection<PolicyDocument> PolicyDocuments { get; set; } = new List<PolicyDocument>();
    }
}