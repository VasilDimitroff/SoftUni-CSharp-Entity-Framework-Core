using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class ImportCellDto
    {
        [Required]
        [Range(1, 1000)]
        [JsonProperty]
        public int CellNumber { get; set; }

        [Required]
        [JsonProperty]
        public bool HasWindow { get; set; }
    }
}
