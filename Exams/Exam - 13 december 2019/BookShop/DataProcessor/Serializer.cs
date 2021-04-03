namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors.OrderByDescending(au => au.AuthorsBooks.Count())
                .ThenBy(x => x.FirstName)
                .Select(author => new
                {
                    AuthorName = author.FirstName + " " + author.LastName,
                    Books = author.AuthorsBooks
                    .OrderByDescending(book => book.Book.Price)
                    .Select(ab => new
                    {
                        BookName = ab.Book.Name,
                        BookPrice = ab.Book.Price.ToString("f2")
                    })
                    .ToList()
                })           
                .ToList();

            var json = JsonConvert.SerializeObject(authors, Formatting.Indented);

            return json;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var books = context.Books
                .Where(book => book.PublishedOn < date && book.Genre.ToString() == "Science")
                .OrderByDescending(book => book.Pages)
                .ThenByDescending(book => book.PublishedOn)
                .Select(book => new ExportBookModel
                {
                    Pages = book.Pages,
                    Name = book.Name,
                    Date = book.PublishedOn.ToString("MM/dd/yyyy")
                })
                .Take(10)
                .ToArray();
            
            StringBuilder sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportBookModel[]), new XmlRootAttribute("Books"));
            using (StringWriter writer = new StringWriter(sb))
            {
                xmlSerializer.Serialize(writer, books, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}