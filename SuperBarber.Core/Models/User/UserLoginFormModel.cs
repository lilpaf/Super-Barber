﻿using System.ComponentModel.DataAnnotations;

namespace SuperBarber.Core.Models.User
{
    public class UserLoginFormModel 
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
