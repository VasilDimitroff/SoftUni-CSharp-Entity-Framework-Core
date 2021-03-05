using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO
{
    public class ImportCustomerDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonProperty("isYoungDriver")]
        public bool IsYoungDriver { get; set; }
    }
}
