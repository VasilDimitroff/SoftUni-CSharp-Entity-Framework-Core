namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportBookDto[]), new XmlRootAttribute("Books"));
            var booksDto = (ImportBookDto[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();
            var books = new List<Book>();

            foreach (var bookDto in booksDto)
            {
                if (!IsValid(bookDto))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }
               
                Genre genreValue;
                bool isGenreValid = Enum.TryParse<Genre>(Enum.GetName(typeof(Genre), bookDto.GenreInt), out genreValue);

                if (!isGenreValid)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                DateTime date = DateTime.ParseExact(bookDto.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                var book = new Book()
                {
                    Name = bookDto.Name,
                    Genre = genreValue,
                    Price = bookDto.Price,
                    Pages = bookDto.Pages,
                    PublishedOn = date
                };

                books.Add(book);
                sb.AppendLine($"Successfully imported book {book.Name} for {book.Price.Value.ToString("f2")}.");
            }

            context.Books.AddRange(books);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authorsDto = JsonConvert.DeserializeObject<ImportAuthorDto[]>(jsonString);
            var sb = new StringBuilder();
            var authors = new List<Author>();
            int counter = 0;

            foreach (var authorDto in authorsDto)
            {
                if (!IsValid(authorDto))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var email = context.Authors.FirstOrDefault(x => x.Email == authorDto.Email);

                if (email != null)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var author = new Author()
                {
                    FirstName = authorDto.FirstName,
                    LastName = authorDto.LastName,
                    Phone = authorDto.Phone,
                    Email = authorDto.Email,
                };

                var authorBooks = new List<AuthorBook>();

                foreach (var bookDto in authorDto.Books)
                {
                    var book = context.Books.FirstOrDefault(b => b.Id == bookDto.Id);

                    if (book == null)
                    {
                        continue;
                    }

                    var currentBook = new AuthorBook()
                    {
                        BookId = bookDto.Id.Value,
                        Author = author
                    };

                    authorBooks.Add(currentBook);
                    context.AuthorsBooks.Add(currentBook);
                    counter++;
                }

                if (authorBooks.Count < 1)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                foreach (var autBook in authorBooks)
                {
                    author.AuthorsBooks.Add(autBook);
                }

                authors.Add(author);
                sb.AppendLine($"Successfully imported author - {author.FirstName + " " + author.LastName} with {author.AuthorsBooks.Count} books.");
            }
            //sb.AppendLine($"count {counter}");
            context.Authors.AddRange(authors);
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