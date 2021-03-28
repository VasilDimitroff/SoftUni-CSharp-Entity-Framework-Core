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
            var projects = context.Projects.Where(p => p.Tasks.Count() > 0)
                .Select(project => new ExportProjectDto
                {
                    TasksCount = project.Tasks.Count(),
                    ProjectName = project.Name,
                    HasEndDate = project.DueDate.HasValue ? "Yes" : "No",
                    Tasks = project.Tasks.Select(task => new TaskDto
                    {
                        Name = task.Name,
                        Label = task.LabelType.ToString()
                    })
                    .OrderBy(task => task.Name)
                    .ToArray()
                })
                .OrderByDescending(project => project.TasksCount)
                .ThenBy(project => project.ProjectName)
                .ToArray();


            var serializer = new XmlSerializer(typeof(ExportProjectDto[]), new XmlRootAttribute("Projects"));
            StringBuilder sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (StringWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, projects, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .Where(employee => employee.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .Select(employee => new ExportEmployeesDto
                {
                    Username = employee.Username,
                    Tasks = employee.EmployeesTasks
                    .Where(et => et.Task.OpenDate >= date)
                    .OrderByDescending(et => et.Task.DueDate)
                    .ThenBy(et => et.Task.Name)
                    .Select(et => new ExportTaskDto
                    {
                        TaskName = et.Task.Name,
                        OpenDate = et.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                        DueDate = et.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                        LabelType = et.Task.LabelType.ToString(),
                        ExecutionType = et.Task.ExecutionType.ToString()
                    })
                    .ToList()
                })
                .OrderByDescending(emp => emp.Tasks.Count())
                .ThenBy(emp => emp.Username)
                .Take(10)
                .ToList();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }
    }
}