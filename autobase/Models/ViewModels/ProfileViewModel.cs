// Models/ViewModels/ProfileViewModel.cs
namespace autobase.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;   // ← new
        public string Department { get; set; } = string.Empty;    // ← new
        public string Role { get; set; } = string.Empty;

    }
}