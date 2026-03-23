using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class AddVehicleViewModel
    {
        [Required(ErrorMessage = "Vehicle name is required")]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Registration number is required")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required")]
        public string VehicleType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        public string FuelType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Available";

        public string ChassisNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}