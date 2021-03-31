namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners.Where(prisoner => ids.Contains(prisoner.Id))
                .Select(prisoner => new
                {
                    Id = prisoner.Id,
                    Name = prisoner.FullName,
                    CellNumber = prisoner.Cell.CellNumber,
                    Officers = prisoner.PrisonerOfficers.Select(po => new
                    {
                        OfficerName = po.Officer.FullName,
                        Department = po.Officer.Department.Name
                    })
                     .OrderBy(officer => officer.OfficerName)
                    .ToList(),
                    TotalOfficerSalary = decimal.Parse(prisoner.PrisonerOfficers.Sum(po => po.Officer.Salary).ToString("f2"))
                })
                .OrderBy(prisoner => prisoner.Name)
                .ThenBy(prisoner => prisoner.Id)
                .ToArray();

            var json = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var names = prisonersNames.Split(",", StringSplitOptions.RemoveEmptyEntries);

            var prisoners = context.Prisoners
                .Where(prisoner => names.Contains(prisoner.FullName))
                .Select(prisoner => new ExportPrisonerModel
                {
                    Id = prisoner.Id,
                    Name = prisoner.FullName,
                    IncarcerationDate = prisoner.IncarcerationDate.ToString("yyy-MM-dd"),
                    EncryptedMessages = prisoner.Mails.Select(mail => new MessageModel
                    {
                        Description = string.Join("", mail.Description.Reverse())
                    })
                    .ToArray()
                })
                .OrderBy(prisoner => prisoner.Name)
                .ThenBy(prisoner => prisoner.Id)
                .ToArray();

            StringBuilder sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportPrisonerModel[]), new XmlRootAttribute("Prisoners"));
            using (StringWriter writer = new StringWriter(sb))
            {
                xmlSerializer.Serialize(writer, prisoners, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}