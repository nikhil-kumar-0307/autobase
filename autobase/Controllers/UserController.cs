using autobase.Data;
using autobase.Models.Entities;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace autobase.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db)
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

        // ── GET: Add User ──
        [HttpGet]
        public IActionResult AddUser()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            return View();
        }

        // ── POST: Add User ──
        [HttpPost]
        public IActionResult AddUser(AddUserViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                SetUserViewBag();
                return View(model);
            }

            if (_db.Users.Any(u => u.EmployeeNumber == model.EmployeeNumber))
            {
                ModelState.AddModelError("EmployeeNumber", "This employee number is already registered.");
                SetUserViewBag();
                return View(model);
            }

            if (_db.Users.Any(u => u.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already registered.");
                SetUserViewBag();
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                EmployeeNumber = model.EmployeeNumber,
                MobileNumber = model.MobileNumber,
                Password = HashPassword(model.Password),
                Role = model.Role
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            TempData["Success"] = $"User '{model.Name}' created successfully! They can now login.";
            return RedirectToAction("AddUser");
        }

        // ── GET: Edit User List ──
        [HttpGet]
        public IActionResult EditUser()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            var users = _db.Users.OrderBy(u => u.Name).ToList();
            return View(users);
        }

        // ── GET: Edit Single User Form ──
        [HttpGet]
        public IActionResult EditUserForm(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();

            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                EmployeeNumber = user.EmployeeNumber,
                MobileNumber = user.MobileNumber,
                Role = user.Role
            };

            return View(model);
        }

        // ── POST: Save Edited User ──
        [HttpPost]
        public IActionResult EditUserForm(EditUserViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                SetUserViewBag();
                return View(model);
            }

            var user = _db.Users.Find(model.Id);
            if (user == null) return NotFound();

            // Check duplicate employee number — exclude current user
            if (_db.Users.Any(u => u.EmployeeNumber == model.EmployeeNumber && u.Id != model.Id))
            {
                ModelState.AddModelError("EmployeeNumber", "This employee number is already registered.");
                SetUserViewBag();
                return View(model);
            }

            // Check duplicate mobile — exclude current user
            if (_db.Users.Any(u => u.MobileNumber == model.MobileNumber && u.Id != model.Id))
            {
                ModelState.AddModelError("MobileNumber", "This mobile number is already registered.");
                SetUserViewBag();
                return View(model);
            }

            user.Name = model.Name;
            user.EmployeeNumber = model.EmployeeNumber;
            user.MobileNumber = model.MobileNumber;
            user.Role = model.Role;

            // Only update password if a new one was entered
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (model.NewPassword.Length < 4)
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 4 characters.");
                    SetUserViewBag();
                    return View(model);
                }
                user.Password = HashPassword(model.NewPassword);
            }

            _db.SaveChanges();

            TempData["Success"] = $"User '{model.Name}' updated successfully!";
            return RedirectToAction("EditUser");
        }

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32));
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }
    }
}