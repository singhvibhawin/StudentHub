using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConnectingDatabase.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        const string SessionUserName = "_UserName";
        const string SessionUserId = "_UserId";
        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User users)
        {
            if (ModelState.IsValid)
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

                TempData["success"] = "Registered Successfullly!"; 
                return RedirectToAction("Login");
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
