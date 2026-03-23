using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class EditVehicleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vehicle name is required")]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration number is required")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required")]
        public string VehicleType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(1990, 2030, ErrorMessage = "Enter a valid year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 999, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Available";

        public string Notes { get; set; } = string.Empty;
    }
}