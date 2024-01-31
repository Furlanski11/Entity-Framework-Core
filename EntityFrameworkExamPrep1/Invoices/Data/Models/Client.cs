using System.ComponentModel.DataAnnotations;

namespace Invoices.Data.Models
{
	public class Client
	{
		[Key]
        public int Id { get; set; }

		[Required]
		[Range(10, 25)]
		public string Name { get; set; } = null!;

		[Required]
		[Range (10, 15)]
		public string NumberVat { get; set; } = null!;

        public ICollection<Invoice> Invoices { get; set; } = new HashSet<Invoice>();

        public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();

		public ICollection<ProductClient> ProductsClients { get; set; } = new HashSet<ProductClient>();
    }
}