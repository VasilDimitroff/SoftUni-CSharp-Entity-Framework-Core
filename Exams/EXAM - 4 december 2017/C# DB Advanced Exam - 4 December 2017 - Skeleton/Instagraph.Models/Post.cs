using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Instagraph.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Caption { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int PictureId { get; set; }
        public virtual Picture Picture { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    }
}
