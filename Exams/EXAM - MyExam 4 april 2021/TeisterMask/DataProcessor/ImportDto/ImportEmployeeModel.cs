using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TeisterMask.DataProcessor.ImportDto
{

    public class ImportEmployeeModel
    {
        [MinLength(3)]
        [MaxLength(40)]
        [Required]
        [RegularExpression(@"^[A-z0-9]{3,}$")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{4}$")]
        public string Phone { get; set; }
        public int[] Tasks { get; set; }
    }

}
