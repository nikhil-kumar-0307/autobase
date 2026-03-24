namespace autobase.Models.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Status { get; set; } = "Available";
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;   // false = disabled (soft delete)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}