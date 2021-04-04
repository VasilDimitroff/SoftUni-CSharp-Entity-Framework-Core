namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportProjectXMLModel[]), new XmlRootAttribute("Projects"));

            var projects = (ImportProjectXMLModel[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();

            foreach (var proj in projects)
            {
                if (!IsValid(proj))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime projectOpenDate;
                bool isProjectOpenDateValid = DateTime.TryParseExact(proj.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
    DateTimeStyles.None, out projectOpenDate);

                DateTime projectDueDateNotNullable;
                bool isProjectDueDateValid = DateTime.TryParseExact(proj.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
    DateTimeStyles.None, out projectDueDateNotNullable);

                DateTime? projectDueDate = projectDueDateNotNullable;

                if (proj.DueDate == null || proj.DueDate == string.Empty)
                {
                    projectDueDate = null;
                }

                if (!isProjectOpenDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var project = new Project()
                {
                    Name = proj.Name,
                    OpenDate = projectOpenDate,
                    DueDate = projectDueDate
                };

                foreach (var task in proj.Tasks)
                {
                    if (!IsValid(task))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime taskOpenDate;
                    bool isTaskOpenDateValid = DateTime
                        .TryParseExact(task.OpenDate,
                        "dd/MM/yyyy", 
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out taskOpenDate);

                    DateTime taskDueDate;
                    bool isTaskDueDateValid = DateTime
                        .TryParseExact(task.DueDate,
                        "dd/MM/yyyy", 
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out taskDueDate);

                    //Must I check if values of dates are null??
                    if (!isTaskDueDateValid || !isTaskOpenDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (taskOpenDate < projectOpenDate || taskDueDate > projectDueDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    ExecutionType executionType;
                    bool isExecutionTypeParsed = 
                        Enum.TryParse<ExecutionType>(task.ExecutionType, out executionType);

                    LabelType labelType;
                    bool isLabelTypeParsed =
                        Enum.TryParse<LabelType>(task.LabelType, out labelType);

                    if (!isExecutionTypeParsed || !isLabelTypeParsed)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var validTask = new Task
                    {
                        Name = task.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = executionType,
                        LabelType = labelType
                    };

                    project.Tasks.Add(validTask);                
                }

                context.Projects.Add(project);
                sb.AppendLine($"Successfully imported project - {project.Name} with {project.Tasks.Count} tasks.");
            }

            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var employeeDtos = JsonConvert.DeserializeObject<ImportEmployeeModel[]>(jsonString);

            var employeesToAdd = new List<Employee>();

            foreach (var employee in employeeDtos)
            {
                if (!IsValid(employee))
                {
                    sb.AppendLine(ErrorMessage);

                    continue;
                }

                if (employee.Username.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    sb.AppendLine(ErrorMessage);
                }

                var employeeToAdd = new Employee()
                {
                    Username = employee.Username,
                    Email = employee.Email,
                    Phone = employee.Phone
                };

                foreach (var taskId in employee.Tasks.Distinct())
                {
                    if (context.Tasks.All(t => t.Id != taskId))
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    var taskToAdd = context.Tasks.FirstOrDefault(t => t.Id == taskId);

                    var employeeTask = new EmployeeTask()
                    {
                        Employee = employeeToAdd,
                        Task = taskToAdd
                    };

                    employeeToAdd.EmployeesTasks.Add(employeeTask);
                }

                employeesToAdd.Add(employeeToAdd);

                sb.AppendLine(string.Format(SuccessfullyImportedEmployee, employeeToAdd.Username,
                    employeeToAdd.EmployeesTasks.Count));
            }

            context.Employees.AddRange(employeesToAdd);

            context.SaveChanges();

            return sb.ToString().Trim();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}