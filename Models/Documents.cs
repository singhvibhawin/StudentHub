using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class Documents
    {
        [Key]
        public int DocumentId { get; set; }
        public string FilePaths { get; set; }
        public string DocumentName { get; set; }
        public string DocumentStatus { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
