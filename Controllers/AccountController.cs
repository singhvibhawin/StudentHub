using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConnectingDatabase.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
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
                    ViewBag.ErrorMessage = "Email Address already exists!";
                    return RedirectToAction("Login");
                }

                _db.Users.Add(users).ToString();
                _db.SaveChanges();

                ViewBag.SuccessMessage = "Registered Successfullly!";
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

            var existingStudent = _db.Students.FirstOrDefault(e => e.Email == users.Email);

            if (existingUser != null && existingStudent != null)
            {
                var userProfileViewModel = new UserProfileViewModel
                {
                    Email = existingUser.Email,
                    Username = existingUser.Username,
                    Password = existingUser.Password,
                    Name = existingStudent.Name,
                    Contact = existingStudent.Contact,
                    Address = existingStudent.Address,
                    City = existingStudent.City,
                    Pincode = existingStudent.Pincode
                };

                if (existingUser.Username == "admin")
                {
                    Console.WriteLine("Login Successfully as ADMIN!");
                    return View("UserProfile", userProfileViewModel);
                }
                else
                {
                    Console.WriteLine("Login Successfully as USER!");
                    return View("UserProfile", userProfileViewModel);
                }
            }
            Console.WriteLine("Login Failed!");
            return View();
        }

        public IActionResult UserProfile(UserProfileViewModel model)
        {
            return View(model);
        }
    }
}
