namespace Invoices.DataProcessor
{
    using Invoices.Data;
	using Invoices.Data.Utilities;
	using Invoices.DataProcessor.ExportDto;
	using Newtonsoft.Json;
	using System.Globalization;

	public class Serializer
    {
        public static string ExportClientsWithTheirInvoices(InvoicesContext context, DateTime date)
        {
			//Selecting the Clients and their Invoices
			var products = context.Clients
				.Where(c => c.Invoices.Any(i => i.IssueDate > date))
				.Select(c => new ExportClientDto
				{
					InvoicesCount = c.Invoices.Count,
					ClientName = c.Name,
					VatNumber = c.NumberVat,
					Invoices = c.Invoices
					.OrderBy(i => i.IssueDate)
					.ThenByDescending(i => i.DueDate)
					.Select(i => new ExportInvoiceDto
					{
						InvoiceNumber = i.Number,
						InvoiceAmount = i.Amount,
						DueDate = i.DueDate.ToString("d", CultureInfo.InvariantCulture),
						Currency = i.CurrencyType.ToString()
					})
					.ToArray()
				})
				.OrderByDescending(i => i.InvoicesCount)
				.ThenBy(i => i.ClientName)
				.ToArray();

			//Output in XML Format
			return new XmlHelper().Serialize<ExportClientDto[]>(products, "Clients");
		}

        public static string ExportProductsWithMostClients(InvoicesContext context, int nameLength)
        {
            var products = context.Products
                .Where(p => p.ProductsClients.Any(pc => pc.Client.Name.Length >= nameLength))
				.Select(p => new
				{
					Name = p.Name,
					Price = p.Price,
					Category = p.CategoryType.ToString(),
					Clients = p.ProductsClients
					.Where(pc => pc.Client.Name.Length >= nameLength)
					.Select(pc => new
					{
						Name = pc.Client.Name,
						NumberVat = pc.Client.NumberVat
					})
					.OrderBy(c => c.Name)
					.ToArray()
				})
				.OrderByDescending(p => p.Clients.Length)
				.ThenBy(p => p.Name)
				.Take(5)
				.ToArray();


			return JsonConvert.SerializeObject(products, Formatting.Indented);
		}
    }
}