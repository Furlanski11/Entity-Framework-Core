namespace Invoices.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
	using System.Globalization;
	using System.Text;
	using Invoices.Data;
	using Invoices.Data.Models;
	using Invoices.Data.Models.Enums;
	using Invoices.Data.Utilities;
	using Invoices.DataProcessor.ImportDto;
	using Newtonsoft.Json;

	public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedClients
            = "Successfully imported client {0}.";

        private const string SuccessfullyImportedInvoices
            = "Successfully imported invoice with number {0}.";

        private const string SuccessfullyImportedProducts
            = "Successfully imported product - {0} with {1} clients.";


        public static string ImportClients(InvoicesContext context, string xmlString)
        {
            XmlHelper xmlHelper = new();

           ImportClientsDto[] clientsDtos = xmlHelper.Deserialize<ImportClientsDto[]>(xmlString, "Clients");

            List<Client> clients = new List<Client>();
            StringBuilder sb = new StringBuilder();

            foreach (var client in clientsDtos)
            {
                if(!IsValid(client))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client currentClient = new Client
                {
                    Name = client.Name,
                    NumberVat = client.NumberVat,
                };

                foreach (var address in client.Addresses)
                {
                    if (!IsValid(address))
                    {
						sb.AppendLine(ErrorMessage);
						continue;
                    }

                    Address currentAddress = new Address
                    {
                        StreetName = address.StreetName,
                        StreetNumber = address.StreetNumber,
                        PostCode = address.PostCode,
                        City = address.City,
                        Country = address.Country,
                    };
                    currentClient.Addresses.Add(currentAddress);
                }
                clients.Add(currentClient);
                sb.AppendLine(string.Format(SuccessfullyImportedClients, client.Name));

            }
            context.Clients.AddRange(clients);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }


        public static string ImportInvoices(InvoicesContext context, string jsonString)
        {
            ImportInvoicesDto[] invoicesDtos = JsonConvert.DeserializeObject<ImportInvoicesDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();
            HashSet<Invoice> invoices = new();
			int[] clientsIds = context.Clients.Select(c => c.Id).ToArray();
			foreach (var invoiceDto in invoicesDtos)
            {

				DateTime issueDate = DateTime.ParseExact(invoiceDto.IssueDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
				DateTime dueDate = DateTime.ParseExact(invoiceDto.DueDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

                if(issueDate > dueDate)
                {
					sb.AppendLine(ErrorMessage);
					continue;
				}

				if (!IsValid(invoiceDto) || !clientsIds.Contains(invoiceDto.ClientId))
                {
					sb.AppendLine(ErrorMessage);
					continue;
				}

				Invoice invoice = new Invoice
                {
                    Number = invoiceDto.Number,
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    Amount = invoiceDto.Amount,
                    CurrencyType = (CurrencyType)invoiceDto.CurrencyType,
                    ClientId = invoiceDto.ClientId,
                };
                invoices.Add(invoice);
                sb.AppendLine(string.Format(SuccessfullyImportedInvoices, invoice.Number));
            }
            context.Invoices.AddRange(invoices);
            context.SaveChanges();
            return sb.ToString().TrimEnd();

        }

        public static string ImportProducts(InvoicesContext context, string jsonString)
        {

            ImportProductsDto[] productsDtos = JsonConvert.DeserializeObject<ImportProductsDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Product> products = new List<Product>();

			int[] clientsIds = context.Clients.Select(c => c.Id).ToArray();

            foreach (var productDto in productsDtos)
            {
                if (!IsValid(productDto))
                {
					sb.AppendLine(ErrorMessage);
					continue;
				}
				
                Product product = new Product
				{
					Name = productDto.Name,
					Price = productDto.Price,
					CategoryType = (CategoryType)productDto.CategoryType,

				};
                
				foreach (var clientId in productDto.Clients.Distinct())
                {
                    if (!clientsIds.Contains(clientId))
                    {
						sb.AppendLine(ErrorMessage);
						continue;
					}

                    product.ProductsClients.Add(new ProductClient
                    {
                        ClientId = clientId,
                    });
                }
                products.Add(product);
				sb.AppendLine(string.Format(SuccessfullyImportedProducts, productDto.Name, product.ProductsClients.Count));

			}
            context.Products.AddRange(products);
            context.SaveChanges();
            return sb.ToString().TrimEnd();

        }

        public static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    } 
}
