using autobase.Data;
using autobase.Models.Entities;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace autobase.Controllers
{
    public class AutobaseController : Controller
    {
        private readonly AppDbContext _db;

        public AutobaseController(AppDbContext db)
        {
            _db = db;
        }

        private void SetUserViewBag()
        {
            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            ViewBag.EmpNo = HttpContext.Session.GetString("UserEmployeeNumber");
            ViewBag.Mobile = HttpContext.Session.GetString("UserMobile");
        }

        private bool IsAdminLoggedIn()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))
                   && (role == "Admin" || role == "SuperAdmin");
        }

        // ── GET: /Autobase/AllocatedVehicle ──
        [HttpGet]
        public IActionResult AllocatedVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();

            var now = DateTime.Now;

            var allocated = _db.VehicleRequests
                               .Where(r => r.Status == "Approved")
                               .OrderBy(r => r.EndTime)
                               .ToList();

            var model = new AllocatedVehicleViewModel
            {
                AllocatedRequests = allocated,
                TotalAllocated = allocated.Count,
                TotalInUse = allocated.Count(r => r.EndTime >= now),
                TotalOverdue = allocated.Count(r => r.EndTime < now)
            };

            return View(model);
        }

        // ── GET: /Autobase/SeeRequest ──
        [HttpGet]
        public IActionResult SeeRequest()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();

            var all = _db.VehicleRequests
                         .OrderByDescending(r => r.CreatedAt)
                         .ToList();

            var model = new SeeRequestViewModel
            {
                AllRequests = all,
                TotalCount = all.Count,
                PendingCount = all.Count(r => r.Status == "Pending"),
                ApprovedCount = all.Count(r => r.Status == "Approved"),
                RejectedCount = all.Count(r => r.Status == "Rejected"),
                CompletedCount = all.Count(r => r.Status == "Completed")
            };

            return View(model);
        }

        // ── POST: Approve Request ──
        [HttpPost]
        public IActionResult ApproveRequest(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("SeeRequest");
            }

            // Mark request approved
            request.Status = "Approved";

            // Mark vehicle as In Use
            var vehicle = _db.Vehicles.Find(request.VehicleId);
            if (vehicle != null) vehicle.Status = "In Use";

            _db.SaveChanges();
            TempData["Success"] = $"Request by {request.UserName} for {request.VehicleName} has been approved.";
            return RedirectToAction("SeeRequest");
        }

        // ── POST: Reject Request ──
        [HttpPost]
        public IActionResult RejectRequest(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("SeeRequest");
            }

            request.Status = "Rejected";
            _db.SaveChanges();

            TempData["Error"] = $"Request by {request.UserName} for {request.VehicleName} has been rejected.";
            return RedirectToAction("SeeRequest");
        }

        // ── POST: Mark Completed ──
        [HttpPost]
        public IActionResult CompleteRequest(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            var request = _db.VehicleRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("SeeRequest");
            }

            request.Status = "Completed";

            // Free the vehicle back to Available
            var vehicle = _db.Vehicles.Find(request.VehicleId);
            if (vehicle != null) vehicle.Status = "Available";

            _db.SaveChanges();
            TempData["Success"] = $"{request.VehicleName} marked as returned and available again.";
            return RedirectToAction("SeeRequest");
        }

        // ── GET: /Autobase/AvailableVehicle ──
        [HttpGet]
        public IActionResult AvailableVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();

            var vehicles = _db.Vehicles
                              .Where(v => v.IsActive)
                              .OrderBy(v => v.VehicleType)
                              .ThenBy(v => v.VehicleName)
                              .ToList();

            var groups = vehicles
                .GroupBy(v => v.VehicleType)
                .Select(g => new VehicleGroupViewModel
                {
                    VehicleType = g.Key ?? "Unknown",
                    TotalCount = g.Count(),
                    AvailableCount = g.Count(v => v.Status == "Available"),
                    InUseCount = g.Count(v => v.Status == "In Use"),
                    MaintenanceCount = g.Count(v => v.Status == "Maintenance"),
                    Vehicles = g.OrderBy(v => v.VehicleName).ToList()
                })
                .OrderBy(g => g.VehicleType)
                .ToList();

            var model = new AvailableVehicleViewModel
            {
                VehicleGroups = groups,
                TotalVehicles = vehicles.Count,
                TotalAvailable = vehicles.Count(v => v.Status == "Available"),
                TotalInUse = vehicles.Count(v => v.Status == "In Use"),
                TotalMaintenance = vehicles.Count(v => v.Status == "Maintenance")
            };

            return View(model);
        }
    }
}