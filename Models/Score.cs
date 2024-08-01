using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class Score
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int DegreeId { get; set; }
        public int SubjectId { get; set; }
        public int Marks { get; set; }
    }
}
