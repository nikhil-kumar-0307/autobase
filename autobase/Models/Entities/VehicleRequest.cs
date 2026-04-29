namespace autobase.Models.Entities
{
    public class VehicleRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;   // ← new
        public string Department { get; set; } = string.Empty;    // ← new
        public int VehicleId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected / Completed
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}