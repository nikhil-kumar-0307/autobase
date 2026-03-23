using autobase.Data;
using autobase.Models.Entities;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace autobase.Controllers
{
    public class VehicleController : Controller
    {
        private readonly AppDbContext _db;

        public VehicleController(AppDbContext db)
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
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))
                   && HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // ── GET: Add Vehicle ──
        [HttpGet]
        public IActionResult AddVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            return View();
        }

        // ── POST: Add Vehicle ──
        [HttpPost]
        public IActionResult AddVehicle(AddVehicleViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                SetUserViewBag();
                return View(model);
            }

            // Check duplicate registration number
            if (_db.Vehicles.Any(v => v.RegistrationNumber == model.RegistrationNumber))
            {
                ModelState.AddModelError("RegistrationNumber", "This registration number already exists.");
                SetUserViewBag();
                return View(model);
            }

            var vehicle = new Vehicle
            {
                VehicleName = model.VehicleName,
                RegistrationNumber = model.RegistrationNumber,
                VehicleType = model.VehicleType,
                Year = model.Year,
                FuelType = model.FuelType,
                Status = model.Status,
                ChassisNumber = model.ChassisNumber,
                EngineNumber = model.EngineNumber,
                Notes = model.Notes,
                CreatedAt = DateTime.Now
            };

            _db.Vehicles.Add(vehicle);
            _db.SaveChanges();

            TempData["Success"] = "Vehicle added successfully!";
            return RedirectToAction("AddVehicle", "Vehicle");
        }

        // ── GET: Edit Vehicle ──
        [HttpGet]
        public IActionResult EditVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            return View();
        }

        // ── GET: Delete Vehicle ──
        [HttpGet]
        public IActionResult DeleteVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            return View();
        }
    }
}