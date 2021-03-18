using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.DTO
{
    [XmlType("Car")]
    public class ImportCarDto
    {
        public ImportCarDto()
        {
            this.Parts = new HashSet<PartIdDto>();
        }

        [XmlElement("make")]
        public string Make { get; set; }

        [XmlElement("model")]
        public string Model { get; set; }

        [XmlElement("TraveledDistance")]
        public long TraveledDistance { get; set; }

        [XmlArray("parts")]
        public HashSet<PartIdDto> Parts { get; set; }
    }
}
