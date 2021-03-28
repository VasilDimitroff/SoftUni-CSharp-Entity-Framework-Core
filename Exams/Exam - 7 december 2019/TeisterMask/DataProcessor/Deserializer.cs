namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using TeisterMask.DataProcessor.ImportDto;
    using System.Xml.Serialization;
    using System.Text;
    using System.IO;
    using TeisterMask.Data.Models;
    using System.Globalization;
    using TeisterMask.Data.Models.Enums;
    using Newtonsoft.Json;
    using System.Linq;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportProjectDto[]), new XmlRootAttribute("Projects"));
            var projectsDto = (ImportProjectDto[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();

            var projects = new List<Project>();

            foreach (var projDto in projectsDto)
            {
                if (!IsValid(projDto))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                DateTime projOpenDate;
                var isProjOpenDateParsed = DateTime.TryParseExact(projDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out projOpenDate);

                DateTime projDueDate;
                var isProjDueDateParsed = DateTime.TryParseExact(projDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out projDueDate);

                if (!isProjOpenDateParsed)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var project = new Project()
                {
                    Name = projDto.Name,
                    OpenDate = projOpenDate,
                    DueDate = projDueDate
                };

                if (!isProjDueDateParsed)
                {
                    project.DueDate = null;
                }

                foreach (var taskDto in projDto.Tasks)
                {
                    if (!IsValid(taskDto))
                    {
                        sb.AppendLine("Invalid data!");
                        continue;
                    }
                    DateTime taskOpenDate;

                    bool isTaskOpenDateValid = DateTime.TryParseExact(taskDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out taskOpenDate);
                   
                    DateTime taskDueDate;

                    bool isTaskDueDateValid = DateTime.TryParseExact(taskDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out taskDueDate);

                    if (taskOpenDate < projOpenDate  || !isTaskOpenDateValid || !isTaskDueDateValid)
                    {
                        sb.AppendLine("Invalid data!");
                        continue;
                    }

                    if (projDueDate != default)
                    {
                        if (taskDueDate > projDueDate)
                        {
                            sb.AppendLine("Invalid data!");
                            continue;
                        }           
                    }

                    
                    LabelType labelType;
                    ExecutionType executionType;
                    bool isExecutionTypeParsed = Enum.TryParse<ExecutionType>(taskDto.ExecutionType, out executionType);
                    bool isLabelTypeParsed = Enum.TryParse<LabelType>(taskDto.LabelType, out labelType);

                    if (!isExecutionTypeParsed || !isLabelTypeParsed)
                    {
                        sb.AppendLine("Invalid data!");
                        continue;
                    }

                    var task = new Task()
                    {
                        Name = taskDto.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = executionType,
                        LabelType = labelType,
                        Project = project
                    };

                    project.Tasks.Add(task);
                }

                projects.Add(project);
                sb.AppendLine($"Successfully imported project - {project.Name} with {project.Tasks.Count} tasks.");
            }

            context.Projects.AddRange(projects);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var employeesDtos = JsonConvert.DeserializeObject<ImportEmployeeDto[]>(jsonString);
            
            var sb = new StringBuilder();

            var employees = new List<Employee>();

            foreach (var empDto in employeesDtos)
            {
                if (!IsValid(empDto))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var employee = new Employee()
                {
                    Username = empDto.Username,
                    Email = empDto.Email,
                    Phone = empDto.Phone
                };

                foreach (var task in empDto.Tasks)
                {
                    var currentTask = context.Tasks.FirstOrDefault(x => x.Id == task);

                    if (currentTask == null)
                    {
                        sb.AppendLine("Invalid data!");
                        continue;
                    }

                    var  currentEmployeeTask = new EmployeeTask()
                    {
                        TaskId = task,
                        Employee = employee
                    };              

                    employee.EmployeesTasks.Add(currentEmployeeTask);
                }

                employees.Add(employee);
                sb.AppendLine($"Successfully imported employee - {employee.Username} with {employee.EmployeesTasks.Count} tasks.");
            }

            context.Employees.AddRange(employees);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}