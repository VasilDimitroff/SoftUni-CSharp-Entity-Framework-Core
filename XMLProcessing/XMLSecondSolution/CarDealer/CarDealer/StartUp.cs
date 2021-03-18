using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var db = new CarDealerContext();

            // db.Database.EnsureCreated();

            var xml = File.ReadAllText("../../../Datasets/customers.xml");
            var result = ImportCustomers(db, xml);

            Console.WriteLine(result);
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportSupplierDto[]), new XmlRootAttribute("Suppliers"));
            var suppliersDto = (ImportSupplierDto[])serializer.Deserialize(new StringReader(inputXml));

            Supplier[] suppliers = suppliersDto.Select(x => new Supplier
            {
                Name = x.Name,
                IsImporter = x.IsImporter
            })
                .ToArray();

            context.Suppliers.AddRange(suppliers);

            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";

        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportPartDto[]), new XmlRootAttribute("Parts"));
            var partsDto = (ImportPartDto[])serializer.Deserialize(new StringReader(inputXml));

            var parts = new List<Part>();
            var supplierIds = context.Suppliers.Select(x => x.Id).ToArray();

            foreach (var partDto in partsDto)
            {
                var part = new Part
                {
                    Name = partDto.Name,
                    Price = partDto.Price,
                    Quantity = partDto.Quantity,
                    SupplierId = partDto.SupplierId
                };

                if (!supplierIds.Contains(part.SupplierId))
                {
                    continue;
                }

                parts.Add(part);
            }

            context.Parts.AddRange(parts);

            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportCarDto[]), new XmlRootAttribute("Cars"));
            var carsDtos = (ImportCarDto[])serializer.Deserialize(new StringReader(inputXml));

            var cars = new List<Car>();
            var partCars = new List<PartCar>();

            foreach (var carDto in carsDtos)
            {
                var car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TraveledDistance
                };

                var parts = carDto
                    .Parts
                    .Where(pc => context.Parts.Any(p => p.Id == pc.Id))
                    .Select(p => p.Id)
                    .Distinct();

                foreach (var part in parts)
                {
                    PartCar partCar = new PartCar()
                    {
                        PartId = part,
                        Car = car
                    };

                    partCars.Add(partCar);
                }

                cars.Add(car);

            }

            context.PartCars.AddRange(partCars);

            context.Cars.AddRange(cars);

           context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportCustomerDto[]), new XmlRootAttribute("Customers"));
            var customersDto = (ImportCustomerDto[])serializer.Deserialize(new StringReader(inputXml));

            var customers = new List<Customer>();

            foreach (var cusDto in customersDto)
            {
                var customer = new Customer
                {
                    Name = cusDto.Name,
                    BirthDate = cusDto.BirthDate,
                    IsYoungDriver = cusDto.IsYoungDriver
                };

                customers.Add(customer);
            }

            context.Customers.AddRange(customers);

            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }
    }
}