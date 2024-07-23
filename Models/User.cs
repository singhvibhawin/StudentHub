﻿using System.ComponentModel.DataAnnotations;

namespace ConnectingDatabase.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(30)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
    }
}
