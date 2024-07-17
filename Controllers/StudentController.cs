﻿using Microsoft.EntityFrameworkCore;
using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

            //var studentsObj = await _db.Students.OrderBy(s => s.StudentId)
            //                        .Skip((page - 1) * pageSize)
            //                        .Take(pageSize)
            //                        .Select(s => new
            //                        {
            //                            StudentId = s.StudentId,
            //                            FilePaths = s.FilePaths ?? string.Empty, // Handle null values
            //                            // Include other properties with null handling
            //                        })
            //                        .ToListAsync();
            
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
        public async Task<IActionResult> Index(Student student)
        {
            //var docType = Request.Form["docType"];
            //var file = Request.Form.Files["file"];

            //if (file != null && file.Length > 0)
            //{
            //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                
            //    if (!Directory.Exists(filePath))
            //    {
            //        Directory.CreateDirectory(filePath);
            //    }

            //    var fileName = $"{userId}_{Path.GetFileName(file.FileName)}";
            //    var uploadPath = Path.Combine(filePath, fileName);

            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        await file.CopyToAsync(stream);
            //    }
                 

                // Save the file path to the database
                // Example: SaveFilePathToDatabase(filePath);

            //    return Json(new { success = true, filePath });
            //}

            if (student != null)
            {
                var existingStudent = _db.Students.FirstOrDefault(e => e.Email == student.Email);

                if (existingStudent != null)
                {
                    TempData["error"] = "Email already exists!";
                    return View();
                }

                string username = HttpContext.Session.GetString("_UserName");
                int? userId = HttpContext.Session.GetInt32("_UserId");

                if (userId == null)
                {
                    TempData["error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var filePaths = new List<string>();

                if (student.Files != null && student.Files.Count > 0)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var files in student.Files)
                    {
                        if (files.Length > 0)
                        {
                            var fileName = $"{userId}_{Path.GetFileName(files.FileName)}";
                            var filePath = Path.Combine(uploadPath, fileName);
                            filePaths.Add(filePath);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await files.CopyToAsync(stream);
                            }
                        }
                    }
                }

                // Set the UserId property and the FilePaths property of the student
                student.UserId = userId.Value;
                student.FilePaths = string.Join(";", filePaths);

                _db.Students.Add(student);
                _db.SaveChanges();

                TempData["success"] = "Student registered successfully!";
                return RedirectToAction("UserProfile");
            }

            var viewModel = new StudentModel
            {
                students = _db.Students.ToList(),
                student = new Student()
            };
                return View("Index", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument()
        {
            int? userId = HttpContext.Session.GetInt32("_UserId");
            var docType = Request.Form["docType"];
            var file = Request.Form.Files["file"];


            if (file != null && file.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/StudentsDocuments");

                var fileName = $"{userId}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Check if a file with the same name already exists
                if (System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, message = "A file with the same name already exists." });
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Json(new { success = true, filePath });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult DeleteDocument([FromBody] DeleteDocumentRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("_UserId");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/StudentsDocuments");

            var destructureFilePath = request.FilePath.Split('_');
            var fileName = $"{userId}_{destructureFilePath[1]}";
            var filePath = Path.Combine(uploadPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Json(new { success = true });
            }
            return Json(new { success = false });
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

        public IActionResult Documents(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.StudentId == id);
            var uploadFolder = "~/uploads/";

            var filePaths = student.FilePaths.Split(';').Select(file =>
                {
                    var fileName = uploadFolder + Path.GetFileName(file);
                    var relativePath = file.Replace(@"C:\StudentHub\wwwroot\", "");
                    return new { FileName = fileName, RelativePath = relativePath };
                }).ToArray();

            return View(filePaths);
        }

        [HttpPost]
        public IActionResult Edit([FromBody]  Student updatedStudent)
        {
            if (updatedStudent != null)
            {
                //    var existingStudent = _db.Students.Find(id => id.UserId == updatedStudent.UserId);
                var existingStudent = _db.Students.FirstOrDefault(uid => uid.UserId == updatedStudent.UserId);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                // Update other properties as needed
                existingStudent.Name = updatedStudent.Name;
                existingStudent.Email = updatedStudent.Email;
                existingStudent.Contact = updatedStudent.Contact;
                existingStudent.Address = updatedStudent.Address;
                existingStudent.City = updatedStudent.City;
                existingStudent.Pincode = updatedStudent.Pincode;

                if (updatedStudent.FilePaths != null)
                {
                    existingStudent.FilePaths = updatedStudent.FilePaths;
                }

                _db.Students.Update(existingStudent);
                _db.SaveChanges();
                TempData["success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
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
