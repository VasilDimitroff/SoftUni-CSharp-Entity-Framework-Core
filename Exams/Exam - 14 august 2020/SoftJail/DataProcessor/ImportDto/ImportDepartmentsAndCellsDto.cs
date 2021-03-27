using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class ImportDepartmentsAndCellsDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        [Required]
        public List<ImportCellDto> Cells { get; set; }
    }

} 
