// Models/ViewModels/ChangePasswordViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}