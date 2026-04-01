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
        [HttpGet]
        public IActionResult VehicleReport(string period = "week")
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();

            var now = DateTime.Now;
            DateTime from = period == "month"
                ? new DateTime(now.Year, now.Month, 1)
                : now.StartOfWeek(DayOfWeek.Monday); // extension below

            var requests = _db.VehicleRequests
                .Where(r => r.CreatedAt >= from && r.CreatedAt <= now
                         && (r.Status == "Approved" || r.Status == "Completed"))
                .ToList();

            // Most used — group by vehicle name, count trips
            var usageGroups = requests
                .GroupBy(r => r.VehicleName)
                .Select(g => new { Name = g.Key, Trips = g.Count() })
                .OrderByDescending(g => g.Trips)
                .Take(6).ToList();

            // Availability — total days in period minus days in use per vehicle
            int totalDays = (now - from).Days + 1;
            var allVehicles = _db.Vehicles.Where(v => v.IsActive).ToList();
            var availGroups = allVehicles
                .Select(v => new {
                    v.VehicleName,
                    DaysInUse = requests.Count(r => r.VehicleName == v.VehicleName),
                    AvailDays = Math.Max(0, totalDays - requests.Count(r => r.VehicleName == v.VehicleName))
                })
                .OrderByDescending(v => v.AvailDays)
                .Take(6).ToList();

            // Trend labels + per-vehicle data
            List<string> trendLabels;
            List<VehicleTrendLine> trendLines = new();
            var top3 = usageGroups.Take(3).Select(g => g.Name).ToList();

            if (period == "week")
            {
                trendLabels = Enumerable.Range(0, 7)
                    .Select(i => from.AddDays(i).ToString("ddd")).ToList();
                foreach (var name in top3)
                {
                    trendLines.Add(new VehicleTrendLine
                    {
                        VehicleName = name,
                        Data = Enumerable.Range(0, 7).Select(i => {
                            var day = from.AddDays(i);
                            return requests.Count(r => r.VehicleName == name
                                && r.CreatedAt.Date == day.Date);
                        }).ToList()
                    });
                }
            }
            else
            {
                trendLabels = Enumerable.Range(0, 4).Select(i => "Wk " + (i + 1)).ToList();
                foreach (var name in top3)
                {
                    trendLines.Add(new VehicleTrendLine
                    {
                        VehicleName = name,
                        Data = Enumerable.Range(0, 4).Select(i => {
                            var wkStart = from.AddDays(i * 7);
                            var wkEnd = wkStart.AddDays(7);
                            return requests.Count(r => r.VehicleName == name
                                && r.CreatedAt >= wkStart && r.CreatedAt < wkEnd);
                        }).ToList()
                    });
                }
            }

            var vehicles = _db.Vehicles.Where(v => v.IsActive).ToList();
            var model = new VehicleReportViewModel
            {
                Period = period,
                TotalRequests = requests.Count,
                MostUsedVehicle = usageGroups.FirstOrDefault()?.Name ?? "—",
                MostUsedTrips = usageGroups.FirstOrDefault()?.Trips ?? 0,
                MostAvailableVehicle = availGroups.FirstOrDefault()?.VehicleName ?? "—",
                MostAvailableDays = availGroups.FirstOrDefault()?.AvailDays ?? 0,
                UtilisationPercent = vehicles.Count == 0 ? 0
                    : (int)Math.Round(requests.Select(r => r.VehicleName).Distinct().Count() * 100.0 / vehicles.Count),
                VehicleNames = usageGroups.Select(g => g.Name).ToList(),
                UsedTrips = usageGroups.Select(g => g.Trips).ToList(),
                AvailableDays = availGroups.Select(g => g.AvailDays).ToList(),
                TrendLabels = trendLabels,
                TrendLines = trendLines,
                StatusAvailable = vehicles.Count(v => v.Status == "Available"),
                StatusInUse = vehicles.Count(v => v.Status == "In Use"),
                StatusMaintenance = vehicles.Count(v => v.Status == "Maintenance")
            };

            return View(model);
        }
    }
}