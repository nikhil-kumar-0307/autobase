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
            var role = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"))
                   && (role == "Admin" || role == "SuperAdmin");
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
            // ── Block Admin from assigning Admin role — only SuperAdmin can ──
            if (model.Role == "Admin" && HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                ModelState.AddModelError("Role", "Only SuperAdmin can assign the Admin role.");
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
                Designation = model.Designation,   // ← new
                Department = model.Department,    // ← new
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

            var currentRole = HttpContext.Session.GetString("UserRole");
            var currentEmpNo = HttpContext.Session.GetString("UserEmployeeNumber");

            IQueryable<User> query = _db.Users;

            if (currentRole == "Admin")
            {
                // Admin sees only Employees — not Admins, not SuperAdmins
                query = query.Where(u => u.Role == "Employee" || u.Role == "Driver");
            }
            else if (currentRole == "SuperAdmin")
            {
                // SuperAdmin sees everyone except SuperAdmin accounts
                query = query.Where(u => u.Role != "SuperAdmin");
            }

            var users = query.OrderBy(u => u.Name).ToList();
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
            // Block editing SuperAdmin account entirely
            if (user.Role == "SuperAdmin")
            {
                TempData["Error"] = "SuperAdmin account cannot be edited.";
                return RedirectToAction("EditUser");
            }

            // Block Admin from editing another Admin's account
            if (user.Role == "Admin" && HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                TempData["Error"] = "Only SuperAdmin can edit an Admin account.";
                return RedirectToAction("EditUser");
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                EmployeeNumber = user.EmployeeNumber,
                MobileNumber = user.MobileNumber,
                Designation = user.Designation,    // ← new
                Department = user.Department, // ← new
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

            // Block Admin from editing another Admin's account
            if (user.Role == "Admin" && HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                TempData["Error"] = "Only SuperAdmin can edit an Admin account.";
                return RedirectToAction("EditUser");
            }

            // Block Admin from assigning Admin role to anyone
            if (model.Role == "Admin" && HttpContext.Session.GetString("UserRole") != "SuperAdmin")
            {
                ModelState.AddModelError("Role", "Only SuperAdmin can assign the Admin role.");
                SetUserViewBag();
                return View(model);
            }

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
            user.Designation = model.Designation;   // ← new
            user.Department = model.Department;    // ← new
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