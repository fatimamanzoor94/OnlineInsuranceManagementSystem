using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [MinLength(3)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only alphabets allowed")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number & special character")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [RegularExpression(@"^03[0-9]{9}$", ErrorMessage = "Phone format should be 03XXXXXXXXX")]
        public string Phone { get; set; }

        //[Required]
        //public string Address { get; set; }

        //[Required]
        //public string Role { get; set; }

        [Required(ErrorMessage = "You must accept the Terms & Conditions")]
        public bool AcceptTerms { get; set; }
    }
}


