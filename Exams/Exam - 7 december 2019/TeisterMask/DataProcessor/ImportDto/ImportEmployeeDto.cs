﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TeisterMask.DataProcessor.ImportDto
{
    public class ImportEmployeeDto
    {

        [RegularExpression(@"^[0-9A-z]{3,}$")]
        [MaxLength(40)]
        [Required]
        public string Username { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{4}$")]
        [Required]
        public string Phone { get; set; }
        public HashSet<int> Tasks { get; set; }
    }
}