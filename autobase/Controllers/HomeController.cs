using autobase.Data;
using autobase.Models.Entities;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace autobase.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        public HomeController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ── Regular User Dashboard ──
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Login", "Account");

            var model = new UserDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "",
                EmployeeNumber = HttpContext.Session.GetString("UserEmployeeNumber") ?? "",
                Mobile = HttpContext.Session.GetString("UserMobile") ?? "",
                Role = HttpContext.Session.GetString("UserRole") ?? "",

                AvailableVehicles = _db.Vehicles
                    .Where(v => v.IsActive && v.Status == "Available")
                    .OrderBy(v => v.VehicleType)
                    .ThenBy(v => v.VehicleName)
                    .ToList(),

                MyRequests = _db.VehicleRequests
                    .Where(r => r.EmployeeNumber == HttpContext.Session.GetString("UserEmployeeNumber"))
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToList(),


                InUseVehicles = _db.VehicleRequests
    .Where(r => r.Status == "Approved")
    .OrderBy(r => r.EndTime)
    .Select(r => new InUseVehicleInfo
    {
        VehicleId = r.VehicleId,
        VehicleName = r.VehicleName,
        RegistrationNumber = r.RegistrationNumber,
        VehicleType = r.VehicleType,
        UserName = r.UserName,
        EmployeeNumber = r.EmployeeNumber,
        EndTime = r.EndTime
    })
    .ToList()
            };

            return View(model);
        }
        // ── POST: Submit Vehicle Request ──
        [HttpPost]
        public IActionResult RequestVehicle(VehicleRequestViewModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid request. Please check the form.";
                return RedirectToAction("Dashboard");
            }

            var vehicle = _db.Vehicles.Find(model.VehicleId);

            // ✅ CHANGED: Allow "In Use" vehicles too (for future bookings)
            if (vehicle == null || !vehicle.IsActive)
            {
                TempData["Error"] = "This vehicle is no longer available.";
                return RedirectToAction("Dashboard");
            }

            if (model.EndTime <= model.StartTime)
            {
                TempData["Error"] = "End time must be after start time.";
                return RedirectToAction("Dashboard");
            }

            // ✅ NEW: Block only if there's an actual time overlap with an approved booking
            bool hasConflict = _db.VehicleRequests.Any(r =>
                r.VehicleId == model.VehicleId &&
                r.Status == "Approved" &&
                r.StartTime < model.EndTime &&
                r.EndTime > model.StartTime);

            if (hasConflict)
            {
                TempData["Error"] = "This vehicle already has an approved booking that overlaps your requested time. Please choose a different time.";
                return RedirectToAction("Dashboard");
            }

            var request = new VehicleRequest
            {
                UserId = HttpContext.Session.GetString("UserId") ?? "",
                UserName = HttpContext.Session.GetString("UserName") ?? "",
                EmployeeNumber = HttpContext.Session.GetString("UserEmployeeNumber") ?? "",
                UserMobile = HttpContext.Session.GetString("UserMobile") ?? "",
                VehicleId = vehicle.Id,
                VehicleName = vehicle.VehicleName,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleType = vehicle.VehicleType ?? "",
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Purpose = model.Purpose,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.VehicleRequests.Add(request);
            _db.SaveChanges();

            TempData["Success"] = $"Request submitted for {vehicle.VehicleName}. Awaiting admin approval.";
            return RedirectToAction("Dashboard");
        }

        // ── POST: User returns vehicle ──
        [HttpPost]
        public IActionResult ReturnVehicle(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Login", "Account");

            var empNo = HttpContext.Session.GetString("UserEmployeeNumber");

            var request = _db.VehicleRequests.Find(id);

            // Safety: only the owner can return their own request
            if (request == null || request.EmployeeNumber != empNo)
            {
                TempData["Error"] = "Request not found or access denied.";
                return RedirectToAction("Dashboard");
            }

            if (request.Status != "Approved")
            {
                TempData["Error"] = "Only approved requests can be returned.";
                return RedirectToAction("Dashboard");
            }

            // Mark request completed
            request.Status = "Completed";

            // Free the vehicle back to Available
            var vehicle = _db.Vehicles.Find(request.VehicleId);
            if (vehicle != null) vehicle.Status = "Available";

            _db.SaveChanges();

            TempData["Success"] = $"{request.VehicleName} has been returned successfully. Thank you!";
            return RedirectToAction("Dashboard");
        }

        // ── Admin Dashboard ──
        public IActionResult AdminDashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Login", "Account");
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "SuperAdmin")
                return RedirectToAction("Dashboard");

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            ViewBag.EmpNo = HttpContext.Session.GetString("UserEmployeeNumber");
            ViewBag.Mobile = HttpContext.Session.GetString("UserMobile");

            ViewBag.TotalVehicles = _db.Vehicles.Count(v => v.IsActive);
            ViewBag.AvailableVehicles = _db.Vehicles.Count(v => v.IsActive && v.Status == "Available");
            ViewBag.AllocatedVehicles = _db.Vehicles.Count(v => v.IsActive && v.Status == "Allocated");
            ViewBag.TotalUsers = _db.Users.Count();

            ViewBag.RecentVehicles = _db.Vehicles
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.RecentUsers = _db.Users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToList();

            return View();
        }
    }
}