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
                    ViewBag.ErrorMessage = "Passwords do not match. Please try again.";
                }
                else
                {
                    var existingUser = _db.Users.FirstOrDefault(e => e.Email == users.Email);

                    if (existingUser != null)
                    {
                        await _customLogger.LogAsync($"Registration failed: Email address already exists for email {users.Email}");
                        TempData["error"] = "Email address already exists!";
                        return RedirectToAction("Login");
                    }

                    try
                    {
                        _db.Users.Add(users);
                        await _db.SaveChangesAsync();
                        await _customLogger.LogAsync($"User registered successfully: {users.Name} with email {users.Email}");
                        TempData["success"] = "Registered successfully!";

                        var subject = "Registration Successful";
                        var message = $"Hello {users.Name},<br><br>Thank you for registering with us. Your account has been created successfully.<br><br>Best regards,<br>Student Hub";
                        await _emailService.SendEmailAsync(users.Email, subject, message);

                        return RedirectToAction("Login");
                    }
                    catch (Exception ex)
                    {
                        await _customLogger.LogAsync($"Registration failed: {ex.Message}");
                        TempData["error"] = "An error occurred while registering. Please try again.";
                    }
                }
            }

            await _customLogger.LogAsync($"Registration failed: Try Again! {users.Email}");
            ViewBag.ErrorMessage = "Registration failed: Try Again!";
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

            if (existingUser != null && existingUser.Password == users.Password)
            {
                try
                {
                    if (existingUser.Username == "admin")
                    {
                        await _customLogger.LogAsync($"Admin login successful for email {users.Email}");
                        TempData["success"] = "Login successful - ADMIN!";
                    }
                    else
                    {
                        await _customLogger.LogAsync($"User login successful for email {users.Email}");
                        TempData["success"] = "Login successful - USER!";
                    }

                    HttpContext.Session.SetString(SessionUserName, existingUser.Username);
                    HttpContext.Session.SetInt32(SessionUserId, existingUser.UserId);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    await _customLogger.LogAsync($"Login failed: An error occurred during login. Please try again.");
                    TempData["error"] = "An error occurred during login. Please try again.";
                }
            }
            else
            {
                await _customLogger.LogAsync($"Login failed: Invalid username or password for email {users.Email}");
                TempData["error"] = "Invalid username or password.";
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _customLogger.LogAsync($"User logged out at {DateTime.UtcNow}");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
