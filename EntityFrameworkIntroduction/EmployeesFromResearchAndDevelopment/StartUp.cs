using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmployeesFromResearchAndDevelopment.Data;
using EmployeesFromResearchAndDevelopment.Models;

namespace EmployeesFromResearchAndDevelopment
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using var context = new SoftUniContext();
            Console.WriteLine(GetEmployeesFromResearchAndDevelopment(context));
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DepartmentName = x.Department.Name,
                    Salary = x.Salary
                })
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToList();

            foreach (var empl in employees)
            {
                var employee =
                    $"{empl.FirstName} {empl.LastName} from {empl.DepartmentName} - ${empl.Salary:f2}";
                sb.AppendLine(employee);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
