using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();
            context.Database.EnsureCreated();

            string json = File.ReadAllText("../../../Datasets/cars.json");

            var result = ImportCars(context, json);
            Console.WriteLine(result);
        }

        //Problem 8
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            Supplier[] suppliers = JsonConvert.DeserializeObject<Supplier[]>(inputJson);

            context.Suppliers.AddRange(suppliers);

            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}.";
        }

        //Problem 9
        public static string ImportParts(CarDealerContext context, string inputJson)
        {

            //var settings = new JsonSerializerSettings();

           // settings.NullValueHandling = NullValueHandling.Ignore;
           // settings.DefaultValueHandling = DefaultValueHandling.Ignore;

            Part[] parts = JsonConvert.DeserializeObject<Part[]>(inputJson)
                .ToArray();

            int[] suppliers = context.Suppliers.Select(s => s.Id).ToArray();

            parts = parts.Where(part => suppliers.Any(supplier => supplier == part.SupplierId))
                .ToArray();

            context.Parts.AddRange(parts);

            context.SaveChanges();

            return $"Successfully imported {parts.Length}.";
        }

        //Problem 10
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            //return $"Successfully imported {cars.Count}.";
        }
    }
}