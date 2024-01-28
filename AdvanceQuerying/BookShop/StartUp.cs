namespace BookShop
{
	using BookShop.Models.Enums;
	using Data;
	using Initializer;
	using Microsoft.EntityFrameworkCore;
	using System.Globalization;
	using System.Text;

	public class StartUp
	{
		public static void Main()
		{
			using var db = new BookShopContext();
			//DbInitializer.ResetDatabase(db);
			Console.WriteLine(GetBooksByAuthor(db, "R"));



		}

		public static string GetBooksByAgeRestriction(BookShopContext context, string command)
		{
			if (!Enum.TryParse<AgeRestriction>(command, true, out var ageRestriction))
			{
				return $"{command} is not a valid restriction!";
			}
			var titles = context.Books.
				Where(b => b.AgeRestriction == ageRestriction)
				.Select(b => new
				{
					BookTitle = b.Title,

				}).ToList().OrderBy(b => b.BookTitle);


			return string.Join(Environment.NewLine, titles.Select(t => t.BookTitle));
		}

		public static string GetGoldenBooks(BookShopContext context)
		{
			var books = context.Books
				.Where(b => b.Copies < 5000 && b.EditionType == EditionType.Gold)
				.OrderBy(b => b.BookId)
				.Select(b => new
				{
					BookTitle = b.Title
				});

			return string.Join(Environment.NewLine, books.Select(b => b.BookTitle));
		}

		public static string GetBooksByPrice(BookShopContext context)
		{
			var books = context.Books
				.Where(b => b.Price > 40)
				.Select(b => new
				{
					BookTitle = b.Title,
					BookPrice = b.Price
				}).OrderByDescending(b => b.BookPrice);

			return string.Join(Environment.NewLine, books.Select(b => $"{b.BookTitle} - ${b.BookPrice:f2}"));
		}

		public static string GetBooksNotReleasedIn(BookShopContext context, int year)
		{
			var books = context.Books
				.Where(b => b.ReleaseDate.HasValue && b.ReleaseDate.Value.Year != year)
				.OrderBy(b => b.BookId)
				.Select(b => new
				{
					BookTitle = b.Title
				});
			return string.Join(Environment.NewLine, books.Select(b => b.BookTitle));
		}

		public static string GetBooksByCategory(BookShopContext context, string input)
		{
			var categories = input
				.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.Select(c => c.ToLower())
				.ToArray();

			var books = context.Books
				.Where(b => b.BookCategories.Any(bc => categories.Contains(bc.Category.Name.ToLower())))
				.OrderBy(b => b.Title)
				.ToList();

			return string.Join(Environment.NewLine, books.Select(b => b.Title));

		}

		public static string GetBooksReleasedBefore(BookShopContext context, string date)
		{
			DateTime searchedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
			var books = context.Books
				.Where(b => b.ReleaseDate < searchedDate)
				.OrderByDescending(b => b.ReleaseDate)
				.Select(b => new
				{
					BookTitle = b.Title,
					BookEdition = b.EditionType,
					BookPrice = b.Price
				});

			return string.Join(Environment.NewLine, books.Select(b => $"{b.BookTitle} - {b.BookEdition} - ${b.BookPrice:f2}"));
		}

		public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
		{

			var authors = context.Authors
				.Where(a => a.FirstName.EndsWith(input))
				.Select(a => new
				{
					FullName = a.FirstName + " " + a.LastName
				})
				.OrderBy(a => a.FullName);

			return string.Join(Environment.NewLine, authors.Select(a => a.FullName));
		}

		public static string GetBookTitlesContaining(BookShopContext context, string input)
		{
			// Option 1
			//var books = context.Books
			//    .Where(b => b.Title.ToLower().Contains(input.ToLower()))
			//    .Select(b => new
			//    {
			//        b.Title
			//    })
			//    .OrderBy(b => b.Title);

			//return string.Join(Environment.NewLine, books.Select(b => b.Title));


			// Option 2
			var books = context.Books
				.Where(b => EF.Functions.Like(b.Title, $"%{input}%"))
				.Select(b => new
				{
					b.Title
				})
				.OrderBy(b => b.Title);

			return string.Join(Environment.NewLine, books.Select(b => b.Title));

		}

		public static string GetBooksByAuthor(BookShopContext context, string input)
		{
			// Option 1

			//var books = context.Books
			//    .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
			//    .Select(b => new
			//    {
			//        BookTitle = b.Title,
			//        AuthorName = b.Author.FirstName + " " + b.Author.LastName
			//    });

			//return string.Join(Environment.NewLine,
			//    books.Select(b => $"{b.BookTitle} ({b.AuthorName})"));


			var books = context.Books
				.Where(b => EF.Functions.Like(b.Author.LastName, $"{input}%"))
				.OrderBy(b => b.BookId)
				.Select(b => new
				{
					BookTitle = b.Title,
					AuthorName = b.Author.FirstName + " " + b.Author.LastName
				});

			return string.Join(Environment.NewLine,
				books.Select(b => $"{b.BookTitle} ({b.AuthorName})"));
		}

		public static int CountBooks(BookShopContext context, int lengthCheck)
		{
			var count = context.Books
				.Where(b => b.Title.Length > lengthCheck)
				.Count();

			return count;
		}

		public static string CountCopiesByAuthor(BookShopContext context)
		{
			var totalCopies = context.Authors
				.Select(a => new
				{
					AuthorName = a.FirstName + " " + a.LastName,
					CopiesCount = a.Books.Sum(b => b.Copies)
				}).OrderByDescending(a => a.CopiesCount);

			return string.Join(Environment.NewLine, totalCopies.Select(tc => $"{tc.AuthorName} - {tc.CopiesCount}"));
		}

		public static string GetTotalProfitByCategory(BookShopContext context)
		{
			var totalProfit = context.Categories
				.Select(c => new
				{
					CategoryName = c.Name,
					TotatProfit = c.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price)
				}).OrderByDescending(tp => tp.TotatProfit).ThenBy(tp => tp.CategoryName);


			return string.Join(Environment.NewLine, totalProfit.Select(tp => $"{tp.CategoryName} ${tp.TotatProfit:f2}"));
		}

		public static string GetMostRecentBooks(BookShopContext context)
		{

			var mostRecentBooks = context.Categories
				.Select(c => new
				{
					CategoryName = c.Name,
					Books = c.CategoryBooks.OrderByDescending(b => b.Book.ReleaseDate)
					.Take(3)
					.Select(cb => new
					{
						BookTitle = cb.Book.Title,
						ReleaseDate = cb.Book.ReleaseDate!.Value.Year
					})
				}).OrderBy(c => c.CategoryName);

			StringBuilder stringBuilder = new StringBuilder();

			foreach (var book in mostRecentBooks)
			{
				stringBuilder.AppendLine($"--{book.CategoryName}");
				foreach (var item in book.Books)
				{
					stringBuilder.AppendLine($"{item.BookTitle} ({item.ReleaseDate})");
				}
			}

			return stringBuilder.ToString().TrimEnd();

		}

		public static void IncreasePrices(BookShopContext context)
		{
			var booksToIncreasePrice = context.Books
				.Where(b => b.ReleaseDate.HasValue && b.ReleaseDate.Value.Year < 2010).ToList();

			foreach (var book in booksToIncreasePrice)
			{
				book.Price += 5;
			}
			context.SaveChanges();
		}

		public static int RemoveBooks(BookShopContext context)
		{
			context.ChangeTracker.Clear();

			var booksToRemove = context.Books
				.Where(b => b.Copies < 4200);

			context.RemoveRange(booksToRemove);

			return context.SaveChanges();
		}
	}
}


