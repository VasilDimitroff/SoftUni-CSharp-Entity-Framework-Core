using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class ImportPrisonerWithMailModel
    {
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"^The [A-Z][A-z]{1,}$")]
        public string Nickname { get; set; }

        [Range(18, 65)]
        public int Age { get; set; }

        [Required]
        public string IncarcerationDate { get; set; }
        public string ReleaseDate { get; set; }
        public decimal? Bail { get; set; }
        public int? CellId { get; set; }
        public ImportPrisonerMailModel[] Mails { get; set; }
    }

    public class ImportPrisonerMailModel
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string Sender { get; set; }

        [Required]
        [RegularExpression(@"^[0-9A-z\s]{1,} str.$")]
        public string Address { get; set; }
    }

}
