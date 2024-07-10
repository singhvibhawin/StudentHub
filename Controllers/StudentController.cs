using Microsoft.EntityFrameworkCore;
using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ConnectingDatabase.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _db;
        const string SessionUserName = "_UserName";
        const string SessionUserId = "_UserId";
        public StudentController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            string username = HttpContext.Session.GetString("_UserName");

            if (username == "admin")
            {
                List<Student> studentsObj = _db.Students.ToList();
                ViewBag.Username = username;
                return View(studentsObj);
            }

            return NotFound();
        }

        // Pagination
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            string username = HttpContext.Session.GetString("_UserName");

            if (username == null)
            {
                TempData["error"] = "Access denied. Please log in to continue.";
                Console.WriteLine("Access denied. Please log in to continue.");
                return RedirectToAction("Login","Account");
            }

            if (username == "user")
            {
                int? userId = HttpContext.Session.GetInt32("_UserId");
                var existingStudent = _db.Students.FirstOrDefault(uid => uid.UserId == userId);

                if (existingStudent != null)
                {
                    TempData["error"] = "A student profile associated with this user already exists.";
                    Console.WriteLine("A student profile associated with this user already exists.");
                    return RedirectToAction("UserProfile");
                }
            }
            ViewBag.Username = username;

            if (page < 0 || page == 0)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalRecord = await _db.Students.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecord / (double)pageSize);
            var studentsObj = await _db.Students
                                .OrderBy(s => s.StudentId) // Ensure ordering for consistent paging
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var viewModel = new StudentModel
            {
                students = studentsObj,
                student = new Student()
            };

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            return View(viewModel);
        }

        public List<Student> GetMatchCore(int page, int pageSize, out int totalRecord, out int totalPage)
        {
            var query = new List<Student>();
            totalRecord = _db.Students.Count();
            totalPage = (totalRecord / pageSize) + ((totalRecord % pageSize) > 0 ? 1 : 0);
            query = _db.Students.OrderBy(a => a.StudentId).Skip(((page - 1) * pageSize)).Take(pageSize).ToList();
            return query;
        }

        [HttpPost]
        public IActionResult Index(Student student)
        {
            if (ModelState.IsValid)
            {
                var existingStudent = _db.Students.FirstOrDefault(e => e.Email == student.Email);

                if (existingStudent != null)
                {
                    // Email already exists
                    TempData["error"] = "Email already exists!";
                    return View();
                }

                string username = HttpContext.Session.GetString("_UserName");
                int? userId = HttpContext.Session.GetInt32("_UserId");

                if (userId == null)
                {
                    // Handle the case where the userId is not found in the session
                    TempData["error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                // Set the UserId property of the student
                student.UserId = userId.Value;

                _db.Students.Add(student).ToString();
                _db.SaveChanges();

                TempData["success"] = "Student registered successfully!";
                return RedirectToAction("UserProfile");
            }

            //If Model State is not valid 
            var viewModel = new StudentModel
            {
                students = _db.Students.ToList(),
                student = new Student()
            };

            return View("Index");
        }

        public IActionResult UserProfile()
        {
            string username = HttpContext.Session.GetString("_UserName");
            ViewBag.Username = username;

            int? userId = HttpContext.Session.GetInt32("_UserId");
    
            if (userId == null)
            {
                TempData["error"] = "User session has expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var studentObj = _db.Students.FirstOrDefault(e => e.UserId == userId.Value);
            ViewBag.Data = studentObj;

            // If no matching student is found, handle accordingly
            if (studentObj == null)
            {
                TempData["error"] = "No student record found for the current user.";
                return RedirectToAction("Index", "Student");
            }
        
            return View();
        }

        [HttpPost]
        public IActionResult Edit([FromBody]  Student updatedStudent)         
        {
            if (updatedStudent != null)
            {
                _db.Students.Update(updatedStudent);
                _db.SaveChanges();
                TempData["success"] = "Student updated successfully!";
            }
            else
            {
                return NotFound();
            }
            return RedirectToAction("Index");
        }

        [HttpDelete]
        public IActionResult DeleteStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }
            _db.Students.Remove(student);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DownloadExcel()
        {
            List<Student> students = _db.Students.ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo("Stduents.xlsx")))
            {
                var worksheet = package.Workbook.Worksheets.Add("Students");

                // Add headers
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Contact";
                worksheet.Cells[1, 4].Value = "Address";
                worksheet.Cells[1, 5].Value = "City";
                worksheet.Cells[1, 6].Value = "Pincode";

                // Add data
                int row = 2;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = student.Name;
                    worksheet.Cells[row, 2].Value = student.Email;
                    worksheet.Cells[row, 3].Value = student.Contact;
                    worksheet.Cells[row, 4].Value = student.Address;
                    worksheet.Cells[row, 5].Value = student.City;
                    worksheet.Cells[row, 6].Value = student.Pincode;
                    row++;
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                var content = stream.ToArray();
                TempData["success"] = "Download successfull!";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students.xlsx");
            }
        }

        public IActionResult DownloadPdf()
        {
            List<Student> students = _db.Students.ToList();
            using (var stream = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, stream).CloseStream = false;
                document.Open();

                var table = new PdfPTable(6) { WidthPercentage = 100 };
                table.AddCell("Name");
                table.AddCell("Email");
                table.AddCell("Contact");
                table.AddCell("Address");
                table.AddCell("City");
                table.AddCell("Pincode");

                foreach (var student in students)
                {
                    table.AddCell(student.Name);
                    table.AddCell(student.Email);
                    table.AddCell(student.Contact);
                    table.AddCell(student.Address);
                    table.AddCell(student.City);
                    table.AddCell(student.Pincode);
                }

                document.Add(table);
                document.Close();

                var content = stream.ToArray();
                TempData["success"] = "Download successfull!";
                return File(content, "application/pdf", "Students.pdf");
            }
        }
    }
}
