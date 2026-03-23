using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Employee Number is required")]
        [Display(Name = "Employee Number")]
        public required string EmployeeNumber { get; set; }

        [Required(ErrorMessage = "Password Is Required")]
        [DataType(DataType.Password)]
        public required string Password {  get; set; }
    }
}
