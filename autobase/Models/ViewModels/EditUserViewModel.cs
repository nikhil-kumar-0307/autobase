using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee number is required")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must be 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string MobileNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Designation is required")]
        public string Designation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; } = string.Empty;

        // Optional — leave blank to keep existing password
        public string? NewPassword { get; set; }
    }
}