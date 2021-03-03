using MusicHub.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MusicHub.Data.Models
{
    public class Song
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Required]
        public string Name { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required]
        public virtual Genre Genre { get; set; }

        public int? AlbumId { get; set; }

        public virtual Album Album { get; set; }

        public int WriterId { get; set; }

        public virtual Writer Writer { get; set; }

        public decimal Price { get; set; }

        public virtual ICollection<SongPerformer> SongPerformers { get; set; } = new HashSet<SongPerformer>();


    }

//    Id – Integer, Primary Key
//Name – Text with max length 20 (required)
//Duration – TimeSpan(required)
//CreatedOn – Date(required)
//Genre – Genre enumeration with possible values: "Blues, Rap, PopMusic, Rock, Jazz" (required)
//AlbumId – Integer, Foreign key

//WriterId – Integer, Foreign key(required)
//Writer – The song’s writer
//Price – Decimal(required)
//SongPerformers – Collection of type SongPerformer
}
