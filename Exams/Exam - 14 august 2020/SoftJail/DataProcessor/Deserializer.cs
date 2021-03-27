namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var depsAndCellsDto = JsonConvert.DeserializeObject<ImportDepartmentsAndCellsDto[]>(jsonString);

            var departmentsToAdd = new List<Department>();
            var sb = new StringBuilder();

            foreach (var depAndCellDto in depsAndCellsDto)
            { 
                var departmentCellsCount = depAndCellDto.Cells.Count();

                if (!IsValid(depAndCellDto) || !IsValid(depAndCellDto.Name) || departmentCellsCount < 1 )
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                bool isCellsValid = true;

                var department = new Department
                {
                    Name = depAndCellDto.Name
                };       

                foreach (var cellDto in depAndCellDto.Cells)
                {
                    if (!IsValid(cellDto))
                    {
                        sb.AppendLine("Invalid Data");
                        isCellsValid = false;
                        break;
                    }

                    var cell = new Cell
                    {
                        CellNumber = cellDto.CellNumber,
                        Department = department,
                        HasWindow = cellDto.HasWindow
                    };

                    department.Cells.Add(cell);
                }

                if (!isCellsValid)
                {
                    continue;
                }

                departmentsToAdd.Add(department);
                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
            }

            context.Departments.AddRange(departmentsToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersDto = JsonConvert.DeserializeObject<ImportPrisonerDto[]>(jsonString);

            var sb = new StringBuilder();
            var prisoners = new List<Prisoner>();

            foreach (var prisonerDto in prisonersDto)
            {
                if (!IsValid(prisonerDto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var prisoner = new Prisoner
                {
                    FullName = prisonerDto.FullName,
                    Nickname = prisonerDto.Nickname,
                    Age = prisonerDto.Age,
                    IncarcerationDate = DateTime.ParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Bail = prisonerDto.Bail,
                    CellId = prisonerDto.CellId,
                };

                if (prisonerDto.ReleaseDate == null)
                {
                    prisoner.ReleaseDate = null;
                }

                else
                {
                    prisoner.ReleaseDate = DateTime.ParseExact(prisonerDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                bool isPrisonerValid = true;

                foreach (var mailDto in prisonerDto.Mails)
                {
                    if (!IsValid(mailDto))
                    {
                        isPrisonerValid = false;
                        sb.AppendLine("Invalid Data");
                        continue;
                    }

                    var mail = new Mail
                    {
                        Description = mailDto.Description,
                        Sender = mailDto.Sender,
                        Address = mailDto.Address
                    };

                    prisoner.Mails.Add(mail);
                }

                if (!isPrisonerValid)
                {
                    continue;
                }

                prisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var serializer =
                new XmlSerializer(typeof(ImportOfficerDto[]), new XmlRootAttribute("Officers"));

            var stringReader = new StringReader(xmlString);

            var officersDto = (ImportOfficerDto[])serializer.Deserialize(stringReader);

            var sb = new StringBuilder();

            List<Officer> officers = new List<Officer>();

            foreach (var officerDto in officersDto)
            {
                if (!IsValid(officerDto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Position position;
                Weapon weapon;

                var isParsedPosition = Enum.TryParse<Position>(officerDto.Position, out position);
                var isParsedWeapon = Enum.TryParse<Weapon>(officerDto.Weapon, out weapon);

                var officer = new Officer
                {
                    FullName = officerDto.Name,
                    Salary = officerDto.Money,
                    DepartmentId = officerDto.DepartmentId
                };

                if (isParsedPosition)
                {
                    officer.Position = position;
                }

                else
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                if (isParsedWeapon)
                {
                    officer.Weapon = weapon;
                }

                else
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }


                foreach (var prisonerDto in officerDto.Prisoners)
                {
                    var officerPrisoner = new OfficerPrisoner
                    {
                        PrisonerId = prisonerDto.Id,
                        Officer = officer
                    };

                    officer.OfficerPrisoners.Add(officerPrisoner);
                }

                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count()} prisoners)");

                officers.Add(officer);
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}