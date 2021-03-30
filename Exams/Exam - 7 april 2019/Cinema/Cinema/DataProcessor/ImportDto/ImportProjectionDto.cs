using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Cinema.DataProcessor.ImportDto
{
    [XmlType("Projection")]
    public class ImportProjectionDto
    {
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public string DateTime { get; set; }
    }
}
