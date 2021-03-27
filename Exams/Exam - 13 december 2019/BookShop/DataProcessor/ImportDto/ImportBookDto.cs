using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace BookShop.DataProcessor.ImportDto
{
    [XmlType("Book")]
    public class ImportBookDto
    {
        [XmlElement("Name")]
        [MinLength(3)]
        [MaxLength(30)]
        [Required]
        public string Name { get; set; }

        [XmlElement("Genre")]
        [Required]
        public int GenreInt { get; set; }

        [XmlElement("Price")]
        [Range(typeof(decimal), "0.01", "9999999999999999.99")]
        public decimal? Price { get; set; }

        [XmlElement("Pages")]
         [Range(50, 5000)]
        public int? Pages { get; set; }

        [XmlElement("PublishedOn")]
        [Required]
        public string PublishedOn { get; set; }
    }
}
