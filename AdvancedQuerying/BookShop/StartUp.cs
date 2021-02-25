namespace BookShop
{
    using Data;
    using Initializer;
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
            // DbInitializer.ResetDatabase(db);

            string input = Console.ReadLine().ToLower().Trim();
            //int input = int.Parse(Console.ReadLine());

            string result = GetBooksReleasedBefore(db, input);
            Console.WriteLine(result);

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

            //!!!!
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
            //Return the title, edition type and price of all books that are released before a given date.
            //The date will be a string in format dd-MM-yyyy.
            //Return all of the rows in a single string, ordered by release date descending.

          //  DateTime parsedDate = DateTime.Parse(date, CultureInfo.InvariantCulture, format);


            string dateString = "Mon 16 Jun 8:30 AM 2008";
            string format = "yy-MM-dd";

            DateTime parsedDate = DateTime.ParseExact(date, format,
                CultureInfo.InvariantCulture);






            var books = context.Books.Where(b => b.ReleaseDate.Value < parsedDate)
                .Select( b=> new 
                { 
                    b.Title,
                    EditionType = b.EditionType.ToString(),
                    b.Price,
                    b.ReleaseDate
                })
                .OrderByDescending(b => b.ReleaseDate)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
