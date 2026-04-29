// Controllers/AccountController.cs
using autobase.Data;
using autobase.Models.Entities;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace autobase.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // ─── REGISTER GET ────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        // ─── REGISTER POST ───────────────────────────────────────
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check if EmployeeNumber already exists
            if (_db.Users.Any(u => u.EmployeeNumber == model.EmployeeNumber))
            {
                ModelState.AddModelError("EmployeeNumber", "Employee Number already registered.");
                return View(model);
            }

            // Check if MobileNumber already exists
            if (_db.Users.Any(u => u.MobileNumber == model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", "Mobile Number already registered.");
                return View(model);
            }

            // Save user with hashed password
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

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        // ─── LOGIN GET ───────────────────────────────────────────
        [HttpGet]
        public IActionResult Login() => View();

        // ─── LOGIN POST ──────────────────────────────────────────
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Find user by EmployeeNumber
            var user = _db.Users.FirstOrDefault(u => u.EmployeeNumber == model.EmployeeNumber);

            if (user == null || !VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid Employee Number or Password.");
                return View(model);
            }

            // Store user details in Session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserEmployeeNumber", user.EmployeeNumber);
            HttpContext.Session.SetString("UserMobile", user.MobileNumber);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Role-based redirect
            return user.Role switch
            {
                "SuperAdmin" => RedirectToAction("AdminDashboard", "Home"),
                "Admin" => RedirectToAction("AdminDashboard", "Home"),
                _ => RedirectToAction("Dashboard", "Home")
            };
        }

        // ─── LOGOUT ─────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Login");
        }

        // ─── HASH HELPERS ────────────────────────────────────────
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

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;
            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32));
            return parts[1] == hashed;
        }
    }
}