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

            var xml = File.ReadAllText("../../../Datasets/categories-products.xml");

            var result = ImportCategoryProducts(db, xml);

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

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportCategoryProductDto[]), new XmlRootAttribute("CategoryProducts"));

            var productCategoriesDtos =
              (ImportCategoryProductDto[])serializer.Deserialize(new StringReader(inputXml));

            var categoryProducts = new List<CategoryProduct>(); 

            foreach (var productCategoryDto in productCategoriesDtos)
            {
                
                Product product = context.Products.FirstOrDefault(x => x.Id == productCategoryDto.ProductId);
                Category category = context.Categories.FirstOrDefault(x => x.Id == productCategoryDto.CategoryId);

                if (product == null || category == null)
                {
                    continue;
                }

                var productCategory = new CategoryProduct
                {
                    CategoryId = productCategoryDto.CategoryId,
                    ProductId = productCategoryDto.ProductId
                };

                categoryProducts.Add(productCategory);
            }

            context.CategoryProducts.AddRange(categoryProducts);

            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Count}";
        }

    }
}