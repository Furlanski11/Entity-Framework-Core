using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Trucks.DataProcessor.ImportDto
{
	[XmlType("Despatcher")]
	public class ImportDespatchersDto
	{
        [Required]
		[MinLength(2)]
		[MaxLength(40)]
		public string Name { get; set; } = null!;

        public string? Position { get; set; }

		[XmlArray("Trucks")]
		public ImportTruckDto[] Trucks { get; set; } = null!;
    }

	[XmlType("Truck")]
	public class ImportTruckDto
	{
		[MinLength(8)]
		[MaxLength(8)]
		[Required]
		[RegularExpression(@"^[A-Z]{2}\d{4}[A-Z]{2}$")]
		public string RegistrationNumber { get; set; } = null!;

		[Required]
		[MinLength(17)]
		[MaxLength(17)]
		public string VinNumber { get; set; } = null!;

		[Range(950, 1420)]
        public int TankCapacity { get; set; }

		[Range(5000, 29000)]
        public int CargoCapacity { get; set; }

		[Range(0, 3)]
        public int CategoryType { get; set; }

		[Range(0, 4)]
        public int MakeType { get; set; }
    }
}
