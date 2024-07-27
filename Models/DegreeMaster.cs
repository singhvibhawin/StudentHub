using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class DegreeMaster
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string DegreeName { get; set; }
    }
}
