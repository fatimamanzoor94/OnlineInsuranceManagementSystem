using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    [Table("Notifications")]
    public class Notification  // ✅ Singular name (best practice)
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "System Notification";

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = "General";

        public bool IsRead { get; set; } = false;

        // ✅ MAP C# PROPERTY "CreatedDate" TO DATABASE COLUMN "Date"
        public DateTime CreatedDate { get; set; }

        public int? RelatedRecordId { get; set; }
        public string? RelatedModule { get; set; }

        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}