using Microsoft.EntityFrameworkCore;
using System;
using EmployeesFullInformation.Models;
using EmployeesFullInformation.Data;
using System.Linq;
using System.Text;

namespace EmployeesFullInformation
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using (var context = new SoftUniContext())
            {
                Console.WriteLine(GetEmployeesFullInformation(context)); 
            }
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees.Select(x => new
            {
                Id = x.EmployeeId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                JobTitle = x.JobTitle,
                Salary = x.Salary
            })
               .OrderBy(x=> x.Id)
               .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in employees)
            {
                var currentEmployee =
                    $"{emp.FirstName} {emp.LastName} {emp.MiddleName} {emp.JobTitle} {emp.Salary:f2}";

                sb.AppendLine(currentEmployee);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
