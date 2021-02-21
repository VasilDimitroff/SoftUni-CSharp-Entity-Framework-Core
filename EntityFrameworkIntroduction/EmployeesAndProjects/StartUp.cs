using EmployeesAndProjects.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EmployeesAndProjects.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeesAndProjects
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            using var db = new SoftUniContext();
           Console.WriteLine(TEST(db));
        }

        //Problem 03

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
               .OrderBy(x => x.Id)
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

        //Problem 04

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees.Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName)
                .ToList();

            foreach (var employee in employees)
            {
                var currentEmployee = $"{employee.FirstName} - {employee.Salary:f2}";
                sb.AppendLine(currentEmployee);
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 05

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

        //Problem 06

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4,
                Town = context.Towns.First(t => t.TownId == 4),
            };

            var targetEmployee = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");

            targetEmployee.Address = address;

            context.SaveChanges();

            var employees = context.Employees.Select(x => new
            {
                AddressId = x.AddressId,
                AddressText = x.Address.AddressText

            }).
            OrderByDescending(x => x.AddressId)
            .Take(10)
            .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine(employee.AddressText);
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 07
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            DateTime startDate = new DateTime(2001, 01, 01);
            DateTime endDate = new DateTime(2003, 12, 31);


            var employees = context.Employees.
                Where(x => x.EmployeesProjects.Any(p => p.Project.StartDate.Year >= 2001 && p.Project.StartDate.Year <= 2003))
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects.Select(p => new
                    {
                        Name = p.Project.Name,
                        StartDate = p.Project.StartDate,
                        EndDate = p.Project.EndDate
                    })
                    .ToList()
                })
                .Take(10)
                .ToList();


            StringBuilder sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

                foreach (var p in e.Projects)
                {
                    var pStartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                    var pEndDate =
                      p.EndDate?.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture) == null ? "not finished"
                      : p.EndDate?.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                    sb.AppendLine($"--{p.Name} - {pStartDate} - {pEndDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 08

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses.OrderByDescending(a => a.Employees.Count)
                .ThenBy(a => a.Town.Name)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .Select(a => new
                {
                    AddressText = a.AddressText,
                    TownName = a.Town.Name,
                    EmployeesCount = a.Employees.Count
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var a in addresses)
            {
                sb.
                    AppendLine($"{a.AddressText}, {a.TownName} - {a.EmployeesCount} employees");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 09

        public static string GetEmployee147(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employee147 = context.Employees.Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    JobTitle = e.JobTitle,
                    Projects = e.EmployeesProjects.Select(p => new
                    {
                        Name = p.Project.Name
                    })
                    .OrderBy(p => p.Name)
                    .ToList()
                })
                .ToList();

            foreach (var e in employee147)
            {
                sb.
                    AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");

                foreach (var p in e.Projects)
                {
                    sb.
                        AppendLine($"{p.Name}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 10

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d => d.Employees.Count)
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    Name = d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
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

            foreach (var dep in departments)
            {
                sb
                    .AppendLine($"{dep.Name} - {dep.ManagerFirstName} {dep.ManagerLastName}");

                foreach (var em in dep.Employees)
                {
                    sb.AppendLine($"{em.FirstName} {em.LastName} - {em.JobTitle}");
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
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt")
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var project in projects)
            {
                sb.
                    AppendLine($"{project.Name}");
                sb.
                    AppendLine($"{project.Description}");
                sb.
                    AppendLine($"{project.StartDate}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 12

        public static string IncreaseSalaries(SoftUniContext context)
        {

            IQueryable<Employee> employees = context.Employees
                .Where(d => d.Department.Name == "Engineering"
                || d.Department.Name == "Tool Design"
                || d.Department.Name == "Marketing"
                || d.Department.Name == "Information Services"
                );
           

            foreach (Employee emp in employees)
            {
                 emp.Salary *= 1.12m;
            }

            context.SaveChanges();

            var changedEmployees = employees.Select(d => new
            {
                FirstName = d.FirstName,
                LastName = d.LastName,
                Salary = d.Salary * 1.12m
            })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();


            context.SaveChanges();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in changedEmployees)
            {
                sb.
                    AppendLine($"{emp.FirstName} {emp.LastName} (${emp.Salary:f2})");
            }

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
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.
                    AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle} - (${emp.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 14

        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectsToDelete = context.Projects.Where(p => p.ProjectId == 2).ToList();

            var projectsToDelFromMappingTable = context.EmployeesProjects
                .Where(ep => projectsToDelete.Any(p => p.ProjectId == ep.ProjectId));

            //context.EmployeesProjects.RemoveRange(projectsToDelFromMappingTable);

            foreach (var project in projectsToDelFromMappingTable)
            {
                context.EmployeesProjects.Remove(project);
            }

            foreach (var project in projectsToDelete)
            {
                context.Projects.Remove(project);
            }

            context.SaveChanges();

            var projects = context.Projects.Select(p => new
            {
                p.Name
            })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var proj in projects)
            {
                sb.AppendLine($"{proj.Name}");
            }
    
            return sb.ToString().TrimEnd();
        }

        //Problem 15

        public static string RemoveTown(SoftUniContext context)
        {
            var townToDelete = context.Towns.FirstOrDefault(t => t.Name == "Seattle");    

            var addressesToDelete = context.Addresses
                .Where(a => a.TownId == townToDelete.TownId);

            int addressesToDelCount = context.Addresses
                .Where(a => a.TownId == townToDelete.TownId)
                .Count();

            var employeesWithAddressToDelete = context.Employees
                .Where(e => addressesToDelete.Any(ad => ad.AddressId == e.AddressId));

            foreach (var empl in employeesWithAddressToDelete)
            {
                empl.AddressId = null;
            }

            foreach (var address in addressesToDelete)
            {
                context.Addresses.Remove(address);
            }

            context.Towns.Remove(townToDelete);

            context.SaveChanges();

            return $"{addressesToDelCount} addresses in {townToDelete.Name} were deleted";
        }

        public static string TEST(SoftUniContext context)
        {
            Employee employee = context.Employees.FirstOrDefault(x=> x.JobTitle =="Tool Designer");

            string name = employee.FirstName + " " + employee.LastName;

            

            return name;
        }


    }
}
