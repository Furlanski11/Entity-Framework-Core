using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Runtime.Serialization.Json;

namespace ProductShop
{
	public class StartUp
	{
		public static void Main()
		{
			ProductShopContext context = new ProductShopContext();

            Console.WriteLine(GetUsersWithProducts(context));
        }

		public static string ImportUsers(ProductShopContext context, string inputJson)
		{
			var users = JsonConvert.DeserializeObject<User[]>(inputJson);

			context.Users.AddRange(users);
			context.SaveChanges();

			return $"Successfully imported {users.Count()}";
		}

		public static string ImportProducts(ProductShopContext context, string inputJson)
		{
			var products = JsonConvert.DeserializeObject<Product[]>(inputJson);

			context.Products.AddRange(products);
			context.SaveChanges();

			return $"Successfully imported {products.Count()}";
		}

		public static string ImportCategories(ProductShopContext context, string inputJson)
		{
			var categories = JsonConvert.DeserializeObject<Category[]>(inputJson);

			var validCategories = categories?
				.Where(c => c.Name is not null)
				.ToArray();

			if (validCategories != null)
			{
				context.Categories.AddRange(validCategories);
				context.SaveChanges();
				return $"Successfully imported {validCategories.Count()}";
			}

			return $"Successfully imported 0";
		}

		public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
		{
			var categoryProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

			context.CategoriesProducts.AddRange(categoryProducts);
			context.SaveChanges();

			return $"Successfully imported {categoryProducts.Count()}";
		}

		public static string GetProductsInRange(ProductShopContext context)
		{
			var products = context.Products
				.Where(p => p.Price >= 500 && p.Price <= 1000)
				.Select(p => new
				{
					name = p.Name,
					price = p.Price,
					seller = p.Seller.FirstName + " " + p.Seller.LastName

				}).OrderBy(p => p.price)
				.ToArray();

			var productsJson = JsonConvert.SerializeObject(products, Formatting.Indented);

			return productsJson;
		}

		public static string GetSoldProducts(ProductShopContext context)
		{
			var users = context.Users
				.Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
				.OrderBy(u => u.LastName)
					.ThenBy(u => u.FirstName)
				.Select(u => new
				{
					firstName = u.FirstName,
					lastName = u.LastName,
					soldProducts = u.ProductsSold
					.Select(ps => new
					{
						name = ps.Name,
						price = ps.Price,
						buyerFirstName = ps.Buyer.FirstName,
						buyerLastName = ps.Buyer.LastName
					}).ToArray()

				}).ToArray();

			var json = JsonConvert.SerializeObject(users, Formatting.Indented);
			return json;
		}

		public static string GetCategoriesByProductsCount(ProductShopContext context)
		{
			var categories = context.Categories
				.Select(c => new
				{
					category = c.Name,
					productsCount = c.CategoriesProducts.Count(),
					averagePrice = c.CategoriesProducts
						.Average(cp => cp.Product.Price).ToString("f2"),
                    totalRevenue = c.CategoriesProducts
					    .Sum(cp => cp.Product.Price).ToString("f2")
				}).OrderByDescending(x => x.productsCount)
				.ToArray();

			var json = JsonConvert.SerializeObject(categories, Formatting.Indented);
			return json;
		}

		public static string GetUsersWithProducts(ProductShopContext context)
		{
			var users = context.Users
				.Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
				.Select(u => new
				{
					firstName = u.FirstName,
					lastName = u.LastName,
					age = u.Age,
					soldProducts = u.ProductsSold
					.Where(p => p.BuyerId != null)
					.Select(p => new
					{
						name = p.Name,
						price = p.Price
					}).ToArray()
				}).OrderByDescending(u => u.soldProducts.Count())
				.ToArray();

			var output = new
			{
				usersCount = users.Count(),
				users = users.Select(u => new
				{
					u.firstName,
					u.lastName,
					u.age,
					soldProducts = new
					{
						count = u.soldProducts.Count(),
						products = u.soldProducts
					}

				})
			};

			string json = JsonConvert.SerializeObject(output, new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore
			});

			return json;
		}

	}
}