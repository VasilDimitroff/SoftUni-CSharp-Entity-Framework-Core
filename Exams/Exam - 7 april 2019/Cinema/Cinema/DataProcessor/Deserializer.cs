namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.Data.Models;
    using Cinema.Data.Models.Enums;
    using Cinema.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie 
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat 
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection 
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket 
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var moviesDto = JsonConvert.DeserializeObject<ImportMovieDto[]>(jsonString);

            var sb = new StringBuilder();
            var movies = new List<Movie>();

            foreach (var movieDto in moviesDto)
            {
                var title = context.Movies.FirstOrDefault(m => m.Title == movieDto.Title);

                if (!IsValid(movieDto) || title != null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Genre genre;
                bool isGenreParsed = Enum.TryParse<Genre>(movieDto.Genre, out genre);

                TimeSpan duration;
                bool isDurationParsed = TimeSpan.TryParse(movieDto.Duration, out duration);

                if (!isGenreParsed || !isDurationParsed)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var movie = new Movie()
                {
                    Title = movieDto.Title,
                    Genre = genre,
                    Duration = duration,
                    Rating = movieDto.Rating,
                    Director = movieDto.Director
                };

                movies.Add(movie);
                sb.AppendLine($"Successfully imported {movie.Title} with genre {movie.Genre} and rating {movie.Rating.ToString("f2")}!");
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var hallsDto = JsonConvert.DeserializeObject<ImportHallWithSeatsDto[]>(jsonString);

            var sb = new StringBuilder();

            var halls = new List<Hall>();

            foreach (var hallDto in hallsDto)
            {
                if (!IsValid(hallDto) || hallDto.Seats < 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var hall = new Hall()
                {
                    Name = hallDto.Name,
                    Is4Dx = hallDto.Is4Dx,
                    Is3D = hallDto.Is3D,
                };

                var seats = new List<Seat>();

                for (int i = 0; i < hallDto.Seats; i++)
                {
                    var seat = new Seat()
                    {
                        Hall = hall
                    };

                    seats.Add(seat);
                    
                }

                halls.Add(hall);
                context.Seats.AddRange(seats);

                var projectionType = string.Empty;

                if (hall.Is3D == true && hall.Is4Dx == true)
                {
                    projectionType = "4Dx/3D";
                }

                else if (hall.Is3D == true)
                {
                    projectionType = "3D";
                }

                else if (hall.Is4Dx == true)
                {
                    projectionType = "4Dx";
                }

                else
                {
                    projectionType = "Normal";
                }

                sb.AppendLine($"Successfully imported {hall.Name}({projectionType}) with {hallDto.Seats} seats!");
            }

            context.Halls.AddRange(halls);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportProjectionDto[]), new XmlRootAttribute("Projections"));
            var projectionsDto = (ImportProjectionDto[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();

            var projections = new List<Projection>();

            foreach (var projDto in projectionsDto)
            {

                var movie = context.Movies.FirstOrDefault(m => m.Id == projDto.MovieId);
                var hall = context.Halls.FirstOrDefault(m => m.Id == projDto.HallId);

                if (!IsValid(projDto) || movie == null || hall == null)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                DateTime date;

                bool isDateParsed = 
                    DateTime.TryParseExact(projDto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
             
                if (!isDateParsed)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
              
                }

                string dateAsStr = date.ToString("MM/dd/yyyy");

                var projection = new Projection
                {
                    MovieId = projDto.MovieId,
                    HallId = projDto.HallId,
                    DateTime = DateTime.ParseExact(dateAsStr, "MM/dd/yyyy", CultureInfo.InvariantCulture)
                };

                projections.Add(projection);

                //Projection moviePorj = context.Projections.FirstOrDefault(p => p.MovieId == projection.MovieId);
               // var movieTitle = moviePorj.Movie.Title;

                sb.AppendLine($"Successfully imported projection {movie.Title} on {projection.DateTime.ToString("MM/dd/yyyy")}!");
            }

            context.Projections.AddRange(projections);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportCustomersDto[]), new XmlRootAttribute("Customers"));

            var customersDto = (ImportCustomersDto[])serializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();

            var customers = new List<Customer>();

            foreach (var custDto in customersDto)
            {
                if (!IsValid(custDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var customer = new Customer
                {
                    FirstName = custDto.FirstName,
                    LastName = custDto.LastName,
                    Age = custDto.Age
                };

                foreach (var ticketDto in custDto.Tickets)
                {
                    var targetProjection = context.Projections.FirstOrDefault(x => x.Id == ticketDto.ProjectionId);

                    if (!IsValid(ticketDto) || targetProjection == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var ticket = new Ticket
                    {
                        ProjectionId = ticketDto.ProjectionId,
                        Price = ticketDto.Price,
                    };

                    customer.Tickets.Add(ticket);
                }

                customers.Add(customer);
                sb.AppendLine($"Successfully imported customer {customer.FirstName} {customer.LastName} with bought tickets: {customer.Tickets.Count}!");
            }

            context.Customers.AddRange(customers);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}