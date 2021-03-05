using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {

        public static void Main(string[] args)
        {
            var context = new CarDealerContext();
            context.Database.EnsureCreated();

           // string json = File.ReadAllText("../../../Datasets/suppliers.json");
           // string json2 = File.ReadAllText("../../../Datasets/parts.json");
           // string json3 = File.ReadAllText("../../../Datasets/cars.json");
           // string json4 = File.ReadAllText("../../../Datasets/customers.json");
           // string json5 = File.ReadAllText("../../../Datasets/sales.json");

          // ImportSuppliers(context, json);
         //  ImportParts(context, json2);
        //   ImportCars(context, json3);
       //    ImportCustomers(context, json4);
       //    ImportSales(context, json5);
            var result = GetSalesWithAppliedDiscount(context);
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
            ImportCarDto[] carsDto = JsonConvert.DeserializeObject<ImportCarDto[]>(inputJson);

            List<Car> cars = new List<Car>();

            foreach (ImportCarDto carDto in carsDto.Distinct())
            {
                Car car = new Car
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance,
                };

                foreach (var partCarDto in carDto.PartCars)
                {

                    PartCar partCar = new PartCar
                    {
                        Car = car,
                        Part = partCarDto.Part

                    };

                    car.PartCars.Add(partCar);
                }

                cars.Add(car);

            }

            context.Cars.AddRange(cars);

            context.SaveChanges();

            return $"Successfully imported {cars.Count}.";
        }

        //Problem 11
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            ImportCustomerDto[] customersDto = JsonConvert.DeserializeObject<ImportCustomerDto[]>(inputJson);

            List<Customer> customers = new List<Customer>();

            foreach (ImportCustomerDto customerDto in customersDto)
            {
                Customer customer = new Customer()
                {
                    Name = customerDto.Name,
                    BirthDate = customerDto.BirthDate,
                    IsYoungDriver = customerDto.IsYoungDriver,
                };

                customers.Add(customer);
            }

            context.Customers.AddRange(customers);

            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }

        //Problem 12
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            List<Sale> sales = JsonConvert.DeserializeObject<List<Sale>>(inputJson);
            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}.";
        }

        //Problem 13
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(customer => new
            {
                Name = customer.Name,
                BirthDate = customer.BirthDate.ToString("dd/MM/yyyy"),
                IsYoungDriver = customer.IsYoungDriver,
            }) 
                .ToArray();

            string json = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return json;
        }

        //Problem 14
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var toyotaCars = context.Cars.Where(car => car.Make == "Toyota")
                .Select(car => new
                {
                    Id = car.Id,
                    Make = car.Make,
                    Model = car.Model,
                    TravelledDistance = car.TravelledDistance,
                })
                .OrderBy(car => car.Model)
                .ThenByDescending(car => car.TravelledDistance)
                .ToList();

            string json = JsonConvert.SerializeObject(toyotaCars, Formatting.Indented);

            return json;
        }

        //Problem 15
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(supplier => supplier.IsImporter == false)
                .Select(supplier => new
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    PartsCount = supplier.Parts.Count()
                })
                .ToList();

            string json = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return json;
        }

        //Problem 16
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(car => new
                {
                    car = new 
                    {
                        car.Make,
                        car.Model,
                        car.TravelledDistance,       
                    },
                    parts = car.PartCars.Select(part => new
                    {
                        Name = part.Part.Name,
                        Price = part.Part.Price.ToString("f2"),
                    }).ToList()
                })
                .ToList();


            string json = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return json;
        }

        //Problem 17
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(customer => customer.Sales.Count() > 0)
                .Select(customer => new
                {
                    fullName = customer.Name,
                    boughtCars = customer.Sales.Count(),
                    spentMoney = customer.Sales.Sum(s => s.Car.PartCars.Sum(p => p.Part.Price)),
                })
                .OrderByDescending(x => x.spentMoney)
                .ThenByDescending(x => x.boughtCars)
                .ToList();

            string json = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return json;
        }

        //Problem 18
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var carsWithCustomers = context.Sales.Select(sale => new
            {
                car = new
                {
                    Make = sale.Car.Make,
                    Model = sale.Car.Model,
                    TravelledDistance = sale.Car.TravelledDistance
                },

                customerName = sale.Customer.Name,

                Discount = sale.Discount.ToString("f2"),

                price = (sale.Car.PartCars.Select(part => part.Part.Price).Sum()).ToString("f2"),

                priceWithDiscount = ((sale.Car.PartCars.Select(part => part.Part.Price).Sum())
                - ((sale.Discount / 100) * (sale.Car.PartCars.Select(part => part.Part.Price).Sum()))).ToString("f2"),
            })
                .Take(10)
                .ToList();

            string json = JsonConvert.SerializeObject(carsWithCustomers, Formatting.Indented);

            return json;
        }
       

    }
}