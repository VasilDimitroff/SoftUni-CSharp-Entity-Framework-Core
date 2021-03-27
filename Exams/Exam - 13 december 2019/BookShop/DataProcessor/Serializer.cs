namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors.Select(au => new ExportAuthorDto
            {
                AuthorName = au.FirstName + " " + au.LastName,
                Books = au.AuthorsBooks.OrderByDescending(a => a.Book.Price).Select(ab => new ExportBookDto
                {
                    BookName = ab.Book.Name,
                    BookPrice = ab.Book.Price.Value.ToString("f2")
                })
                .ToList()
            })
                .ToList()
                .OrderByDescending(author => author.Books.Count)
                .ThenBy(author => author.AuthorName);
                

           var json = JsonConvert.SerializeObject(authors, Formatting.Indented);

            return json;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var oldestBooks = context.Books
                .Where(book => book.PublishedOn < date && book.Genre == Enum.Parse<Genre>("Science"))
                .OrderByDescending(book => book.Pages)
                .ThenByDescending(book => book.PublishedOn)
                .Take(10)
                .Select(book => new ExportOldestBookDto
                {
                    Pages = book.Pages.ToString(),
                    Name = book.Name,
                    Date = book.PublishedOn.ToString("MM/dd/yyyy")
                })
                .ToArray();

            var sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var serializer = new XmlSerializer(typeof(ExportOldestBookDto[]), new XmlRootAttribute("Books"));
            serializer.Serialize(new StringWriter(sb), oldestBooks, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}