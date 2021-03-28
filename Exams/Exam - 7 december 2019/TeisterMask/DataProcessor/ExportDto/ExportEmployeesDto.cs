using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TeisterMask.DataProcessor.ExportDto
{
    public class ExportEmployeesDto
    {
        [Required]
        public string Username { get; set; }
        public List<ExportTaskDto> Tasks { get; set; }
    }
}
