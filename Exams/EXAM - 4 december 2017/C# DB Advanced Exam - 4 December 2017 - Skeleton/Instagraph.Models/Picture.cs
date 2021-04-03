using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Instagraph.Models
{
    public class Picture
    {
        public int Id { get; set; }

        [Required]   
        [MinLength(1)]
        public string Path { get; set; }

        [Range(typeof(decimal), "0.00000000000000001", "79228162514264337593543950335")]
        public decimal Size { get; set; }

        public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
    }
}
