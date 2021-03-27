using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class ImportUserDto
    {
        [Required]
        [RegularExpression(@"^[A-Z][a-z]+\s+[A-Z][a-z]+$")]
        public string FullName { get; set; }

        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Range(3, 103)]
        public int Age { get; set; }
   
        public List<ImportCardDto> Cards { get; set; }
    }
}
