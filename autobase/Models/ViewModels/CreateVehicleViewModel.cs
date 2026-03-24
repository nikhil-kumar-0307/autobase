using System.ComponentModel.DataAnnotations;

namespace autobase.Models.ViewModels
{
    public class CreateVehicleViewModel
    {
        [Required(ErrorMessage = "Vehicle name is required")]
        [StringLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters")]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type is required")]
        [StringLength(50, ErrorMessage = "Vehicle type cannot exceed 50 characters")]
        public string VehicleType { get; set; } = string.Empty;
    }
}