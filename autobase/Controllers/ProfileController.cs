using autobase.Data;
using autobase.Models.ViewModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace autobase.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        private void SetViewBag()
        {
            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            ViewBag.EmpNo = HttpContext.Session.GetString("UserEmployeeNumber");
            ViewBag.Mobile = HttpContext.Session.GetString("UserMobile");
        }

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));

        // ── GET: /Profile ──
        [HttpGet]
        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            SetViewBag();

            var userId = HttpContext.Session.GetString("UserId");
            var user = _db.Users.Find(int.Parse(userId!));
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new ProfileViewModel
            {
                Name = user.Name,
                EmployeeNumber = user.EmployeeNumber,
                MobileNumber = user.MobileNumber,
                Role = user.Role
            };

            return View(model);
        }

        // ── POST: Change Password ──
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            SetViewBag();

            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Please fix the errors below.";
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var user = _db.Users.Find(int.Parse(userId!));
            if (user == null) return RedirectToAction("Login", "Account");

            // Verify current password
            if (!VerifyPassword(model.CurrentPassword, user.Password))
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToAction("Index");
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["PasswordError"] = "New passwords do not match.";
                return RedirectToAction("Index");
            }

            user.Password = HashPassword(model.NewPassword);
            _db.SaveChanges();

            TempData["PasswordSuccess"] = "Password changed successfully!";
            return RedirectToAction("Index");
        }

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password, salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000, numBytesRequested: 32));
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;
            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password, salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000, numBytesRequested: 32));
            return parts[1] == hashed;
        }
    }
}