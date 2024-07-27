using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class SubjectMaster
    {
        [Key]
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
    }
}
