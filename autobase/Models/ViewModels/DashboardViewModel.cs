using autobase.Models.Entities;

namespace autobase.Models.ViewModels
{
    public class UserDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<Vehicle> AvailableVehicles { get; set; } = new();
        public List<VehicleRequest> MyRequests { get; set; } = new();
        public List<InUseVehicleInfo> InUseVehicles { get; set; } = new();
    }

    public class VehicleRequestViewModel
    {
        public int VehicleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; } = string.Empty;
    }

    public class InUseVehicleInfo
    {
        public string VehicleName { get; set; } = "";
        public string RegistrationNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string UserName { get; set; } = "";
        public string EmployeeNumber { get; set; } = "";
        public DateTime EndTime { get; set; }
    }
}