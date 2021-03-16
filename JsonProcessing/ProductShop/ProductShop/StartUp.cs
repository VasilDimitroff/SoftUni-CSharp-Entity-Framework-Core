using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            ProductShopContext context = new ProductShopContext();

            //context.Database.EnsureCreated();

           //string json = File.ReadAllText("../../../Datasets/categories-products.json");
            var result = GetUsersWithProducts(context);

            Console.WriteLine(result);

        }

        //Import data

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
           User[] users = JsonConvert.DeserializeObject<User[]>(inputJson);

            context.Users.AddRange(users);

            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            Category[] categories = JsonConvert.DeserializeObject<Category[]>(inputJson);
            var categoriesToAdd = new List<Category>();

            foreach (var cat in categories)
            {
                if (cat.Name == null)
                {
                    continue;
                }

                categoriesToAdd.Add(cat);
            }

            context.Categories.AddRange(categoriesToAdd);
            context.SaveChanges();

            return $"Successfully imported {categoriesToAdd.Count}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoriesProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(categoriesProducts);

            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Length}";
        }

        //Querying
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(product => product.Price >= 500 && product.Price <= 1000)
                .Select(product => new
                {
                    name = product.Name,
                    price = product.Price,
                    seller = product.Seller.FirstName + " " + product.Seller.LastName,
                })
                .OrderBy(product => product.price)
                .ToArray();

            var json = JsonConvert.SerializeObject(products, Formatting.Indented);

            return json;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(user => user.ProductsSold.Any(p => p.Buyer != null))
                .Select(user => new
                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    soldProducts = user.ProductsSold
                        .Where(p => p.Buyer != null)
                        .Select(prod => new
                            {
                                name = prod.Name,
                                price = prod.Price,
                                buyerFirstName = prod.Buyer.FirstName,
                                buyerLastName = prod.Buyer.LastName
                            })
                })
                .OrderBy(user => user.lastName)
                .ThenBy(user => user.firstName)
                .ToList();

            var json = JsonConvert.SerializeObject(users, Formatting.Indented);

            return json;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(cat => cat.CategoryProducts.Count())
                .Select(category => new
                {
                    category = category.Name,
                    productsCount = category.CategoryProducts.Count(),
                    averagePrice = category.CategoryProducts.Average(cp => cp.Product.Price).ToString("f2"),
                    totalRevenue = category.CategoryProducts.Sum(cp => cp.Product.Price).ToString("f2")
                })
                .ToList();

            var json = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return json;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .Include(c => c.ProductsSold)
                .OrderByDescending(u => u.ProductsSold.Count(p => p.Buyer != null))
                .ToList()
                .Select(u => new
                {
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Count(p => p.Buyer != null),
                        products = u.ProductsSold
                            .ToList()
                        .Where(p => p.Buyer != null)
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                            .ToList()
                    }
                })
                .ToList();
                
            var resultObj = new 
            {
                usersCount = users.Count,
                users = users
            };

            var result = JsonConvert.SerializeObject(resultObj, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });

            return result;
        }
    }
}