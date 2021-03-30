using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cinema.DataProcessor.ImportDto
{
    class ImportMovieDto
    {
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string Title { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        [Range(1, 10)]
        public double Rating { get; set; }

        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string Director { get; set; }
    }
}
