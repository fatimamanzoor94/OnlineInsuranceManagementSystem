// ViewModels/UserProfileViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only alphabets and spaces allowed")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty; // Read-only

        [Phone(ErrorMessage = "Invalid phone format")]
        [RegularExpression(@"^03[0-9]{9}$", ErrorMessage = "Phone must be 03XXXXXXXXX")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "CNIC must be 13 digits")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be 13 digits without dashes")]
        public string? CNIC { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; } // "Male", "Female", "Other"

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; } = "Pakistan";

        public string Role { get; set; } = string.Empty; // Admin / Agent / Customer

        public string? ProfileImageUrl { get; set; }

        public DateTime? LastLoginDate { get; set; }

        // ✅ Computed properties for display
        public string RoleBadgeClass => Role.ToLower() switch
        {
            "admin" => "bg-primary",
            "agent" => "bg-info",
            "customer" => "bg-success",
            _ => "bg-secondary"
        };

        // ✅ FIXED: Changed return type from string to int
        public int ProfileCompletionPercentage
        {
            get
            {
                int totalFields = 10;
                int completedFields = 0;

                // Required fields (always count if filled)
                if (!string.IsNullOrEmpty(FullName)) completedFields++;
                if (!string.IsNullOrEmpty(Email)) completedFields++;
                if (!string.IsNullOrEmpty(Phone)) completedFields++;

                // Optional fields (count if filled)
                if (!string.IsNullOrEmpty(Address)) completedFields++;
                if (!string.IsNullOrEmpty(CNIC)) completedFields++;
                if (!string.IsNullOrEmpty(Gender)) completedFields++;
                if (DateOfBirth.HasValue) completedFields++;
                if (!string.IsNullOrEmpty(City)) completedFields++;
                if (!string.IsNullOrEmpty(Country)) completedFields++;

                // Profile image (bonus field)
                if (!string.IsNullOrEmpty(ProfileImageUrl)) completedFields++;

                return Math.Min(100, (completedFields * 100) / totalFields);
            }
        }

        // ✅ Enhanced: Safe image URL handling
        public string ProfileImageUrlDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(ProfileImageUrl))
                    return "~/aassets/img/avatars/default-avatar.png";

                // Ensure path starts with / for absolute URL
                return ProfileImageUrl.StartsWith("/")
                    ? ProfileImageUrl
                    : $"/{ProfileImageUrl}";
            }
        }

        // ✅ Added: CreatedAt for "Member Since" display
        public DateTime? CreatedAt { get; set; }
    }
}
