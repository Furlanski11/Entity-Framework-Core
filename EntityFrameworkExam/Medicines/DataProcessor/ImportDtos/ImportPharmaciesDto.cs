using Medicines.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Medicines.DataProcessor.ImportDtos
{
	[XmlType("Pharmacy")]
	public class ImportPharmaciesDto
	{
		[XmlAttribute("non-stop")]
		[Required]
		public string IsNonStop { get; set; } = null!;

		[XmlElement("Name")]
		[Required]
		[MinLength(2)]
		[MaxLength(50)]
		public string Name { get; set; } = null!;

		[XmlElement("PhoneNumber")]
		[Required]
		[MinLength(14)]
		[MaxLength(14)]
		[RegularExpression("\\(\\d{3}\\) \\d{3}-\\d{4}")]
		public string PhoneNumber { get; set; } = null!;

		[XmlArray("Medicines")]
		public ImportMedicineDto[] Medicines { get; set; } = null!;
    }

	[XmlType("Medicine")]
	public  class ImportMedicineDto
	{
		[XmlAttribute("category")]
		
		[Required]
		[Range(0, 4)]
        public int Category { get; set; }

		[XmlElement("Name")]
		[Required]
		[MinLength(3)]
		[MaxLength(150)]
		public string Name { get; set; } = null!;
		
		[XmlElement("Price")]
		[Required]
		[Range(0.01, 1000.00)]
		public decimal Price { get; set; }

		[XmlElement("ProductionDate")]
		[Required]
		public string ProductionDate { get; set; } 

		[XmlElement("ExpiryDate")]
		[Required]
		public string ExpiryDate { get; set; } 

		[XmlElement("Producer")]
		[Required]
		[MinLength(3)]
		[MaxLength(100)]
        public string Producer { get; set; } = null!;

    }
}
