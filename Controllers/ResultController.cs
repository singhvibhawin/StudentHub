using ConnectingDatabase.Data;
using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Utilities;
using System.Linq;

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

                return RedirectToAction("ApprovedStudents");
            }
            else
            {
                return BadRequest("Error in getting values, Try Again!");
            }
        }

        public IActionResult UploadScores(int id)
        {
            // Fetch enrolled data for the student
            var enrolledData = _db.tbl_student_degree_enrollment.Where(enrollment => enrollment.StudentId == id).ToList();

            // Get distinct subject IDs
            var subjectIds = enrolledData.SelectMany(e => e.SubjectId.Split(',').Select(id => int.Parse(id))).Distinct().ToList();

            // Fetch subjects based on subject IDs
            var subjects = _db.tbl_subject_master.Where(subject => subjectIds.Contains(subject.SubjectId)).ToList();

            // Get distinct degree IDs
            var degreeIds = enrolledData.Select(e => e.DegreeId).Distinct().FirstOrDefault();

            // Fetch degrees based on degree IDs
            var degrees = _db.tbl_degree_master.Where(d => d.Id == degreeIds).Select(e => e.DegreeName).FirstOrDefault();

            var studentName = _db.Students.Where(i => i.StudentId == id).Select(e => e.Name).FirstOrDefault();

            var scoresList = _db.Marks.Where(i => i.StudentId == id).ToList();


            // Prepare data for the view if necessary
            var viewModel = new UploadScoresViewModel
            {
                StudentId = id,
                StudentName = studentName,
                Subjects = subjects,
                DegreeId = degreeIds,
                DegreeName = degrees,
                Scores = scoresList 
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UploadScores(Scores scores)
        {
            if (ModelState.IsValid)
            {
                for (int i = 0; i < scores.SubjectId.Count; i++)
                {
                    var score = new Score
                    {
                        StudentId = scores.StudentId,
                        DegreeId = scores.DegreeId,
                        SubjectId = scores.SubjectId[i],
                        Marks = scores.Marks[i]
                    };

                    _db.Marks.Add(score);
                    _db.SaveChanges();
                }
                return RedirectToAction("ApprovedStudents");
            }

            return View();
        }
    }
}
