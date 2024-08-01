using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class Scores
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Marks are required.")]
        public List<int> SubjectId { get; set; }
        public List<int> Marks { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int DegreeId { get; set; }

    }

}
