namespace SoftJail.DataProcessor
{
    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
                .Where(pr => ids.Contains(pr.Id))
                .Select(prisoner => new PrisonersWithCellsDto
                {
                    Id = prisoner.Id,
                    Name = prisoner.FullName,
                    CellNumber = prisoner.Cell.CellNumber,
                    Officers = prisoner.PrisonerOfficers.Select(officer => new OfficerDto
                    {
                        OfficerName = officer.Officer.FullName,
                        Department = officer.Officer.Department.Name
                    })
                    .OrderBy(x => x.OfficerName)
                    .ToList(),
                    TotalOfficerSalary = Math.Round((Decimal)prisoner.PrisonerOfficers.Sum(of => of.Officer.Salary), 2),
                })
                .OrderBy(x => x.Name)
                .ThenBy(x=> x.Id)
                .ToArray();

            var json = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var prisoners = context
                .Prisoners
                .ToArray()
                .Where(p => prisonersNames.Contains(p.FullName))
                .Select(p => new ExportPrisonerDto
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EncryptedMessages = p.Mails
                       
                        .Select(m => new MessageDto
                        {
                            Description = ReverseString(m.Description)
                        })
                        .ToList()

                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToArray();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(ExportPrisonerDto[]), new XmlRootAttribute("Prisoners"));

            using (StringWriter writer = new StringWriter(sb))
            {
                xmlSerializer.Serialize(writer, prisoners, namespaces);
            }

            return sb.ToString().Trim();
        }

        private static string ReverseString(string s)
        {
            char[] array = s.ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }
    }
}