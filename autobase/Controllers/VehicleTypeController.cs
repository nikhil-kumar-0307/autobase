using autobase.Data;
using autobase.Models.Entities;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace autobase.Controllers
{
    public class VehicleTypeController : Controller
    {
        private readonly AppDbContext _db;

        public VehicleTypeController(AppDbContext db)
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

        // ── GET: Create Vehicle Type ──
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            SetUserViewBag();

            ViewBag.Vehicles = _db.VehicleTypes
                                  .Where(v => v.IsActive)
                                  .OrderByDescending(v => v.CreatedAt)
                                  .ToList();

            return View();
        }

        // ── POST: Create Vehicle Type ──
        [HttpPost]
        public IActionResult Create(CreateVehicleViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                SetUserViewBag();
                ViewBag.Vehicles = _db.VehicleTypes
                                      .Where(v => v.IsActive)
                                      .OrderByDescending(v => v.CreatedAt)
                                      .ToList();
                return View(model);
            }

            if (_db.VehicleTypes.Any(v => v.VehicleName == model.VehicleName
                                      && v.Type == model.VehicleType
                                      && v.IsActive))
            {
                ModelState.AddModelError("VehicleName", "Vehicle already exists.");
                SetUserViewBag();
                ViewBag.Vehicles = _db.VehicleTypes
                                      .Where(v => v.IsActive)
                                      .OrderByDescending(v => v.CreatedAt)
                                      .ToList();
                return View(model);
            }

            var vehicleType = new VehicleType
            {
                VehicleName = model.VehicleName,
                Type = model.VehicleType,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.VehicleTypes.Add(vehicleType);
            _db.SaveChanges();

            TempData["Success"] = "Vehicle Type added successfully!";
            return RedirectToAction("Create");
        }
    }
}