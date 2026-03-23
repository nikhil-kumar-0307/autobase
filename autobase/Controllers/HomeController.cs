using Microsoft.AspNetCore.Mvc;

namespace autobase.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // ?? Regular User Dashboard 
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

        // ?? Admin Dashboard 
        public IActionResult AdminDashboard()
        {
            // If not logged in ? back to Login
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
                return RedirectToAction("Login", "Account");

            // If logged in but NOT Admin ? redirect to regular Dashboard
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Dashboard");

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.Role = HttpContext.Session.GetString("UserRole");
            ViewBag.EmpNo = HttpContext.Session.GetString("UserEmployeeNumber");
            ViewBag.Mobile = HttpContext.Session.GetString("UserMobile");

            return View();
        }
    }
}