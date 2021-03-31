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
            var departmentModels =
                JsonConvert.DeserializeObject<ImportDepartmentModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var model in departmentModels)
            {
                if (!IsValid(model) || model.Cells.Length < 1)
                {
                    sb.AppendLine($"Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = model.Name,
                };

                bool isInvalidCell = false;

                foreach (var cellModel in model.Cells)
                {

                    if (!IsValid(cellModel))
                    {
                        sb.AppendLine($"Invalid Data");
                        isInvalidCell = true;
                        continue;
                    }

                    department.Cells.Add(new Cell
                    {
                        CellNumber = cellModel.CellNumber,
                        HasWindow = cellModel.HasWindow
                    });
                }

                if (isInvalidCell)
                {
                    continue;
                }

                context.Departments.Add(department);
                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersModels = JsonConvert.DeserializeObject<ImportPrisonerWithMailModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var prisModel in prisonersModels)
            {
                if (!IsValid(prisModel))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var prisoner = new Prisoner
                {
                    FullName = prisModel.FullName,
                    Nickname = prisModel.Nickname,
                    Age = prisModel.Age,
                    Bail = prisModel.Bail,
                    CellId = prisModel.CellId
                };

                DateTime incarcerationDate;
                bool isIncarcerationDateParsed = DateTime.TryParseExact(prisModel.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out incarcerationDate);

                DateTime releaseDate;
                bool isReleaseDateParsed = DateTime.TryParseExact(prisModel.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

                if (!isIncarcerationDateParsed)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                prisoner.IncarcerationDate = incarcerationDate;

                if (prisModel.ReleaseDate == null)
                {
                    prisoner.ReleaseDate = null;
                }

                else if (isReleaseDateParsed)
                {
                    DateTime? date = releaseDate;
                    prisoner.ReleaseDate = date;
                }

                else
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                bool isValid = true;

                foreach (var mailModel in prisModel.Mails)
                {
                    if (!IsValid(mailModel))
                    {
                        sb.AppendLine("Invalid Data");
                        isValid = false;
                        break;
                    }

                    var mail = new Mail
                    {
                        Address = mailModel.Address,
                        Sender = mailModel.Sender,
                        Description = string.Join("", mailModel.Description.Reverse()),
                        Prisoner = prisoner
                    };

                    prisoner.Mails.Add(mail);
                }

                if (!isValid)
                {
                    continue;
                }

                context.Prisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportOfficersModel[]), new XmlRootAttribute("Officers"));
            var officerModels = (ImportOfficersModel[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();

            foreach (var officerModel in officerModels)
            {
                if (!IsValid(officerModel))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer
                {
                    FullName = officerModel.Name,
                    Salary = officerModel.Money,
                    DepartmentId = officerModel.DepartmentId
                };

                Position position;
                bool isPositionParsed = Enum.TryParse<Position>(officerModel.Position, out position);

                Weapon weapon;
                bool isWeaponParsed = Enum.TryParse<Weapon>(officerModel.Weapon, out weapon);

                if (!isWeaponParsed || !isPositionParsed)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                officer.Position = position;
                officer.Weapon = weapon;

                foreach (var prisModel in officerModel.Prisoners)
                {
                    if (!IsValid(prisModel))
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }

                    var officerPrisoner = new OfficerPrisoner
                    {
                        Officer = officer,
                        PrisonerId = prisModel.Id
                    };

                   officer.OfficerPrisoners.Add(officerPrisoner);
                }

                context.Officers.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }

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