using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MusicHub.Data.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Price  => this.Songs.Sum(song => song.Price);
        public int? ProducerId { get; set; }
        public virtual Producer Producer { get; set; }
        public virtual ICollection<Song> Songs { get; set; } = new HashSet<Song>();
    }
}
//Id – Integer, Primary Key
//Name – Text with max length 40 (required)
//ReleaseDate – Date(required)
//Price – calculated property(the sum of all song prices in the album)
//ProducerId – integer, Foreign key
//Producer – the album’s producer
//Songs – collection of all Songs in the Album 