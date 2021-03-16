using Microsoft.EntityFrameworkCore;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var db = new ProductShopContext();

            //var xml = File.ReadAllText("../../../Datasets/categories-products.xml");

            var result = GetUsersWithProducts(db);

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

        public static string GetProductsInRange(ProductShopContext context)
        {
            var productsInRangeDto = context.Products
                .Where(product => product.Price >= 500 && product.Price <= 1000)
                .OrderBy(product => product.Price)
                .Take(10)
                .Select(product => new ExportProductsInRangeDto
                {
                    Name = product.Name,
                    Price = product.Price,
                    BuyerFullName = product.Buyer.FirstName + " " + product.Buyer.LastName,
                })
                .ToArray();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportProductsInRangeDto[]), new XmlRootAttribute("Products"));
   
            serializer.Serialize(new StringWriter(sb), productsInRangeDto, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {

            var usersWithProductsDto = context.Users
                .Where(user => user.ProductsSold.Count > 0)
                .Select(user => new ExportUsersWithSoldProductsDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SoldProducts = user.ProductsSold.Select(product => new ExportProductDto {
                        Name = product.Name,
                        Price = product.Price,
                    })
                    .Take(5)
                    .ToList()
                })
                .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                .Take(5)
                .ToArray();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportUsersWithSoldProductsDto[]), new XmlRootAttribute("Users"));

            serializer.Serialize(new StringWriter(sb), usersWithProductsDto, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Include(x => x.CategoryProducts)
                .ThenInclude(y => y.Product)
                .Select(cat => new ExportCategoriesByProductsCountDto
            {
                Name = cat.Name,
                Count = cat.CategoryProducts.Count(),
                AveragePrice = cat.CategoryProducts.Average(product => product.Product.Price),
                TotalRevenue = cat.CategoryProducts.Sum(product => product.Product.Price),
            })
                .OrderByDescending(cat => cat.Count)
                .ThenBy(cat => cat.TotalRevenue)
                .ToArray();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportCategoriesByProductsCountDto[]), new XmlRootAttribute("Categories"));

            serializer.Serialize(new StringWriter(sb), categories, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            UsersAndProductsDTO[] users = context.Users.Where(user => user.ProductsSold.Count > 0)
                .Select(user => new UsersAndProductsDTO
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Age = user.Age,
                    SoldProducts = user.ProductsSold.Select(product => new SoldProductsDTO
                    {
                        Count = user.ProductsSold.Count(),
                        Products = user.ProductsSold.Select(pr => new ExportProductDto
                        {
                            Name = pr.Name,
                            Price = pr.Price,
                        })
                        .OrderByDescending(prod => prod.Price)
                        .ToArray()

                    })
                    .ToArray()
                })
                .OrderByDescending(user => user.SoldProducts.Count())
                .Take(10)
                .ToArray();

            ExportUsersAndProductsDTO finalUsers = new ExportUsersAndProductsDTO
            {
                Count = context.Users.Where(user => user.ProductsSold.Count > 0).Count(),
                UsersWithProducts = users,
            };

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportUsersAndProductsDTO), new XmlRootAttribute("Users"));

            serializer.Serialize(new StringWriter(sb), finalUsers, namespaces);

            return sb.ToString().Trim();
        }
    }
}