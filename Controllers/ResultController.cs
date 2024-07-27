using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConnectingDatabase.Controllers
{
    public class ResultController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ResultController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult ApprovedStudents()
        {
            // Getting userId from Docs table
            var approvedDocs = _db.Documents.Where(status => status.DocumentStatus == "Approve").Select(uid => uid.UserId).Distinct().ToList();

            // Getting student data where userId matches in students table
            var approvedStudents = _db.Students.Where(s => approvedDocs.Contains(s.UserId)).ToList();
            
            var degreeIds = approvedStudents.Select(did => did.DegreeId).ToList();

            var degrees = _db.tbl_degree_master.Where(d => degreeIds.Contains(d.Id)).ToList();

            var subjectsObj = _db.tbl_subject_master.ToList();
            var semesterObj = _db.tbl_semester_enrollment.ToList();
            var yearObj = _db.tbl_year_master.ToList();

            // Join students with degrees to the view model
            var ApprovedData = approvedStudents
                .Join(degrees,
                      student => student.DegreeId,
                      degree => degree.Id,
                      (student, degree) => new ApprovedDataViewModel
                      {
                          UserId = student.UserId,
                          StudentId = student.StudentId,
                          StudentName = student.Name,
                          DegreeId = degree.Id,
                          DegreeName = degree.DegreeName,
                          Subjects = subjectsObj,   // Add the additional data
                          Years = yearObj,
                          Semesters = semesterObj
                      })
                .ToList();

            return View(ApprovedData);
        }

        [HttpPost]
        public IActionResult degreeEnrollment([FromBody] ApprovedDataViewModel model)
        {
            if (model != null)
            {
                if (model.SubjectIds == null || !model.SubjectIds.Any())
                {
                    return BadRequest("No subjects selected");
                }

                var enrollment = new StudentDegreeEnrollment
                {
                    UserId = model.UserId,
                    StudentId = model.StudentId,
                    DegreeId = model.DegreeId,
                    SemesterId = model.SemesterId,
                    YearId = model.YearId,
                    SubjectId = string.Join(",", model.SubjectIds)
                };

                _db.tbl_student_degree_enrollment.Add(enrollment);
                _db.SaveChanges();                
            }
            else
            {
                return BadRequest("Error in getting values, Try Again!");
            }

            return View();
        }

    }
}
