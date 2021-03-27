using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Data.Models
{
    public class Author
    {
        public int Id { get; set; }

        [MinLength(3)]
        [MaxLength(30)]
        [Required]
        public string FirstName { get; set; }

        [MinLength(3)]
        [MaxLength(30)]
        [Required]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        [RegularExpression(@"^[0-9A-z._-]+@[0-9A-z-]{1,}.[A-z]{1,}$")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{4}$")]
        public string Phone { get; set; }

        public ICollection<AuthorBook> AuthorsBooks { get; set; } = new HashSet<AuthorBook>();
    }
}
