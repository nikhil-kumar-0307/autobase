using autobase.Models.Entities;

namespace autobase.Models.ViewModels
{
    public class SeeRequestViewModel
    {
        public List<VehicleRequest> AllRequests { get; set; } = new();
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CompletedCount { get; set; }

    }
    public class RequestActionViewModel 
    {
        public int Id { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}