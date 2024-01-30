using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProductShop.DTOs.Export
{
	[XmlType("User")]
	public class ExportSoldProductsDTO
	{
		[XmlElement("firstName")]
		public string firstName { get; set; }
		[XmlElement("lastName")]
		public string lastName { get; set; }
		[XmlArray("soldProducts")]
		public SoldProductDTO[] soldProducts { get; set; }
	}

	[XmlType("Product")]
	public class SoldProductDTO
	{
		[XmlElement("name")]
		public string name { get; set; }
		[XmlElement("price")]
		public decimal price { get; set; }
	}
}
