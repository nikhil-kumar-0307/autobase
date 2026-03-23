namespace autobase.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string EmployeeNumber { get; set; }
        public required string MobileNumber { get; set; }
        public required string Password { get; set; }  
        public required string Role { get; set; }
    }
}