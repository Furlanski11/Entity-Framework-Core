using AutoMapper;
using Castle.Core.Internal;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using ProductShop.Utilities;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using static ProductShop.DTOs.Export.ExportUsersAndProductsDTO;

namespace ProductShop
{
	public class StartUp
	{
		public static void Main()
		{
			ProductShopContext context = new ProductShopContext();
			//1.
			//string importUsersInput = File.ReadAllText("../../../Datasets/users.xml");
			//Console.WriteLine(ImportUsers(context, importUsersInput));
			//2.
			//string importProductsInput = File.ReadAllText("../../../Datasets/products.xml");
			//Console.WriteLine(ImportProducts(context, importProductsInput));
			//3.
			//string importCategories = File.ReadAllText("../../../Datasets/categories.xml");
			//Console.WriteLine(ImportCategories(context, importCategories));
			//4.
			//string importCategoriesInput = File.ReadAllText("../../../Datasets/categories-products.xml");
			//Console.WriteLine(ImportCategoryProducts(context, importCategoriesInput));
			//5.
			Console.WriteLine(GetProductsInRange(context));
			//6.
			//Console.WriteLine(GetSoldProducts(context));
			//7.
			//Console.WriteLine(GetCategoriesByProductsCount(context));
			//8.
			//Console.WriteLine(GetUsersWithProducts(context));
		}

		private static Mapper GetMapper()
		{
			var cfg = new MapperConfiguration(c => c.AddProfile<ProductShopProfile>());
			return new Mapper(cfg);
		}
		//1.
		public static string ImportUsers(ProductShopContext context, string inputXml)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportUserDTO[]), new XmlRootAttribute("Users"));

			using StringReader reader = new StringReader(inputXml);

			ImportUserDTO[] importUsersDTOs = (ImportUserDTO[])xmlSerializer.Deserialize(reader);

			var mapper = GetMapper();

			User[] users = mapper.Map<User[]>(importUsersDTOs);

			context.AddRange(users);
			context.SaveChanges();

			return $"Successfully imported {users.Count()}";
		}

		public static string ImportProducts(ProductShopContext context, string inputXml)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportProductDTO[]), new XmlRootAttribute("Products"));

			using StringReader reader = new StringReader(inputXml);

			ImportProductDTO[] importProductDTOs = (ImportProductDTO[])xmlSerializer.Deserialize(reader);

			var mapper = GetMapper();

			Product[] products = mapper.Map<Product[]>(importProductDTOs);

			context.AddRange(products);
			context.SaveChanges();

			return $"Successfully imported {products.Count()}";
		}

		public static string ImportCategories(ProductShopContext context, string inputXml)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCategoriesDTO[]), new XmlRootAttribute("Categories"));

			using StringReader reader = new StringReader(inputXml);

			ImportCategoriesDTO[] importCategoriesDTOs = (ImportCategoriesDTO[])xmlSerializer.Deserialize(reader);

			var mapper = GetMapper();

			Category[] categories = mapper.Map<Category[]>(importCategoriesDTOs.Where(c => c.Name != null));

			context.AddRange(categories);
			context.SaveChanges();

			return $"Successfully imported {categories.Count()}";
		}

		public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCategoriesProductsDTO[]), new XmlRootAttribute("CategoryProducts"));

			using StringReader reader = new StringReader(inputXml);

			ImportCategoriesProductsDTO[] importCategoriesProductsDTOs = (ImportCategoriesProductsDTO[])xmlSerializer.Deserialize(reader);

			var mapper = GetMapper();

			var validCategoryProducts = new List<CategoryProduct>();

			HashSet<int> ProductIds = context.Products.Select(p => p.Id).ToHashSet<int>();
			HashSet<int> CategoryIds = context.Categories.Select(c => c.Id).ToHashSet<int>();

			foreach (var dto in importCategoriesProductsDTOs)
			{
				if (ProductIds.Contains(dto.ProductId) && CategoryIds.Contains(dto.CategoryId))
				{
					var categoryProduct = mapper.Map<CategoryProduct>(dto);
					validCategoryProducts.Add(categoryProduct);
				}

			}
			context.CategoryProducts.AddRange(validCategoryProducts);
			context.SaveChanges();

			return $"Successfully imported {validCategoryProducts.Count}";
		}

		public static string GetProductsInRange(ProductShopContext context)
		{
			var xmlParser = new XmlParser();

			//Selecting the Products
			var productsInRange = context.Products
					.Where(p => p.Price >= 500 && p.Price <= 1000)
					.OrderBy(p => p.Price)
					.Take(10)
					.Select(p => new ExportProductsInRange()
					{
						Price = p.Price,
						Name = p.Name,
						BuyerName = p.Buyer.FirstName + " " + p.Buyer.LastName
					})
					.ToArray();

			//Output
			return xmlParser.Serialize<ExportProductsInRange[]>(productsInRange, "Products");
		}

		public static string GetSoldProducts(ProductShopContext context)
		{
			var users = context.Users
				.Where(u => u.ProductsSold.Count >= 1)
				.OrderBy(u => u.LastName)
					.ThenBy(u => u.FirstName)
				.Take(5)
				.Select(u => new ExportSoldProductsDTO
				{
					firstName = u.FirstName,
					lastName = u.LastName,
					soldProducts = u.ProductsSold.Select(p => new SoldProductDTO()
					{
						name = p.Name,
						price = p.Price
					}).ToArray()

				}).ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportSoldProductsDTO[]), new XmlRootAttribute("Users"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter stringWriter = new StringWriter(sb))
			{
				xmlSerializer.Serialize(stringWriter, users, xsn);
			}
			return sb.ToString().TrimEnd();
		}

		public static string GetCategoriesByProductsCount(ProductShopContext context)
		{
			var categories = context.Categories
				.Select(c => new ExportCategoriesByProductsCount
				{
					name = c.Name,
					count = c.CategoryProducts.Count,
					averagePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
					totalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
				})
				.OrderByDescending(exd => exd.count)
					.ThenBy(exd => exd.totalRevenue)
				.ToArray();

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCategoriesByProductsCount[]), new XmlRootAttribute("Categories"));

			var xsn = new XmlSerializerNamespaces();
			xsn.Add(string.Empty, string.Empty);

			StringBuilder sb = new StringBuilder();

			using (StringWriter stringWriter = new StringWriter(sb))
			{
				xmlSerializer.Serialize(stringWriter, categories, xsn);
			}
			return sb.ToString().TrimEnd();
		}

		public static string GetUsersWithProducts(ProductShopContext context)
		{
			var xmlParser = new XmlParser();

			//Selecting the Users
			var usersInfo = context
					.Users
					.Where(u => u.ProductsSold.Any())
					.OrderByDescending(u => u.ProductsSold.Count)
					.Select(u => new UserInfo()
					{
						FirstName = u.FirstName,
						LastName = u.LastName,
						Age = u.Age,
						SoldProducts = new SoldProductsCount()
						{
							Count = u.ProductsSold.Count,
							Products = u.ProductsSold.Select(p => new SoldProduct()
							{
								Name = p.Name,
								Price = p.Price
							})
							.OrderByDescending(p => p.Price)
							.ToArray()
						}
					})
					.Take(10)
					.ToArray();

			ExportUserCountDto exportUserCountDto = new ExportUserCountDto()
			{
				Count = context.Users.Count(u => u.ProductsSold.Any()),
				Users = usersInfo
			};

			//Output
			return xmlParser.Serialize<ExportUserCountDto>(exportUserCountDto, "Users");
		}
	}
}