using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlType("User")]
    public class UserWithSoldProductsDto
    {
        [XmlElement("firstName")]
        public string FirstName { get; set; }
        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlElement("soldProducts")]
        public ProductWithNameAndPriceDto[] SoldProducts { get; set; }
    }
}