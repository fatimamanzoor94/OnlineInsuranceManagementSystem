using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only alphabets allowed")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [RegularExpression(@"^03[0-9]{9}$",
            ErrorMessage = "Phone format should be 03XXXXXXXXX")]
        public string Phone { get; set; }

        [Required]
        public string Role { get; set; }

        public string? Address { get; set; }

        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool AcceptTerms { get; set; } = false;

        public string? AccountStatus { get; set; } = "Pending";

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // ✅ ADD THESE NEW PROPERTIES (From Database Migration)
        public string? CNIC { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Username { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}
