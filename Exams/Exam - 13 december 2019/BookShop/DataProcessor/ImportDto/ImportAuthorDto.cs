using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.DataProcessor.ImportDto
{
    public class ImportAuthorDto
    {
        [MinLength(3)]
        [MaxLength(30)]
        [Required]
        public string FirstName { get; set; }

        [MinLength(3)]
        [MaxLength(30)]
        [Required]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{4}$")]
        public string Phone { get; set; }

        [EmailAddress]
        [Required]
        [RegularExpression(@"^[0-9A-z._-]+@[0-9A-z-]{1,}.[A-z]{1,}.*[A-z]*$")]
        public string Email { get; set; }
        public List<BookDto> Books { get; set; }
    }
}
