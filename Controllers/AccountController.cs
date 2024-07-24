using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using ConnectingDatabase.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectingDatabase.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly CustomLoggerService _customLogger;

        const string SessionUserName = "_UserName";
        const string SessionUserId = "_UserId";

        public AccountController(ApplicationDbContext db, IEmailService emailService, CustomLoggerService customLogger)
        {
            _db = db;
            _emailService = emailService;
            _customLogger = customLogger;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User users)
        {
            if (ModelState.IsValid)
            {
                if (users.Password != users.ConfirmPassword)
                {
                    await _customLogger.LogAsync($"Registration failed: Passwords do not match for email {users.Email}");
                    ViewBag.ErrorMessage = "Password does not match, Try Again!";
                }
                else
                {
                    var existingUser = _db.Users.FirstOrDefault(e => e.Email == users.Email);

                    if (existingUser != null)
                    {
                        await _customLogger.LogAsync($"Registration failed: Email address already exists for email {users.Email}");
                        TempData["error"] = "Email Address already exists!";
                        return RedirectToAction("Login");
                    }

                    _db.Users.Add(users);
                    _db.SaveChanges();
                    await _customLogger.LogAsync($"User registered successfully: {users.Name} with email {users.Email}");
                    TempData["success"] = "Registered Successfully!";

                    var subject = "Registration Successful";
                    var message = $"Hello {users.Name},<br><br>Thank you for registering with us. Your account has been created successfully.<br><br>Best regards,<br>Student Hub";
                    await _emailService.SendEmailAsync(users.Email, subject, message);

                    return RedirectToAction("Login");
                }
            }

            await _customLogger.LogAsync($"Registration failed: ModelState is invalid for email {users.Email}");
            ViewBag.ErrorMessage = "Something went wrong, Try Again!";
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User users)
        {
            var existingUser = _db.Users.FirstOrDefault(e => e.Email == users.Email);

            if (existingUser != null)
            {
                if (existingUser.Username == "admin")
                {
                    await _customLogger.LogAsync($"Admin login successful for email {users.Email}");
                    TempData["success"] = "Login Successfully - ADMIN!";
                    HttpContext.Session.SetString("_UserName", existingUser.Username);
                    HttpContext.Session.SetInt32("_UserId", existingUser.UserId);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    await _customLogger.LogAsync($"User login successful for email {users.Email}");
                    TempData["success"] = "Login Successfully - USER!";
                    HttpContext.Session.SetString("_UserName", existingUser.Username);
                    HttpContext.Session.SetInt32("_UserId", existingUser.UserId);

                    return RedirectToAction("Index", "Home");
                }
            }

            await _customLogger.LogAsync($"Login failed: Invalid username or password for email {users.Email}");
            TempData["error"] = "Invalid username or password";
            return View();
        }

        public IActionResult Logout()
        {
            _customLogger.LogAsync($"User logged out at {DateTime.UtcNow}");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
