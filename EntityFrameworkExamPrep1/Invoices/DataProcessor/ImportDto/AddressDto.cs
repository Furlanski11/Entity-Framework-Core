using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Invoices.DataProcessor.ImportDto
{
	[XmlType("Address")]
	public class AddressDto
	{
		[XmlElement("StreetName")]
		[Required]
		[MinLength(10)]
		[MaxLength(20)]
        public string StreetName { get; set; }

		[XmlElement("StreetNumber")]
		[Required]
        public int StreetNumber { get; set; }

		[XmlElement("PostCode")]
		[Required]
        public string PostCode { get; set; }
		[XmlElement("City")]
		[Required]
		[MinLength(5)]
		[MaxLength(15)]
        public string City { get; set; }

        [XmlElement("Country")]
		[Required]
		[MinLength(5)]
		[MaxLength(15)]
        public string Country { get; set; }
    }
}
