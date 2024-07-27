using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class StudentDegreeEnrollment
    {
        [Key]
        public int Id {  get; set; }
        public int UserId { get; set; }
        public int StudentId { get; set; }
        public int DegreeId { get; set; }
        public int SemesterId { get; set; }
        public int YearId { get; set; }
        public string SubjectId { get; set; }
    }
}
