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
            var serializer = new XmlSerializer(typeof(ImportBookModel[]), new XmlRootAttribute("Books"));
            var booksModels = (ImportBookModel[])serializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();

            foreach (var bookModel in booksModels)
            {
                if (!IsValid(bookModel))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var book = new Book()
                {
                    Name = bookModel.Name,
                    Price = decimal.Parse(bookModel.Price.ToString("f2")),
                    Pages = bookModel.Pages
                };

                DateTime publishedOn;

                bool isPublishedOnValid = DateTime.TryParseExact(bookModel.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out publishedOn);
            
                Genre genre;
                bool isGenreParsed = Enum.TryParse<Genre>(bookModel.Genre, out genre);

                if (!isGenreParsed || !isPublishedOnValid || (bookModel.Genre != "1" && bookModel.Genre != "2" && bookModel.Genre != "3"))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }
                
                book.PublishedOn = publishedOn;
                book.Genre = genre;

                context.Books.Add(book);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported book {book.Name} for {book.Price}.");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authorsDto = JsonConvert.DeserializeObject<ImportAuthorModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var authorDto in authorsDto)
            {
                if (!IsValid(authorDto))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                var authorEmail = context.Authors.FirstOrDefault(x => x.Email == authorDto.Email);

                if (authorEmail != null)
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

                foreach (var bookDto in authorDto.Books)
                {
                    if (!IsValid(bookDto))
                    {
                        sb.AppendLine("Invalid data!");
                        continue;
                    }

                    var currentBook = context.Books.FirstOrDefault(x => x.Id == bookDto.Id);

                    if (currentBook == null)
                    {
                        continue;
                    }

                    var authorBook = new AuthorBook()
                    {
                        Author = author,
                        Book = currentBook
                    };

                    author.AuthorsBooks.Add(authorBook);
                }

                if (author.AuthorsBooks.Count < 1)
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }

                context.Authors.Add(author);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported author - {author.FirstName + " " + author.LastName} with {author.AuthorsBooks.Count} books.");
            }


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