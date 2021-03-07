namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            // DbInitializer.ResetDatabase(db);

            //var input = Console.ReadLine();
            //var input = int.Parse(Console.ReadLine());

            // var result = GetMostRecentBooks(db);
            // Console.WriteLine(result);

            RemoveBooks(db);
        }

        //Problem 01
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var books = context.Books
                .ToList()
                .Where(book => (book.AgeRestriction).ToString().ToLower() == command.ToLower())
                .Select(book => book.Title)
                .OrderBy(book => book);
              

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 02
        public static string GetGoldenBooks(BookShopContext context)
        {
            EditionType parsed;
            Enum.TryParse<EditionType>("Gold", out parsed);
            var books = context.Books
                .Where(book => book.Copies < 5000 && book.EditionType == parsed)
                .OrderBy(book => book.BookId)
               .ToList();

            return string.Join(Environment.NewLine, books.Select(book => book.Title));
        }

        //Problem 03
        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books.Where(book => book.Price > 40)
                .OrderByDescending(book => book.Price)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(book => $"{book.Title} - ${book.Price:f2}"));
        }

        //Problem 04
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books.Where(book => book.ReleaseDate.Value.Year != year)
                .OrderBy(book => book.BookId)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(book => book.Title));
        }

        //Problem 05 
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(c => c.ToLower());

            var books = context.Books
                .Where(book => book.BookCategories.Any(bc => categories.Contains(bc.Category.Name.ToLower())))
                .OrderBy(book => book.Title)
                .ToList();
                

            return string.Join(Environment.NewLine, books.Select(book => $"{book.Title}"));
        }

        //Problem 06
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books.Where(book => book.ReleaseDate < parsedDate)
                .Select(book => new
                {
                    book.Title,
                    book.EditionType,
                    book.Price,
                    book.ReleaseDate
                })
                .OrderByDescending(book => book.ReleaseDate)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(book => $"{book.Title} - {book.EditionType} - ${book.Price:f2}"));

        }

        //Problem 07 
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors.Where(au => au.FirstName.ToLower().EndsWith(input.ToLower()))
                .Select(au => new
                {
                    au.FirstName,
                    au.LastName
                })
                .OrderBy(au => au.FirstName)
                .ThenBy(au => au.LastName)
                .ToList();

            return string.Join(Environment.NewLine, authors.Select(au => au.FirstName + " " + au.LastName));
        }

        //Problem 08
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(book => book.Title.ToLower().Contains(input.ToLower()))
                .OrderBy(book => book.Title)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(book => book.Title));
        }

        //Problem 09
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books.Where(book => book.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .Select(book => new
                {
                    book.BookId,
                    book.Title,
                    Author = book.Author.FirstName + " " + book.Author.LastName
                })
                .OrderBy(book => book.BookId)
                .ToList();

                return string.Join(Environment.NewLine, books.Select(book => $"{book.Title} ({book.Author})"));
        }

        //Problem 10
        public static int CountBooks(BookShopContext context, int lengthCheck)   
             =>   context.Books.Where(book => book.Title.Length > lengthCheck).Count();

        //Problem 11
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(author => new
                    {
                        FullName = author.FirstName + " " + author.LastName,
                        TotalCopies = author.Books.Sum(book => book.Copies)
                    })
                .OrderByDescending(author => author.TotalCopies)
                .ToList();

            return string.Join(Environment.NewLine, authors
                .Select(author => $"{author.FullName} - {author.TotalCopies}"));
        }

        //Problem 12 
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select(cat => new
                {
                    cat.Name,
                    TotalProfit = cat.CategoryBooks
                    .Select(bookCat => bookCat.Book.Copies * bookCat.Book.Price).Sum()
                })
                .OrderByDescending(book => book.TotalProfit)
                .ThenBy(book => book.Name)
                .ToList();

            return string.Join(Environment.NewLine, categories.Select(cat => $"{cat.Name} ${cat.TotalProfit:f2}"));
        }

        //Problem 13
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var cats = context.Categories
                .Select(cat => new
                {
                    cat.Name,
                    Books = cat.CategoryBooks.OrderByDescending(cb => cb.Book.ReleaseDate.Value)
                    .Take(3)
                    .Select(b => new
                    {
                        Name = b.Book.Title,
                        Year = b.Book.ReleaseDate.Value.Year
                    })
                })
                .OrderBy(b => b.Name)
                .ToList();

            var sb = new StringBuilder();

            foreach (var cat in cats)
            {
                sb.AppendLine($"--{cat.Name}");

                foreach (var book in cat.Books)
                {
                    sb.AppendLine($"{book.Name} ({book.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 14
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books.Where(book => book.ReleaseDate.Value.Year < 2010).ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        //Problem 15
        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books.Where(book => book.Copies < 4200).ToList();

            context.Books.RemoveRange(books);

            int count = books.Count();

            context.SaveChanges();

            return count;
        }
    }
}
