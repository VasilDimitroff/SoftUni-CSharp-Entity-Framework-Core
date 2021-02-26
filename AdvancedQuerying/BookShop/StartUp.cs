namespace BookShop
{
    using BookShop.Models;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            //string input = Console.ReadLine();
            //int input = int.Parse(Console.ReadLine());

            //Console.WriteLine(result);

            Console.WriteLine(RemoveBooks(db));
            

        }

        //Problem 02
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            StringBuilder sb = new StringBuilder();

            string parameter = string.Empty;

            if (command == "minor")
            {
                parameter = "0";
            }

            else if (command == "teen")
            {
                parameter = "1";
            }

            else if (command == "adult")
            {
                parameter = "2";
            }

            var books = context.Books
                .Where(b => ((int)b.AgeRestriction).ToString() == parameter)
                .Select(b => new
                {
                    b.Title
                })
                .OrderBy(b => b.Title)
                .ToList();


            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title}");
            }
           
            return sb.ToString().TrimEnd();
        }

        //Problem 03
        public static string GetGoldenBooks(BookShopContext context)
        {
            var sb = new StringBuilder();

            var books = context.Books
                .Where(b => (((int)b.EditionType).ToString() == "2") && b.Copies < 5000)
                .Select(b => new
                {
                    b.BookId,
                    b.Title,
                })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title}");
            }

            return sb.ToString().TrimEnd();

        }

        //Problem 04

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Price > 40m)
                .Select(b => new
                {
                    b.Title,
                    b.Price,
                })
                .OrderByDescending(b => b.Price)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 05
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .Select(b => new
                {
                    b.BookId,
                    b.Title,
                })
                .OrderBy(b => b.BookId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 06
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] categoryInfo = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var books = new List<String>();

            for (int i = 0; i < categoryInfo.Length; i++)
            {
                categoryInfo[i] = categoryInfo[i].ToLower();
            }

            foreach (var categ in categoryInfo)
            {
                List<string> currentCat = context.Books
                    .Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == categ))
                    .Select( c => c.Title)
                    .ToList();

                books.AddRange(currentCat);
            }

            books = books.OrderBy(b => b).ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 07
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books.Where(b => b.ReleaseDate < parsedDate)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    EditionType = b.EditionType.ToString(),
                    b.Price,
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");

            }

            return sb.ToString().TrimEnd();
        }

        //Problem 08
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    FullName = a.FirstName + " " + a.LastName
                })
                .OrderBy(a => a.FullName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var author in authors)
            {
                sb.AppendLine($"{author.FullName}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 09
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
          
            var books = context.Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 10
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .Select(b => new
                {
                    b.Title,
                    b.BookId,
                    FullName = b.Author.FirstName + " " + b.Author.LastName,
                })
                .OrderBy(b => b.BookId);

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} ({book.FullName})");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 11
        public static int CountBooks(BookShopContext context, int lengthCheck)
            => context.Books.Count(b => b.Title.Length > lengthCheck);

        //Problem 12

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors.Select(a => new
            {
                FullName = a.FirstName + " " + a.LastName,
                TotalCopies = a.Books.Sum(b => b.Copies)
            })
               .OrderByDescending(x => x.TotalCopies)
               .ToList();

            var sb = new StringBuilder();

            foreach (var author in authors)
            {
                sb.AppendLine($"{author.FullName} - {author.TotalCopies}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 13
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select( x=> new
                    {
                        Profit = x.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price),
                        Name = x.Name
                    })
               .OrderByDescending(x => x.Profit)
               .ThenBy(x => x.Name)
               .ToList();

            var sb = new StringBuilder();

            foreach (var category in categories)
            {
                sb.AppendLine($"{category.Name} ${category.Profit:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 14
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                   Name = c.Name,
                   Books = c.CategoryBooks
                       .Select(b => new Book
                       {
                           ReleaseDate = b.Book.ReleaseDate,
                           Title = b.Book.Title,
                       })
                           .OrderByDescending(x => x.ReleaseDate)
                           .Take(3)
                           .ToList()
                })
                .OrderBy(a => a.Name)
                .ToList();


            var sb = new StringBuilder();

            foreach (var category in categories)
            {
                sb.AppendLine($"--{category.Name}");

                foreach (var book in category.Books)
                { 
                    sb.AppendLine($"{book.Title} ({book.ReleaseDate.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 15
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books.Where(b => b.ReleaseDate.Value.Year < 2010).ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        //Problem 16
        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books.Where(b => b.Copies < 4200).ToList();
            int count = books.Count;

            context.Books.RemoveRange(books);

            context.SaveChanges();

            return count;
        }

    }  
}
