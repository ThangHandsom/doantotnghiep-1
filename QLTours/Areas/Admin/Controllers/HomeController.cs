using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private QuanLyTourContext _ctx;

        public HomeController(QuanLyTourContext ctx)
        {
            _ctx = ctx;
        }

        // Index
        public IActionResult Index()
        {
            return View();
        }

        // Login GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login POST
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _ctx.Manages
                                  .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) 
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View();
            }

            if (user.Status == "Locked") 
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View();
            }

            var claims = new List<Claim>
    {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role ? "Admin" : "Employee")  // Admin hoặc Employee
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);


            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("IdMng", user.IdMng);
            HttpContext.Session.SetString("Role", user.Role ? "Admin" : "Employee");


            if (user.Role)
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Employee"); 
            }
        }


        // Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home", new { area = "Admin" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var IdMng = HttpContext.Session.GetInt32("IdMng");
            if (!IdMng.HasValue)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập trước." });
            }

            var user = await _ctx.Manages.FirstOrDefaultAsync(u => u.IdMng == IdMng.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                return Json(new { success = false, message = "Mật khẩu cũ không đúng." });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu mới và xác nhận không khớp." });
            }


            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _ctx.Update(user);
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }



    }
}
