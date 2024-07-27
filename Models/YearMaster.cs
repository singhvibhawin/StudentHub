using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class YearMaster
    {
        [Key]
        public int YearId { get; set; }
        public string Year { get; set; }
    }
}
