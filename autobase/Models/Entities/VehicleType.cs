namespace autobase.Models.Entities
{
    public class VehicleType
    {
        public int Id { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}