// Models/ViewModels/VehicleAvailabilityViewModel.cs
using autobase.Models.Entities;

namespace autobase.Models.ViewModels
{
    public class VehicleGroupViewModel
    {
        public string VehicleType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int AvailableCount { get; set; }
        public int InUseCount { get; set; }
        public int MaintenanceCount { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new();
    }

    public class AvailableVehicleViewModel
    {
        public List<VehicleGroupViewModel> VehicleGroups { get; set; } = new();
        public int TotalVehicles { get; set; }
        public int TotalAvailable { get; set; }
        public int TotalInUse { get; set; }
        public int TotalMaintenance { get; set; }
    }
}