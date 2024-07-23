using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using ConnectingDatabase.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConnectingDatabase.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;

        const string SessionUserName = "_UserName";
        const string SessionUserId = "_UserId";

        public AccountController(ApplicationDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
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
                    ViewBag.ErrorMessage = "Password does not matches, Try Again!";
                }
                else
                {
                    var existingUser = _db.Users.FirstOrDefault(e => e.Email == users.Email);

                    if (existingUser != null)
                    {
                        // Email already exists
                        TempData["error"] = "Email Address already exists!";
                        return RedirectToAction("Login");
                    }

                    _db.Users.Add(users); // No need for .ToString()
                    _db.SaveChanges();
                    TempData["success"] = "Registered Successfully!";

                    // Send confirmation email
                    var subject = "Registration Successful";
                    var message = $"Hello {users.Name},<br><br>Thank you for registering with us. Your account has been created successfully.<br><br>Best regards,<br>Student Hub";
                    await _emailService.SendEmailAsync(users.Email, subject, message);

                    return RedirectToAction("Login");
                }
            }

            ViewBag.ErrorMessage = "Something went wrong, Try Again!";
            return View();
            }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(User users)
        {
            var existingUser = _db.Users.FirstOrDefault(e => e.Email == users.Email);

            if (existingUser != null)
            {
                if (existingUser.Username == "admin")
                {
                    TempData["success"] = "Login Successfully - ADMIN!";
                    Console.WriteLine("Login Successfully as ADMIN!");

                    //Storing Data into Session using SetString and SetInt32 method
                    HttpContext.Session.SetString("_UserName", existingUser.Username);
                    HttpContext.Session.SetInt32("_UserId", existingUser.UserId); // Assuming UserId is an int

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["success"] = "Login Successfully - USER!";
                    Console.WriteLine("Login Successfully as USER!");

                    HttpContext.Session.SetString("_UserName", existingUser.Username);
                    HttpContext.Session.SetInt32("_UserId", existingUser.UserId); // Assuming UserId is an int

                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["error"] = "Invalid username or password";
            Console.WriteLine("Login Failed!");
            return View();
        }
       
        public ActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
