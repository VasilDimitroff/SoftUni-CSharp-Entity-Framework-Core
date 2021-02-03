using System;
using Microsoft.EntityFrameworkCore;
using EmployeesWithSalaryOver50000.Models;
using EmployeesWithSalaryOver50000.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace EmployeesWithSalaryOver50000
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using var context = new SoftUniContext();
            Console.WriteLine(GetEmployeesWithSalaryOver50000(context));
            context.Dispose();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            List<Employee> employees = context.Employees.Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName)
                .ToList();

            foreach (var employee in employees)
            {
                var currentEmployee = $"{employee.FirstName} - {employee.Salary:f2}";
                sb.AppendLine(currentEmployee);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
