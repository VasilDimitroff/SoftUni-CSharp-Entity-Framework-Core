using ProductShop.Data;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var db = new ProductShopContext();

            var xml = File.ReadAllText("../../../Datasets/categories.xml");

            var result = ImportCategories(db, xml);

            Console.WriteLine(result);
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportUsersDto[]), new XmlRootAttribute("Users"));

            var userDtos =
              (ImportUsersDto[])serializer.Deserialize(new StringReader(inputXml));

            var users = userDtos.Select(user => new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age
            })
                .ToArray();

            context.Users.AddRange(users);

            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportProductDto[]), new XmlRootAttribute("Products"));

            var productDtos =
              (ImportProductDto[])serializer.Deserialize(new StringReader(inputXml));

            var products = productDtos.Select(productDto => new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                SellerId = productDto.SellerId, 
                BuyerId = productDto.BuyerId
            })
                .ToArray();

            context.Products.AddRange(products);

            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportCategoryDto[]), new XmlRootAttribute("Categories"));

            var categoryDtos =
              (ImportCategoryDto[])serializer.Deserialize(new StringReader(inputXml));

            var categories = new List<Category>();

            foreach (var categoryDto in categoryDtos)
            {
                var category = new Category();

                if (categoryDto.Name == null)
                {
                    continue;
                }

                category.Name = categoryDto.Name;

                categories.Add(category);
            }

            context.Categories.AddRange(categories);

            context.SaveChanges();

            return $"Successfully imported {categories.Count}";
        }
    }
}