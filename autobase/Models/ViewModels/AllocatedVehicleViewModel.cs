using autobase.Models.Entities;

namespace autobase.Models.ViewModels
{
    public class AllocatedVehicleViewModel
    {
        public List<VehicleRequest> AllocatedRequests { get; set; } = new();
        public int TotalAllocated { get; set; }
        public int TotalInUse { get; set; }
        public int TotalOverdue { get; set; }
    }
}