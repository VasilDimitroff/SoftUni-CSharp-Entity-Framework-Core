namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.Data.Models;
    using Cinema.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var movies = context.Movies
                .Where(m => m.Rating >= rating && m.Projections.Any(p => p.Tickets.Count > 0))
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(movie => movie.Projections.Sum(p => p.Tickets.Sum(t => t.Price)))
                .Take(10)
                .Select(movie => new
                {
                    MovieName = movie.Title,
                    Rating = movie.Rating.ToString("f2"),
                    TotalIncomes = movie.Projections.Sum(p => p.Tickets.Sum(t => t.Price)).ToString("f2"),
                    Customers = movie.Projections   
                    .SelectMany(p => p.Tickets).Select(t => new
                    {
                        FirstName = t.Customer.FirstName,
                        LastName = t.Customer.LastName,
                        Balance = t.Customer.Balance.ToString("f2")
                    })
                    .OrderByDescending(c => c.Balance)
                     .ThenBy(c => c.FirstName)
                     .ThenBy(c => c.LastName)
                    .ToList()
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(movies, Formatting.Indented);

            return json;
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            var topCustomers = context.Customers.Where(c => c.Age >= age)
                .Select(c => new ExportCustomerDto
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    SpentTime = TimeSpan.FromMilliseconds(c.Tickets.Sum(t => t.Projection.Movie.Duration.TotalMilliseconds)).ToString(@"hh\:mm\:ss"),
                    SpentMoney = decimal.Parse(c.Tickets.Sum(t => t.Price).ToString("f2"))
                })
            
            .OrderByDescending(x => x.SpentMoney)
            .Take(10)
            .ToList();

            StringBuilder sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ExportCustomerDto>), new XmlRootAttribute("Customers"));
           
            using (StringWriter writer = new StringWriter(sb))
            {
                xmlSerializer.Serialize(writer, topCustomers, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}