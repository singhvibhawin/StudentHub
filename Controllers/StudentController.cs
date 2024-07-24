using Microsoft.EntityFrameworkCore;
using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using iTextSharp.text.pdf;
using Microsoft.CodeAnalysis;
using ConnectingDatabase.Services;

namespace ConnectingDatabase.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<StudentController> _logger;
        private readonly CustomLoggerService _customLogger;

        const string SessionUserName = "_UserName";
        const string SessionUserId = "_UserId";
        public StudentController(ApplicationDbContext db, IEmailService emailService, ILogger<StudentController> logger, CustomLoggerService customLogger)
        {
            _db = db;
            _emailService = emailService;
            _customLogger = customLogger;
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

            // _customLogger.LogAsync($"Access denied for user: {username}").Wait();
            return NotFound();
        }

        // Pagination
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            string username = HttpContext.Session.GetString("_UserName");

            if (username == null)
            {
                // await _customLogger.LogAsync("Access denied. User is not logged in.");
                TempData["error"] = "Access denied. Please log in to continue.";
                return RedirectToAction("Login","Account");
            }

            if (username == "user")
            {
                int? userId = HttpContext.Session.GetInt32("_UserId");
                var existingStudent = _db.Students.FirstOrDefault(uid => uid.UserId == userId);

                if (existingStudent != null)
                {
                    // await _customLogger.LogAsync("A student profile associated with this user already exists.");
                    TempData["error"] = "A student profile associated with this user already exists.";
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
        public async Task<IActionResult> Index(Student student)
        {
            if (ModelState.IsValid)
            {
                var existingStudent = _db.Students.FirstOrDefault(e => e.Email == student.Email);

                if (existingStudent != null)
                {
                    // await _customLogger.LogAsync($"Email already exists for email: {student.Email}");
                    TempData["error"] = "Email already exists!";
                    return View();
                }

                string username = HttpContext.Session.GetString("_UserName");
                int? userId = HttpContext.Session.GetInt32("_UserId");

                if (userId == null)
                {
                    // await _customLogger.LogAsync("User session has expired. Please log in again.");
                    TempData["error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                // Set the UserId property and the FilePaths property of the student
                student.UserId = userId.Value;

                _db.Students.Add(student);
                _db.SaveChanges();

                // await _customLogger.LogAsync($"Student registered successfully with Email: {student.Email}");
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
        public async Task<IActionResult> UploadDocument(Documents docs)
        {
            var docType = Request.Form["docType"];
            var file = Request.Form.Files["file"];


            if (file != null && file.Length > 0)
            {
                int? userId = HttpContext.Session.GetInt32("_UserId");

                if (userId == null)
                {
                    // await _customLogger.LogAsync("Session expired while uploading document.");
                    TempData["error"] = "Session expired! Try Again!";
                    return Json(new { success = false, message = TempData["error"] });
                }

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/StudentsDocuments");

                var fileName = $"{userId}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Check if a file with the same name already exists
                if (System.IO.File.Exists(filePath))
                {
                    // await _customLogger.LogAsync($"File already exists with name: {fileName}");
                    return Json(new { success = false, message = "A file with the same name already exists." });
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                docs.FilePaths = filePath;
                docs.DocumentName = docType;
                docs.UserId = userId.Value;

                _db.Documents.Add(docs);
                _db.SaveChanges();
                
                // await _customLogger.LogAsync($"Document uploaded successfully with name: {fileName}");
                TempData["success"] = "Document uploaded successfully!";

                return Json(new { success = true, filePath });
            }

            // await _customLogger.LogAsync("No file uploaded during document upload.");
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument([FromBody] DeleteDocumentRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("_UserId");

            if (userId == null)
            {
                // await _customLogger.LogAsync("User is not logged in while trying to delete document.");
                return Json(new { success = false, message = "User is not logged in." });
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/StudentsDocuments");

            var destructureFilePath = request.FilePath.Split('_');

            var fileName = $"{userId}_{destructureFilePath[1]}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Find the document based on both UserId and FilePath
            var docs = _db.Documents.FirstOrDefault(s => s.UserId == userId && s.FilePaths == filePath);

            if (docs == null)
            {
                // await _customLogger.LogAsync("Document not found during delete attempt.");
                return Json(new { success = false, message = "Document not found." });
            }

            _db.Documents.Remove(docs);
            _db.SaveChanges();

            TempData["success"] = "Document deleted successfully!";

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                // await _customLogger.LogAsync($"Document deleted successfully with path: {filePath}");
                return Json(new { success = true });
            }

            // await _customLogger.LogAsync($"Document file not found for deletion with path: {filePath}");
            return Json(new { success = false });
        }

        public IActionResult UserProfile()
        {
            string username = HttpContext.Session.GetString("_UserName");
            ViewBag.Username = username;

            int? userId = HttpContext.Session.GetInt32("_UserId");
    
            if (userId == null)
            {
                // _customLogger.LogAsync("User session has expired while accessing user profile.").Wait();
                TempData["error"] = "User session has expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var studentObj = _db.Students.FirstOrDefault(e => e.UserId == userId.Value);
            ViewBag.Data = studentObj;

            var docsObj = _db.Documents.FirstOrDefault(e => e.UserId == userId.Value);
            ViewBag.Docs = docsObj;

            // If no matching student is found, handle accordingly
            if (studentObj == null)
            {
                // _customLogger.LogAsync("No student record found for the current user in profile access.").Wait();
                TempData["error"] = "No student record found for the current user.";
                return RedirectToAction("Index", "Student");
            }
        
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody]  Student updatedStudent)
        {
            if (updatedStudent != null)
            {
                string username = HttpContext.Session.GetString("_UserName");

                if (username == "admin")
                {
                    var student = _db.Students.FirstOrDefault(uid => uid.StudentId == updatedStudent.StudentId);
                    if (student == null)
                    {
                        // await _customLogger.LogAsync($"No student record found with ID: {updatedStudent.StudentId}");
                        return NotFound();
                    }

                    // Update other properties as needed
                    student.Name = updatedStudent.Name;
                    student.Email = updatedStudent.Email;
                    student.Contact = updatedStudent.Contact;
                    student.Address = updatedStudent.Address;
                    student.City = updatedStudent.City;
                    student.Pincode = updatedStudent.Pincode;

                    _db.Students.Update(student);
                    _db.SaveChanges();

                    // await _customLogger.LogAsync($"Student record updated successfully with ID: {updatedStudent.StudentId}");
                    TempData["success"] = "Student updated successfully!";
                    return RedirectToAction("Index");
                }

                var existingStudent = _db.Students.FirstOrDefault(uid => uid.UserId == updatedStudent.UserId);
                if (existingStudent == null)
                {
                    // await _customLogger.LogAsync($"No student record found with ID: {updatedStudent.StudentId}");
                    return NotFound();
                }

                // Update other properties as needed
                existingStudent.Name = updatedStudent.Name;
                existingStudent.Email = updatedStudent.Email;
                existingStudent.Contact = updatedStudent.Contact;
                existingStudent.Address = updatedStudent.Address;
                existingStudent.City = updatedStudent.City;
                existingStudent.Pincode = updatedStudent.Pincode;

                _db.Students.Update(existingStudent);
                _db.SaveChanges();

                // await _customLogger.LogAsync($"Student record updated successfully with ID: {updatedStudent.StudentId}");
                TempData["success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                // await _customLogger.LogAsync("No student data provided for editing.");
                return NotFound();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = _db.Students.FirstOrDefault(s => s.StudentId == id);
            if (student == null)
            {
                // await _customLogger.LogAsync($"Student with ID {id} not found for deletion.");
                return NotFound();
            }
            _db.Students.Remove(student);
            _db.SaveChanges();

            // await _customLogger.LogAsync($"Student with ID {id} successfully deleted.");
            return RedirectToAction("Index");
        }

        public IActionResult Documents(int id)
        {
            var documents = _db.Documents.Where(s => s.UserId == id).ToList();
            var uploadFolder = "~/StudentsDocuments/";

            var filePaths = documents.SelectMany(doc => doc.FilePaths.Split(';')
                .Select(file =>
                {
                    var fileName = uploadFolder + Path.GetFileName(file);
                    var relativePath = file.Replace(@"C:\StudentHub\wwwroot\", "");
                    return new
                    {
                        FileName = fileName,
                        RelativePath = relativePath,
                        DocumentId = doc.DocumentId,
                        DocumentName = doc.DocumentName,
                        Remarks = doc.Remarks,
                        DocumentStatus = doc.DocumentStatus
                    };
                }
            ))
            .ToArray();

            ViewBag.UserId = id;

            return View(filePaths);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrReject([FromBody] Documents model)
        {
            if (model == null)
            {
                // await _customLogger.LogAsync("Invalid data provided for document approval or rejection.");
                return BadRequest("Invalid data.");
            }

            // Retrieve the user from the database
            var user = _db.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null)
            {
                // await _customLogger.LogAsync($"User with ID {model.UserId} not found for document approval or rejection.");
                return NotFound("User not found.");
            }

            // Process the action (approve or reject) based on the model data
            if (model.DocumentStatus == "Approve")
            {
                // Find all documents where UserId matches the UserId in the model
                var documents = _db.Documents.Where(doc => doc.UserId == model.UserId).ToList();

                if (documents == null || !documents.Any())
                {
                    // await _customLogger.LogAsync($"No documents found for user ID {model.UserId}.");
                    return NotFound("No documents found for the specified UserId.");
                }

                // Update the status and remarks for each document
                foreach (var document in documents)
                {
                    document.DocumentStatus = model.DocumentStatus;
                    document.Remarks = string.IsNullOrWhiteSpace(model.Remarks) ? "Pending" : model.Remarks;
                    // Update any other columns if needed
                }

                // Save changes to the database
                _db.SaveChanges();

                // Send approval email
                var subject = "Documents Approved";
                var message = $"Hello {user.Name},<br><br>Your documents have been approved with the following remarks: {model.Remarks}.<br><br>Best regards,<br>Student Hub";
                await _emailService.SendEmailAsync(user.Email, subject, message);

                // Return a success response
                return RedirectToAction("Index", "Students");
            }
            else if (model.DocumentStatus == "Reject")
            {
                // Perform reject logic
                var documents = _db.Documents.Where(doc => doc.UserId == model.UserId).ToList();

                if (documents == null || !documents.Any())
                {
                    // await _customLogger.LogAsync($"No documents found for user ID {model.UserId}.");
                    return NotFound("No documents found for the specified UserId.");
                }

                // Update the status and remarks for each document
                foreach (var document in documents)
                {
                    document.DocumentStatus = model.DocumentStatus;
                    document.Remarks = string.IsNullOrWhiteSpace(model.Remarks) ? "Pending" : model.Remarks;
                    // Update any other columns if needed
                }

                // Save changes to the database
                _db.SaveChanges();

                // Send rejection email
                var subject = "Documents Rejected";
                var message = $"Hello {user.Name},<br><br>Your documents have been rejected with the following remarks: {model.Remarks}.<br><br>Best regards,<br>Student Hub";
                await _emailService.SendEmailAsync(user.Email, subject, message);

                // Return a success response
                return RedirectToAction("Index", "Students");
            }

            // Return a success response
            return RedirectToAction("Index", "Students");
        }

        public IActionResult UploadExcel()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (file == null || file.Length == 0)
            {
                _logger.LogError("No file uploaded.");
                // await _customLogger.LogAsync("No file uploaded during Excel file upload.");
                return BadRequest("No file uploaded.");
            }

            var students = new List<Student>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var name = worksheet.Cells[row, 1].Value?.ToString().Trim();
                        var email = worksheet.Cells[row, 2].Value?.ToString().Trim();
                        var contact = worksheet.Cells[row, 3].Value?.ToString().Trim();
                        var address = worksheet.Cells[row, 4].Value?.ToString().Trim();
                        var city = worksheet.Cells[row, 5].Value?.ToString().Trim();
                        var pincode = worksheet.Cells[row, 6].Value?.ToString().Trim();
                        var userIdValue = "1";

                        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                            string.IsNullOrWhiteSpace(contact) || string.IsNullOrWhiteSpace(address) ||
                            string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(pincode) ||
                            string.IsNullOrWhiteSpace(userIdValue))
                        {
                            // await _customLogger.LogAsync($"Invalid data at row {row}. One or more fields are null or empty.");
                            _logger.LogError($"Invalid data at row {row}. One or more fields are null or empty.");
                            continue;
                        }

                        if (!int.TryParse(userIdValue, out int userId))
                        {
                            // await _customLogger.LogAsync($"Invalid UserId at row {row}. UserId should be an integer.");
                            _logger.LogError($"Invalid UserId at row {row}. UserId should be an integer.");
                            continue;
                        }

                        var student = new Student
                        {
                            Name = name,
                            Email = email,
                            Contact = contact,
                            Address = address,
                            City = city,
                            Pincode = pincode,
                            UserId = userId
                        };
                        students.Add(student);
                    }
                }
            }

            if (students.Count > 0)
            {
                _db.Students.AddRange(students);
                await _db.SaveChangesAsync();
                // await _customLogger.LogAsync("Excel file processed successfully. Students added to database.");
            }
            else
            {
                // await _customLogger.LogAsync("No valid students data found in the uploaded file.");
                _logger.LogError("No valid students data found in the uploaded file.");
                return BadRequest("No valid students data found in the uploaded file.");
            }

            ViewBag.Message = "File uploaded successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadExcel()
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
                // await _customLogger.LogAsync("Students data exported to Excel successfully.");
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students.xlsx");
            }
        }

        public async Task<IActionResult> DownloadPdf()
        {
            List<Student> students = _db.Students.ToList();
            using (var stream = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
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
                // await _customLogger.LogAsync("Students data exported to PDF successfully.");
                return File(content, "application/pdf", "Students.pdf");
            }
        }
    }
}
