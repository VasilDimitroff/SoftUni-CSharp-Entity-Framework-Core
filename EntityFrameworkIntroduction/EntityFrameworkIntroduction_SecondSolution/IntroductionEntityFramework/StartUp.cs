using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using var context = new SoftUniContext();

            Console.WriteLine(RemoveTown(context));
        }

        //Problem 03
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees.Select(e => new
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                MiddleName = e.MiddleName,
                LastName = e.LastName,
                JobTitle = e.JobTitle,
                Salary = e.Salary
            })
                .OrderBy(e => e.Id)
                .ToList();
            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                var currentLine = $"{employee.FirstName} " +
                    $"{employee.LastName} {employee.MiddleName} " +
                    $"{employee.JobTitle} {employee.Salary:f2}";

                sb.AppendLine(currentLine);
            }

            return sb.ToString().TrimEnd();
        }

        //Prolbem 04 
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Salary > 50000)
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} - {e.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 05
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {

            var employees = context.Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(e => new
                { 
                    e.FirstName,
                    e.LastName,
                    Department = e.Department.Name,
                    e.Salary
                })
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb
                    .AppendLine($"{e.FirstName} {e.LastName} from {e.Department} - ${e.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 06
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address();

            address.AddressText = "Vitoshka 15";
            address.TownId = 4;

            context.Addresses.Add(address);

            var employeeNakov = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");
            employeeNakov.Address = address;

            context.SaveChanges();

            var employees = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => new
                {
                    AddressText = e.Address.AddressText
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb
                    .AppendLine($"{e.AddressText}");
            }

            return sb.ToString().TrimEnd();

        }

        //Problem 07

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {

            var employees = context.Employees
                .Where(e => e.EmployeesProjects
                    .Any(p => p.Project.StartDate.Year >= 2001 && p.Project.StartDate.Year <= 2003))
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects.Select( p => new
                    {
                        Name = p.Project.Name,
                        StartDate = p.Project.StartDate,
                        EndDate = p.Project.EndDate
                    })
                })
                .Take(10)
                .ToList();


            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb
                    .AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

                foreach (var p in e.Projects)
                {
                    var endDate = p.EndDate?.ToString("M/d/yyyy h:mm:ss tt");

                    if (endDate == null)
                    {
                        endDate = "not finished";
                    }

                    sb.AppendLine
                        ($"--{p.Name} - {p.StartDate.ToString("M/d/yyyy h:mm:ss tt")} - {endDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 08

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var adresses = context.Addresses
                .OrderByDescending(a => a.Employees.Count)
                .ThenBy(a => a.Town.Name)
                .ThenBy(a => a.AddressText)
                //.Select( a =>  new
                //{
                //    TownName = a.Town.Name,
                //    AddressText = a.AddressText,
                //    EmployeesCount = a.Employees.Count
                //})
                .Take(10)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var ad in adresses)
            {
                sb
                    .AppendLine($"{ad.AddressText}, {ad.Town.Name} - {ad.Employees.Count} employees"); 
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 09
        public static string GetEmployee147(SoftUniContext context)
        {
            var employee147 = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects.Select(ep => new
                    {
                        Name = ep.Project.Name
                    }).OrderBy(p => p.Name)
                   .ToList()

                })
                .ToList();

            var result = new StringBuilder();
            foreach (var item in employee147)
            {
                result.AppendLine($"{item.FirstName} {item.LastName} - {item.JobTitle}");

                foreach (var p in item.Projects)
                {
                    result.AppendLine($"{p.Name}");
                }
            }
              

            return result.ToString().TrimEnd();
        }

        //Problem 10
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    Department = d.Name,
                    ManagerFN = d.Manager.FirstName,
                    ManagerLN = d.Manager.LastName,
                    Employees = d.Employees.Select(e => new
                    {
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        JobTitle = e.JobTitle
                    })
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToList()
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var d in departments)
            {
                sb.AppendLine($"{d.Department} - {d.ManagerFN} {d.ManagerLN}");

                foreach (var e in d.Employees)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
                }
            }

            return sb.ToString().TrimEnd();

        }

        //Problem 11

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .OrderByDescending(p => p.StartDate)  
                .Take(10)   
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    StartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt")
                })
                .ToList();

            projects = projects.OrderBy(x => x.Name).ToList();

            var sb = new StringBuilder();

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Name}");
                sb.AppendLine($"{p.Description}");
                sb.AppendLine($"{p.StartDate}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 12
        public static string IncreaseSalaries(SoftUniContext context)
        {

            var employeesToIncrease = context.Employees
                .Where(e => e.Department.Name == "Engineering" ||
                   e.Department.Name == "Tool Design" ||
                   e.Department.Name == "Marketing" ||
                   e.Department.Name == "Information Services")
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in employeesToIncrease)
            {
                e.Salary = e.Salary * 1.12m; 
            }

            var sb = new StringBuilder();

            foreach (var e in employeesToIncrease)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        //Problem 13
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Salary = e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 14

        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectMapForDelete = context.EmployeesProjects
                .Where(ep => ep.ProjectId == 2)
                .ToList();

            foreach (var p in projectMapForDelete)
            {
                context.Remove(p);
            }

            var projectForDelete = context.Projects.Find(2);

            context.Projects.Remove(projectForDelete);

            var projects = context.Projects
                .Take(10)
                .ToList();


            var sb = new StringBuilder();

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Name}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 15

        public static string RemoveTown(SoftUniContext context)
        {
            var employeeForNullAdresss = context.Employees.Where(e => e.Address.Town.Name == "Seattle").ToList();

            foreach (var e in employeeForNullAdresss)
            {
                var forNull = context.Employees.FirstOrDefault(x => x.EmployeeId == e.EmployeeId);
                forNull.AddressId = null;
            }

            var addressesForDelete = context.Addresses.Where(a => a.Town.Name == "Seattle").ToList();

            int deletedAdresses = addressesForDelete.Count;

            foreach (var ad in addressesForDelete)
            {
                context.Addresses.Remove(ad);
            }

            var townToRemove = context.Towns.FirstOrDefault(t => t.Name == "Seattle");

            context.Towns.Remove(townToRemove);

            context.SaveChanges();

            return $"{deletedAdresses} addresses in Seattle were deleted";
        }

    }
}
