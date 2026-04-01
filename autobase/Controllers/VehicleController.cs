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
            var role = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))
                   && (role == "Admin" || role == "SuperAdmin");
        }

        // ── GET: Add Vehicle ──
        [HttpGet]
        public IActionResult AddVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            ViewBag.VehicleTypeList = _db.VehicleTypes
                                         .Where(v => v.IsActive)
                                         .OrderBy(v => v.VehicleName)
                                         .ToList();
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
                ViewBag.VehicleTypeList = _db.VehicleTypes
                                             .Where(v => v.IsActive)
                                             .OrderBy(v => v.VehicleName)
                                             .ToList();
                return View(model);
            }

            if (_db.Vehicles.Any(v => v.RegistrationNumber == model.RegistrationNumber && v.IsActive))
            {
                ModelState.AddModelError("RegistrationNumber", "This registration number already exists.");
                SetUserViewBag();
                ViewBag.VehicleTypeList = _db.VehicleTypes
                                             .Where(v => v.IsActive)
                                             .OrderBy(v => v.VehicleName)
                                             .ToList();
                return View(model);
            }

            var vehicle = new Vehicle
            {
                VehicleName = model.VehicleName,
                RegistrationNumber = model.RegistrationNumber,
                VehicleType = model.VehicleType,
                Year = model.Year,
                Status = model.Status,
                Notes = model.Notes,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _db.Vehicles.Add(vehicle);
            _db.SaveChanges();

            TempData["Success"] = "Vehicle added successfully!";
            return RedirectToAction("AddVehicle", "Vehicle");
        }

        // ── GET: Edit Vehicle List ──
        [HttpGet]
        public IActionResult EditVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            // Only show active vehicles
            var vehicles = _db.Vehicles
                              .Where(v => v.IsActive)
                              .OrderByDescending(v => v.CreatedAt)
                              .ToList();
            return View(vehicles);
        }

        // ── GET: Edit Single Vehicle Form ──
        [HttpGet]
        public IActionResult EditVehicleForm(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();

            var vehicle = _db.Vehicles.Find(id);
            if (vehicle == null || !vehicle.IsActive) return NotFound();

            var model = new EditVehicleViewModel
            {
                Id = vehicle.Id,
                VehicleName = vehicle.VehicleName,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleType = vehicle.VehicleType,
                Year = vehicle.Year,
                Status = vehicle.Status,
                Notes = vehicle.Notes
            };

            return View(model);
        }

        // ── POST: Save Edited Vehicle ──
        [HttpPost]
        public IActionResult EditVehicleForm(EditVehicleViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                SetUserViewBag();
                return View(model);
            }

            var vehicle = _db.Vehicles.Find(model.Id);
            if (vehicle == null || !vehicle.IsActive) return NotFound();

            if (_db.Vehicles.Any(v => v.RegistrationNumber == model.RegistrationNumber
                                   && v.Id != model.Id
                                   && v.IsActive))
            {
                ModelState.AddModelError("RegistrationNumber", "This registration number already exists.");
                SetUserViewBag();
                return View(model);
            }

            vehicle.VehicleName = model.VehicleName;
            vehicle.RegistrationNumber = model.RegistrationNumber;
            vehicle.VehicleType = model.VehicleType;
            vehicle.Year = model.Year;
            vehicle.Status = model.Status;
            vehicle.Notes = model.Notes;

            _db.SaveChanges();

            TempData["Success"] = "Vehicle updated successfully!";
            return RedirectToAction("EditVehicle", "Vehicle");
        }

        // ── GET: Delete Vehicle List ──
        [HttpGet]
        public IActionResult DeleteVehicle()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            // Show only active vehicles (can be disabled)
            var vehicles = _db.Vehicles
                              .Where(v => v.IsActive)
                              .OrderByDescending(v => v.CreatedAt)
                              .ToList();
            return View(vehicles);
        }

        // ── POST: Soft Delete (Disable) Vehicle ──
        [HttpPost]
        public IActionResult ConfirmDelete(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            var vehicle = _db.Vehicles.Find(id);

            if (vehicle == null)
            {
                TempData["Error"] = "Vehicle not found.";
                return RedirectToAction("DeleteVehicle");
            }

            // Soft delete — just mark as inactive, do NOT remove from DB
            vehicle.IsActive = false;
            _db.SaveChanges();

            TempData["Success"] = $"{vehicle.VehicleName} ({vehicle.RegistrationNumber}) has been disabled successfully.";
            return RedirectToAction("DeleteVehicle");
        }
    }
}