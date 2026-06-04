// ViewModels/ApplyPolicyViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace OnlineInsuranceManagementSystem.ViewModels
{
    public class ApplyPolicyViewModel
    {
        [Required]
        public int PolicyId { get; set; }

        public int UserId { get; set; }
    }
}

