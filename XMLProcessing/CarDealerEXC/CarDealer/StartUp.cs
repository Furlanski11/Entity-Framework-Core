using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();
			//9.
			//string inputSuppliersXml = File.ReadAllText("../../../Datasets/suppliers.xml");
			//Console.WriteLine(ImportSuppliers(context, inputSuppliersXml));

			//10.
			//string inputPartsXml = File.ReadAllText("../../../Datasets/parts.xml");
			//Console.WriteLine(ImportParts(context, inputPartsXml));

			//11.
			//string inputCarsXml = File.ReadAllText("../../../Datasets/cars.xml");
			//Console.WriteLine(ImportCars(context, inputCarsXml));

			//12.
			//string inputCarsXml = File.ReadAllText("../../../Datasets/customers.xml");
			//Console.WriteLine(ImportCustomers(context, inputCarsXml));

			//13.
			//string inputSalesXml = File.ReadAllText("../../../Datasets/sales.xml");
			//Console.WriteLine(ImportSales(context, inputSalesXml));

			//14.
			//Console.WriteLine(GetCarsWithDistance(context));

			//15.
			//Console.WriteLine(GetCarsFromMakeBmw(context));

			//16.
			//Console.WriteLine(GetLocalSuppliers(context));

			//17.
			// Console.WriteLine(GetCarsWithTheirListOfParts(context));

			//18.
			Console.WriteLine(GetTotalSalesByCustomer(context));
			//19.
			//Console.WriteLine(GetSalesWithAppliedDiscount(context));


		}
		private static Mapper GetMapper()
        {
            var cfg = new MapperConfiguration(c => c.AddProfile<CarDealerProfile>());
            return new Mapper(cfg);
        }
        //9.
		public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            //1.Create xml serializer
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSuppliersDTO[]),
                                        new XmlRootAttribute("Suppliers"));
            
            using StringReader reader = new StringReader(inputXml);

            ImportSuppliersDTO[] importSuppliersDTOs = (ImportSuppliersDTO[])xmlSerializer.Deserialize(reader);

            var mapper = GetMapper();

            Supplier[] suppliers = mapper.Map<Supplier[]>(importSuppliersDTOs);

            context.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";
		}
        //10.
		public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPartsDTO[]), new XmlRootAttribute("Parts"));

            StringReader reader = new StringReader(inputXml);

            ImportPartsDTO[] importPartsDTOs = (ImportPartsDTO[])xmlSerializer.Deserialize(reader);

            var mapper = GetMapper();

            Part[] parts = mapper.Map<Part[]>(importPartsDTOs);
            List<Part> valirdParts = new List<Part>();

            var supplierIds = context.Suppliers
                .Select(s => s.Id)
                .ToList();

            foreach (Part part in parts)
            {
                if (!supplierIds.Contains(part.SupplierId))
                {
                    continue;
                }
                valirdParts.Add(part);
            }
            context.AddRange(valirdParts);
            context.SaveChanges();

			return $"Successfully imported {valirdParts.Count}";

		}
        //11.
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
			XmlSerializer xmlSerializer =
			  new XmlSerializer(typeof(ImportCarDTO[]), new XmlRootAttribute("Cars"));

			using StringReader stringReader = new StringReader(inputXml);

			ImportCarDTO[] importCarDTOs = (ImportCarDTO[])xmlSerializer.Deserialize(stringReader);

			var mapper = GetMapper();
			List<Car> cars = new List<Car>();

			foreach (var carDTO in importCarDTOs)
			{
				Car car = mapper.Map<Car>(carDTO);

				int[] carPartIds = carDTO.PartsIds
					.Select(x => x.Id)
					.Distinct()
					.ToArray();

				var carParts = new List<PartCar>();

				foreach (var id in carPartIds)
				{
					carParts.Add(new PartCar
					{
						Car = car,
						PartId = id
					});
				}

				car.PartsCars = carParts;
				cars.Add(car);
			}

			context.AddRange(cars);
			context.SaveChanges();

			return $"Successfully imported {cars.Count}";
		}
		//12.
		public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCustomerDTO[]), new XmlRootAttribute("Customers"));

			using StringReader stringReader = new StringReader(inputXml);

			ImportCustomerDTO[] importCustomerDTOs = (ImportCustomerDTO[])xmlSerializer.Deserialize(stringReader);

			var mapper = GetMapper();

			Customer[] customers = mapper.Map<Customer[]>(importCustomerDTOs);

			context.AddRange(customers);
			context.SaveChanges();

			return $"Successfully imported {customers.Length}";
		}
		//13.
		public static string ImportSales(CarDealerContext context, string inputXml)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSaleDTO[]), new XmlRootAttribute("Sales"));

			using StringReader inputReader = new StringReader(inputXml);

			ImportSaleDTO[] importSaleDTOs = (ImportSaleDTO[])xmlSerializer.Deserialize(inputReader);

			var mapper = GetMapper();

			var carIds = context.Cars.Select(c => c.Id).ToList();

			var sales = mapper.Map<Sale[]>(importSaleDTOs);

			var validSales = new List<Sale>();

			foreach (var saleDTO in sales)
			{
				if (carIds.Contains(saleDTO.CarId))
				{
					validSales.Add(saleDTO);
				}
			}
			context.AddRange(validSales);
			context.SaveChanges();
			return $"Successfully imported {validSales.Count}"; ;
		}
		//14.
		public static string GetCarsWithDistance(CarDealerContext context)
		{
			var mapper = GetMapper();

			var carsToExport = context.Cars
				.Where(c => c.TraveledDistance > 2000000)
				.OrderBy(c => c.Make)
					.ThenBy(c => c.Model)
				.Take(10)
				.ProjectTo<ExportCarsWithDistance>(mapper.ConfigurationProvider)
				.ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarsWithDistance[]), new XmlRootAttribute("cars"));


			var xsn = new XmlSerializerNamespaces();

			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb))
			{
				xmlSerializer.Serialize(sw, carsToExport, xsn);
			}

			return sb.ToString().TrimEnd();
		}
		//15.
		public static string GetCarsFromMakeBmw(CarDealerContext context)
		{
			var mapper = GetMapper();

			var carsToExport = context.Cars
				.Where(c => c.Make == "BMW")
				.OrderBy(c => c.Model)
					.ThenByDescending(c => c.TraveledDistance)
				.ProjectTo<ExportBMWcars>(mapper.ConfigurationProvider)
				.ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportBMWcars[]), new XmlRootAttribute("cars"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb))
			{
				xmlSerializer.Serialize(sw , carsToExport, xsn);
			}
			return sb.ToString().TrimEnd();
		}
		//16.
		public static string GetLocalSuppliers(CarDealerContext context)
		{
			
			var localSuppliersToExport = context.Suppliers
				.Where(s => !s.IsImporter)
				.Select(s => new ExportLocalSuppliers
				{
					Id = s.Id,
					Name = s.Name,
				    PartsCount = s.Parts.Count
				})
				.ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportLocalSuppliers[]), new XmlRootAttribute("suppliers"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using(StringWriter sw = new StringWriter(sb))
			{
				xmlSerializer.Serialize(sw, localSuppliersToExport, xsn);
			}
			return sb.ToString().TrimEnd();
		}
		//17.
		public static string GetCarsWithTheirListOfParts(CarDealerContext context)
		{
			var cars = context.Cars
				.OrderByDescending(c => c.TraveledDistance)
				.ThenBy(c => c.Model)
				.Take(5)
				.Select(c => new ExportCarsWithParts
				{
					Make = c.Make,
					Model = c.Model,
					TraveledDistance = c.TraveledDistance,
					Parts = c.PartsCars.Select(pc => new ExportPartsDto() 
					{
						Name = pc.Part.Name,
						Price = pc.Part.Price,

					})
					.OrderByDescending(p => p.Price)
					.ToArray()
				}).ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarsWithParts[]), new XmlRootAttribute("cars"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb))
			{
				xmlSerializer.Serialize(sw, cars, xsn);
			}
			return sb.ToString().TrimEnd();
		}
		//18.
		public static string GetTotalSalesByCustomer(CarDealerContext context)
		{

			//Finding the Sales
			var tempDto = context.Customers
				.Where(c => c.Sales.Any())
				.Select(c => new
				{
					FullName = c.Name,
					BoughtCars = c.Sales.Count(),
					SalesInfo = c.Sales.Select(s => new
					{
						Prices = c.IsYoungDriver
							? s.Car.PartsCars.Sum(p => Math.Round((double)p.Part.Price * 0.95, 2))
							: s.Car.PartsCars.Sum(p => (double)p.Part.Price)
					}).ToArray(),
				})
				.ToArray();

			TotalSalesByCustomerDto[] totalSalesDtos = tempDto
				.OrderByDescending(t => t.SalesInfo.Sum(s => s.Prices))
				.Select(t => new TotalSalesByCustomerDto()
				{
					FullName = t.FullName,
					BoughtCars = t.BoughtCars,
					SpentMoney = t.SalesInfo.Sum(s => s.Prices).ToString("f2")
				})
				.ToArray();

			//Output
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TotalSalesByCustomerDto[]), new XmlRootAttribute("customers"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb))
			{
				xmlSerializer.Serialize(sw, totalSalesDtos, xsn);
			}
			return sb.ToString().TrimEnd();

		}

		//19.
		public static string GetSalesWithAppliedDiscount(CarDealerContext context)
		{

			SalesWithAppliedDiscountDto[] salesDtos = context
				.Sales
				.Select(s => new SalesWithAppliedDiscountDto()
				{
					SingleCar = new SingleCar()
					{
						Make = s.Car.Make,
						Model = s.Car.Model,
						TraveledDistance = s.Car.TraveledDistance
					},
					Discount = (int)s.Discount,
					CustomerName = s.Customer.Name,
					Price = s.Car.PartsCars.Sum(p => p.Part.Price),
					PriceWithDiscount = Math.Round((double)(s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - (s.Discount / 100))), 4)
				})
				.ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SalesWithAppliedDiscountDto[]), new XmlRootAttribute("sales"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter sw = new StringWriter(sb))
			{
				xmlSerializer.Serialize(sw, salesDtos, xsn);
			}
			return sb.ToString().TrimEnd();
		}
	}
}