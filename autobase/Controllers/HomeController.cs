using autobase.Data;
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

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            ViewBag.EmpNo = HttpContext.Session.GetString("UserEmployeeNumber");
            ViewBag.Mobile = HttpContext.Session.GetString("UserMobile");
            return View();
        }

        // ── Admin Dashboard ──
        public IActionResult AdminDashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Dashboard");

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            ViewBag.EmpNo = HttpContext.Session.GetString("UserEmployeeNumber");
            ViewBag.Mobile = HttpContext.Session.GetString("UserMobile");

            // ── Stats for dashboard cards ──
            ViewBag.TotalVehicles = _db.Vehicles.Count(v => v.IsActive);
            ViewBag.AvailableVehicles = _db.Vehicles.Count(v => v.IsActive && v.Status == "Available");
            ViewBag.AllocatedVehicles = _db.Vehicles.Count(v => v.IsActive && v.Status == "Allocated");
            ViewBag.TotalUsers = _db.Users.Count();

            // ── Recent vehicles (last 5) ──
            ViewBag.RecentVehicles = _db.Vehicles
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.CreatedAt)
                .Take(5)
                .ToList();

            // ── Recent users (last 5) ──
            ViewBag.RecentUsers = _db.Users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToList();

            return View();
        }
    }
}