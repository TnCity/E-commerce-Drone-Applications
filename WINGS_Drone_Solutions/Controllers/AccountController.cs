using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        //==============================
        // Register
        //==============================

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool exists = await _userService.EmailExistsAsync(model.Email);

            if (exists)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            User user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                Phone = model.Phone,
                Address = model.Address,
                Role = "Customer"
            };

            await _userService.RegisterAsync(user);

            return RedirectToAction("Login");
        }

        //==============================
        // Login
        //==============================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.LoginAsync(model.Email, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid Email or Password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Index", "Home");
        }

        //==============================
        // Logout
        //==============================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}