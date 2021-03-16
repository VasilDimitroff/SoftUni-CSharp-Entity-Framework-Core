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

            var result = GetProductsInRange(db);

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
    }
}