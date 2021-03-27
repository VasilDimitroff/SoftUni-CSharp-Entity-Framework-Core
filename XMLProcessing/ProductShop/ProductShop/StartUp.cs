using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var db = new ProductShopContext();
           // db.Database.EnsureCreated();

           // var xml = File.ReadAllText("../../../Datasets/categories-products.xml");

            var result = GetUsersWithProducts(db);

            Console.WriteLine(result);
        }

        //Import data
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportUserDto[]), new XmlRootAttribute("Users"));

            var users = (ImportUserDto[])serializer.Deserialize(new StringReader(inputXml));
            List<User> usersToAdd = new List<User>();

            foreach (var user in users)
            {
                var currentUser = new User()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Age = user.Age
                };

                usersToAdd.Add(currentUser);
            }

            context.Users.AddRange(usersToAdd);

            context.SaveChanges();

            return $"Successfully imported {usersToAdd.Count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(List<ImportProductDto>), new XmlRootAttribute("Products"));
            var products = (List<ImportProductDto>)serializer.Deserialize(new StringReader(inputXml));

            var productsToAdd = new List<Product>();

            foreach (var product in products)
            {
                var currentProduct = new Product() 
                {
                    Name = product.Name,
                    Price = product.Price,
                    BuyerId = product.BuyerId,
                    SellerId = product.SellerId
                };

                productsToAdd.Add(currentProduct);
            }

            context.Products.AddRange(productsToAdd);

            context.SaveChanges();

            return $"Successfully imported {productsToAdd.Count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(List<ImportCategoryDto>), new XmlRootAttribute("Categories"));
            var categories = (List<ImportCategoryDto>)serializer.Deserialize(new StringReader(inputXml));
            var categoriesToAdd = new List<Category>();

            foreach (var cat in categories)
            {
                var currentCategory = new Category()
                {
                    Name = cat.Name
                };

                if (currentCategory.Name == null)
                {
                    continue;
                }

                categoriesToAdd.Add(currentCategory);
            }

            context.Categories.AddRange(categoriesToAdd);

            context.SaveChanges();

            return $"Successfully imported {categoriesToAdd.Count}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var serializer = 
                new XmlSerializer(typeof(ImportCategoryProductDto[]), new XmlRootAttribute("CategoryProducts"));

            var categoryProducts = (ImportCategoryProductDto[])serializer.Deserialize(new StringReader(inputXml));

            var categories = context.Categories.Select(x => x.Id).ToArray();
            var products = context.Products.Select(x => x.Id).ToArray();

            List<CategoryProduct> cpForAdd = new List<CategoryProduct>();

            foreach (var cp in categoryProducts)
            {
                var productId = cp.ProductId;
                var categoryId = cp.CategoryId;

                if ((!categories.Contains(categoryId)) || (!products.Contains(productId)))
                {
                    continue;
                }

                var categoryProduct = new CategoryProduct()
                {
                    ProductId = cp.ProductId,
                    CategoryId = cp.CategoryId
                };

                cpForAdd.Add(categoryProduct);
            }

            context.CategoryProducts.AddRange(cpForAdd);

            context.SaveChanges();

            return $"Successfully imported {cpForAdd.Count}";
        }

        //Querying
        public static string GetProductsInRange(ProductShopContext context)
        {
            var sb = new StringBuilder();

            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select( x => new ProductInRangeDto
                 {
                    Name = x.Name,
                    Price = x.Price,
                    FullName = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .OrderBy(x => x.Price)
                .Take(10)
                .ToList();

            var serializer = new XmlSerializer(typeof(List<ProductInRangeDto>), new XmlRootAttribute("Products"));
            var namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]{
               new XmlQualifiedName(string.Empty, string.Empty)
            });

            using (var writer = new StringWriter(sb))
{
              serializer.Serialize(writer, products, namespaces);
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetSoldProducts(ProductShopContext context)
        {    
            var users = context.Users
                .Where(u => u.ProductsSold.Count() > 0)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new UserWithSoldProductsDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold.Select(p => new ProductWithNameAndPriceDto
                    {
                        Name = p.Name,
                        Price = p.Price
                    })
                    .ToArray()
                })
                .Take(5)
                .ToArray();

            var serializer = new XmlSerializer(typeof(UserWithSoldProductsDto[]), new XmlRootAttribute("Users"));
            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]{
               new XmlQualifiedName(string.Empty, string.Empty)
            });

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, users, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c => new CategoryDto
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count(),
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            var serializer = new XmlSerializer(typeof(CategoryDto[]), new XmlRootAttribute("Categories"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]{
               new XmlQualifiedName(string.Empty, string.Empty)
            });

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, categories, namespaces);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Count() > 0)
                .Select(u => new UserDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new SoldProductsDto
                    {
                        Count = u.ProductsSold.Where(pr => pr.Buyer != null).Count(),
                        Products = u.ProductsSold
                        .Where(pr => pr.Buyer != null)
                        .Select(p => new ProductWithNameAndPriceDto
                        {
                            Name = p.Name,
                            Price = p.Price,
                        })
                        .OrderByDescending(x => x.Price)
                        .ToArray()
                    }
                })
                .OrderByDescending(x => x.SoldProducts.Count)
                .ToArray();

            var resultObj = new ExportUserDto
            {
                Count = users.Count(),
                Users = users.Take(10).ToArray()
            };

            var serializer = new XmlSerializer(typeof(ExportUserDto));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]{
               new XmlQualifiedName(string.Empty, string.Empty)
            });

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, resultObj, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}