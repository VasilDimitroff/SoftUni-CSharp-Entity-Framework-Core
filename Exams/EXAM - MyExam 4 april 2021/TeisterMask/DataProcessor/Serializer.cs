namespace TeisterMask.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var projects = context.Projects
                .Where(p => p.Tasks.Count() > 0)
                .ToArray()
                .Select(project => new ExportProjectsModel
                {
                    TasksCount = project.Tasks.Count,
                    ProjectName = project.Name,
                    HasEndDate = project.DueDate.HasValue ? "Yes" : "No",
                    Tasks = project.Tasks.ToArray().Select(task => new ExportTaskModel
                    {
                        Name = task.Name,
                        Label = task.LabelType.ToString(),
                    })
                    .OrderBy(task => task.Name)
                    .ToArray()
                })
                .OrderByDescending(project => project.TasksCount)
                .ThenBy(project => project.ProjectName)
                .ToArray();

            StringBuilder sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportProjectsModel[]), new XmlRootAttribute("Projects"));
            using (StringWriter writer = new StringWriter(sb))
            {
                xmlSerializer.Serialize(writer, projects, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .Where(employee => employee.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .ToArray()
                .Select(employee => new
                {
                    Username = employee.Username,
                    Tasks = employee.EmployeesTasks.Where(et => et.Task.OpenDate >= date)
                    .OrderByDescending(task => task.Task.DueDate)
                    .ThenBy(task => task.Task.Name)
                    .ToArray()
                    .Select(task => new
                    {
                        TaskName = task.Task.Name,
                        OpenDate = task.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                        DueDate = task.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                        LabelType = task.Task.LabelType.ToString(),
                        ExecutionType = task.Task.ExecutionType.ToString()
                    })
                    .ToArray()
                })
                .OrderByDescending(employee => employee.Tasks.Count())
                .ThenBy(employee => employee.Username)
                .Take(10)
                .ToArray();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }
    }
}