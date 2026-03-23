using Microsoft.AspNetCore.Mvc;

namespace autobase.Controllers
{
    public class UserController : Controller
    {
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

        public IActionResult AddUser()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            return View();
        }

        public IActionResult EditUser()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Account");
            SetUserViewBag();
            return View();
        }
    }
}