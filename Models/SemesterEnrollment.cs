using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class SemesterEnrollment
    {
        [Key]
        public int SemesterId { get; set; }
        public int Semesters { get; set; }
    }
}
