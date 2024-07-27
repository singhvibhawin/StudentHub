using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectingDatabase.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Invalid Phone number")]
        public string Contact { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [MaxLength(6)]
        [MinLength(6)]
        [RegularExpression(@"^\(?([0-9]{3})\)?([0-9]{3})$", ErrorMessage = "Non Alpha-Numeric characters with minimum length 6")]
        public string Pincode { get; set; }

        public int UserId { get; set; }
        public int DegreeId { get; set; }
    }
}


