using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {   
            var context = new ProductShopContext();

            context.Database.EnsureCreated();     

           //string json = File.ReadAllText("../../../Datasets/categories-products.json");

           var result = GetUsersWithProducts(context);
           Console.WriteLine(result);
        }

        //Problem 1
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            User[] users = JsonConvert.DeserializeObject<User[]>(inputJson);

            context.Users.AddRange(users);

            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        //Problem 2
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            Product[] products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            context.Products.AddRange(products);

            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Problem 3
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            Category[] categories = JsonConvert.DeserializeObject<Category[]>(inputJson)
                .Where(x => x.Name != null)
                .ToArray();

            context.Categories.AddRange(categories);

            context.SaveChanges();

            return $"Successfully imported {categories.Length}";

        }

        //Problem 4
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            CategoryProduct[] categoriesProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(categoriesProducts);

            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Length}";

        }

        //Problem 5
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(product => product.Price >= 500 && product.Price <= 1000)
                .Select(product => new
                {
                    name = product.Name,
                    price = product.Price,
                    seller  = product.Seller.FirstName + " " + product.Seller.LastName,
                })
                .OrderBy(product => product.price)
                .ToArray();

            string result = JsonConvert.SerializeObject(products, Formatting.Indented);

            return result;
        }

        //Problem 6
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(user => user.ProductsSold.Any(product => product.Buyer != null))
                .Select(user => new
                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    soldProducts = user.ProductsSold
                    .Where(product => product.Buyer != null)
                    .Select(product => new
                    {
                        name = product.Name,
                        price = product.Price,
                        buyerFirstName = product.Buyer.FirstName,
                        buyerLastName = product.Buyer.LastName,
                    })
                    .ToArray()
                })
                .OrderBy(user => user.lastName)
                .ThenBy(user=> user.firstName)
                .ToList();

            var result = JsonConvert.SerializeObject(users, Formatting.Indented);

            return result;
        }

        //Problem 7
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(category => category.CategoryProducts.Count())
                .Select(category => new
                {
                    category = category.Name,
                    productsCount = category.CategoryProducts.Count(),
                    averagePrice = category.CategoryProducts.Average(product => product.Product.Price).ToString("F2"),
                    totalRevenue = category.CategoryProducts.Sum(product => product.Product.Price).ToString("F2"),
                })
                .ToList();

            var result = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return result;
        }

        //Problem 8
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users.Include(p => p.ProductsSold)
                .ToList()
                .Where(user => user.ProductsSold.Any(product => product.Buyer != null))
                .OrderByDescending(u => u.ProductsSold.Count(p => p.Buyer != null))
                .Select(user => new
                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    age = user.Age,
                    soldProducts = user.ProductsSold.Where(p => p.Buyer != null).Select(product => new
                    {
                        count = user.ProductsSold.Count(),
                        products = user.ProductsSold.Select(p => new
                        {
                            name = p.Name,
                            price = p.Price,
                        })
                        .ToList()
                    })
                })
                .ToList();
                
                

            var userWithCountProducts = new
            {
                usersCount = users.Count,
                users = users
            };

            string result = JsonConvert.SerializeObject(userWithCountProducts, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            return result;
        }
    }
}