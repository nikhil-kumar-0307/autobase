using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee number is required")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; } = string.Empty;
    }
}